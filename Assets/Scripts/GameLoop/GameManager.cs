using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Networking.Behaviours;
using Utils;
using UnityEngine.EventSystems;

namespace GameLoop
{
    public class GameManager : NetworkBehaviour
    {
        private static string BOSS_KEY = "boss";
        
        //best practice = keeping these in seperate script
        public enum GameState
        {
            MainMenu,
            ActiveLevel,
            Lost,
            Win
        }
        
        public static GameManager instance;
        
        [SerializeField] private int maxLevels;
        [SerializeField] private RewardsMap rewardsMap;
        
        private ChangeDetector _change;
        private Dictionary<string, Objective> objectivesMap;
        private Dictionary<string, Objective> objectivesGUIData;
        private int _currentLevel = 1;
        private TimeSpan _timeLeft;
        private GameUI _gameUI;
        private NetworkTimer _timer;
        private List<Player> _players;
        private List<NetworkObject> _enemies;
        private RewardManager _rewardManager;
        
        public event Action<bool> OnPauseStatusChanged;
        public GameState CurrentGameState { get; private set; }
        public static event Action<GameState> OnGameStateChanged;

        [Networked] public bool gameStarted { get; private set; } = false;
        [Networked] public bool bossDefeated { get; private set; }
        [Networked] public bool IsPaused { get; private set; }
        [Networked] public bool IsPausable { get; private set; }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        private void OnDestroy()
        {
            if(_gameUI == null)
                return;
            _gameUI.DeRegisterLoadToMainMenu(LoadMainMenu);
            _gameUI.DeRegisterLoadNextLevel(LoadNextLevel);
        }
        
        public void ResetManager()
        {
            if(objectivesMap != null)
                objectivesMap.Clear();
            
            if(objectivesGUIData != null)
                objectivesGUIData.Clear();
            
            if (_players != null)
            {
                foreach (var player in _players)
                {
                    if(player != null)
                        Runner.Despawn(player.GetComponent<NetworkObject>());
                }
                _players.Clear();
            }

            if (_enemies != null)
            {
                foreach (var enemy in _enemies)
                {
                    if(enemy != null)
                        Runner.Despawn(enemy);
                }
                _enemies.Clear();
            }
        }
        
        // Creates the Pause UI in scene
        void Start()
        {
            IsPausable = true; //for testing
            var gameUIObj = Instantiate(Resources.Load<GameObject>("GameUI"), transform);
            _gameUI = gameUIObj.GetComponentInChildren<GameUI>();
            _gameUI.RegisterLoadToMainMenu(LoadMainMenu);
            _gameUI.RegisterLoadNextLevel(LoadNextLevel);
            _rewardManager = new RewardManager();
            
            if (EventSystem.current == null)
                Instantiate(Resources.Load("EventSystem"));
        }

        private void Update()
        {
            if(!Runner.IsServer)
                return;
            if(!gameStarted)
                return;

            int downedPlayers = 0;
            foreach(var player in _players)
            {
                if (player != null && player.PlayerDowned)
                    downedPlayers+=1;
            }

            if (downedPlayers >= _players.Count)
            {
                gameStarted = false;
                StartCoroutine(DelayedGameLost());
            }
        }
        
        public override void Spawned()
        {
            objectivesMap = new Dictionary<string, Objective>();
            _timer = GetComponentInChildren<NetworkTimer>();
            _timer.OnTimerTick += TimerTick;
            _change = GetChangeDetector(ChangeDetector.Source.SimulationState);
            if (!Runner.IsServer)
                return;
            UpdateGameState(GameState.ActiveLevel);
        }
        
        private void TimerTick(TimeSpan timeLeft)
        {
            this._timeLeft = timeLeft;
            _gameUI.UpdateTimerText(timeLeft);
            if (gameStarted && timeLeft.TotalSeconds <=0)
            {
                gameStarted = false;
                StartCoroutine(DelayedGameLost());
            }
        }

        private IEnumerator DelayedGameLost()
        {
            yield return new WaitForSeconds(1);
            UpdateGameState(GameState.Lost);
        }

        public override void Render()
        {
            base.Render();
            if (_change == null)
                return;
            foreach (var change in _change.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(IsPaused):
                        OnPauseStatusChanged?.Invoke(IsPaused);
                        break;
                }
            }
        }
        
        public void UpdateGameState(GameState newState)
        {
            if(!Runner.IsServer)
                return;
            CurrentGameState = newState;
            switch (newState)
            {
                case GameState.MainMenu:
                    LoadMainMenu();
                    break;
                case GameState.ActiveLevel:
                    StartLevel();
                    break;
                case GameState.Win:
                    OnGameWon();
                    break;
                case GameState.Lost:
                    OnGameLost();
                    break;
            }
            OnGameStateChanged?.Invoke(newState);
        }

        private void LoadMainMenu()
        {
            RPC_LoadMainMenuOnClient();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_LoadMainMenuOnClient()
        {
            NetworkManager.Instance.ResetGame();
        }
        
        private void DoPause(bool pause)
        {
            if (!IsPausable)
            {
                return;
            }

            IsPaused = pause;
        }

        public void Pause(bool pause)
        {
            Debug.Log($"Pause {pause}");
            if (Runner.IsServer)
                DoPause(pause);
            else
                RPC_Pause(pause);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_Pause(bool pause)
        {
            DoPause(pause);
        }

        private async void StartLevel()
        {
            if (Runner == null)
                return;
            if (!Runner.IsServer)
            {
                Debug.LogError("Cannot call StartLevel on client");
                return;
            }
            IsPausable = true;
            LevelManager.LoadLevel(_currentLevel);
            bool result = await NetworkManager.Instance.LoadSceneNetworked(LevelManager.LevelSceneIndx, false);
            if (!result)
            {
                Debug.LogError("Failed to load level");
                return;
            }
            var defPos = new List<Vector3> { new Vector3(0, 2, 0), new Vector3(0, 4, 0) };
            var defRot = new List<Quaternion> { Quaternion.identity, Quaternion.identity };
            var spawnPositions = new List<Vector3>();
            var spawnRotations = new List<Quaternion>();
            foreach (var spawnpoint in FindObjectsOfType<PlayerSpawnPoint>())
            {
                spawnPositions.Add(spawnpoint.SpawnPosition);
                spawnRotations.Add(spawnpoint.SpawnRotation);
            }
            if (spawnPositions.Count < 0 || spawnRotations.Count < 0)
            {
                spawnPositions = defPos;
                spawnRotations = defRot;
            }
            _enemies = new List<NetworkObject>();
            NetworkManager.Instance.SpawnPlayers(spawnPositions, spawnRotations);
            foreach (var spawner in FindObjectsOfType<GenericEnemySpawner>())
            {
                var no = Runner.Spawn(spawner.Prefab, spawner.transform.position, spawner.transform.rotation);
                _enemies.Add(no);
            }
            //change objectives
            
            string levelPath = LevelManager.LevelDataPath;
            objectivesMap = new Dictionary<string, Objective>();
            foreach (var objectiveData in LevelManager.Objectives)
            {
                objectivesMap.Add(objectiveData.key, new Objective(objectiveData));
            }
            bossDefeated = false;
            InitialiseObjectiveTexts();
            RPC_LoadLevelObjectivesOnClient(levelPath);
            //start game timer
            NetworkLogger.Log("Starting timer");
            _timer.StartTimer(TimeSpan.FromSeconds(LevelManager.LevelTime));
            _gameUI.ShowGameTimer(true);
            _players = FindObjectsOfType<Player>().ToList();
            foreach (var player in _players)
            {
                int indx = _rewardManager.GetRewardIndex(player.PlayerId);
                player.SetWeapon(indx);
            }
            InitialiseScores();
            RPC_InitialiseScoreOnClients();
            gameStarted = true;
        }

        private void InitialiseScores()
        {
            var list = new List<int>();
            var nicknames = new Dictionary<int, string>();
            foreach (var player in _players)
            {
                list.Add(player.PlayerId);
                nicknames.Add(player.PlayerId, NetworkManager.Instance.GetPlayerNickNameById(player.PlayerId));
            }
            ScoreManager.Initialise(list);
            _gameUI.InitialiseScores(ScoreManager.Score, nicknames);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_InitialiseScoreOnClients()
        {
            if(Runner.IsServer)
                return;
            _players = FindObjectsOfType<Player>().ToList();
            InitialiseScores();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_LoadLevelObjectivesOnClient(string levelPath)
        {
            if (Runner.IsServer)
                return;
            LevelManager.LoadLevelObjectivesFrom(levelPath);
            objectivesMap = new Dictionary<string, Objective>();
            foreach (var objectiveData in LevelManager.Objectives)
            {
                objectivesMap.Add(objectiveData.key, new Objective(objectiveData));
            }
            InitialiseObjectiveTexts();
        }

        private void InitialiseObjectiveTexts()
        {
            if (objectivesGUIData == null)
                objectivesGUIData = new Dictionary<string, Objective>();
            foreach (var kv in objectivesMap)
            {
                objectivesGUIData.Add(kv.Key, new Objective(kv.Value.Data));
            }
            UpdateGameUI();
        }

        private void SpawnBoss()
        {
            if (LevelManager.BossToSpawn.Equals(default))
            {
                UpdateGameState(GameState.Win);
                return;
            }
            var pos = Vector3.zero;
            var rot = Quaternion.identity;
            var spawner = FindObjectOfType<BossSpawner>();
            if (spawner == null)
            {
                pos = spawner.SpawnPos;
                rot = spawner.SpawnRotation;
            }
            Runner.Spawn(LevelManager.BossToSpawn, pos, rot);
        }

        public void UpdateScore(int player, string enemyKey)
        {
            ScoreManager.UpdateScore(player, LevelManager.ScoreMap[enemyKey]);
            _gameUI.UpdateScore(player, ScoreManager.Score[player]);
            if (enemyKey.Equals(BOSS_KEY))
            {
                StartCoroutine(DelayedGameWin());
                return;
            }
            if (Runner.IsServer)
            {
                RPC_UpdateScoreOnClient(player, enemyKey);
            }
        }
        
        private IEnumerator DelayedGameWin()
        {
            yield return new WaitForSeconds(1);
            UpdateGameState(GameState.Win);
        }
        
        public void RPC_UpdateScoreOnClient(int player, string enemyKey)
        {
            if (Runner.IsServer)
                return;
            UpdateScore(player, enemyKey);
        }
        
        public void RaiseObjective(string key)
        {
            if (Runner == null)
                return;
            if (!Runner.IsServer)
                return;
            if (!objectivesMap.ContainsKey(key))
                return;
            Debug.Log("Objective Raised!!");
            var objective = objectivesMap[key];
            objective.UpdateObjective();
            objectivesGUIData[key].SetValue(objective.Current);
            RPC_UpdateObjectiveData(key, objective.Current);
            if (objective.IsCompleted)
                objectivesMap.Remove(key);
            TryUpdateGameState();
            UpdateGameUI();
        }
        
        private void TryUpdateGameState()
        {
            if(!gameStarted)
                return;
            if(!CurrentGameState.Equals(GameState.ActiveLevel))
                return;
            if (bossDefeated)
                return;
            if (objectivesMap.Count <= 0)
            {
                SpawnBoss();
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateObjectiveData(string key, int value)
        {
            if (Runner.IsServer)
                return;
            objectivesGUIData[key].SetValue(value);
            UpdateGameUI();
        }

        private void UpdateGameUI()
        {
            if (_gameUI == null)
                return;
            _gameUI.UpdateLevelObjectives(objectivesGUIData);
        }
        
        private void OnGameWon()
        {
            _timer.StopTimer();
            gameStarted = false;
            _gameUI.ShowGameTimer(false);
            LevelManager.LevelComplete(true, _timeLeft);
            _rewardManager.Calculate(ScoreManager.Score, rewardsMap);
            ResetManager();
            _gameUI.ShowWinGameUI(true, true);
            RPC_ShowWinScreenOnClients();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ShowWinScreenOnClients()
        {
            if(Runner.IsServer)
                return;
            ResetManager();
            _gameUI.ShowWinGameUI(true, false);
        }
        
        private void LoadNextLevel()
        {
            if(!Runner.IsServer)
                return;
            if (_currentLevel < maxLevels)
            {
                ResetManager();
                _currentLevel+=1;
                UpdateGameState(GameState.ActiveLevel);
                _gameUI.ShowWinGameUI(false, false);
                RPC_HideWinScreenOnClients();
            }
            else
            {
                //TODO: Show Final Screen
                Debug.Log("Game Actually Over!!");
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_HideWinScreenOnClients()
        {
            if(Runner.IsServer)
                return;
            _gameUI.ShowWinGameUI(false, false);
        }
        
        private void OnGameLost()
        {
            Debug.Log("Game Lost");
            _timer.StopTimer();
            gameStarted = false;
            _gameUI.ShowGameTimer(false);
            LevelManager.LevelComplete(false, _timeLeft);
            RPC_ShowLoseScreenOnClients();
            _gameUI.ShowLostGameUI(true);
            ResetManager();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ShowLoseScreenOnClients()
        {
            if(Runner.IsServer)
                return;
            _gameUI.ShowLostGameUI(true);
            ResetManager();
        }
        
        //so main logic is as such, when an enemy dies,
        //it will update the objective. if it matches,
        //then the objective is updated. once objective is met,
        //it is removed from list. Once list is empty,
        //all objectives have been met and we can spawn the boss.
        //Once boss is defeated, triggers gamestate change and level ends.
    }
}