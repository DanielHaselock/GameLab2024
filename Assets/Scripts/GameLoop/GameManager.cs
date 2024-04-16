using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audio;
using Cinemachine;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using Networking.Behaviours;
using UnityEditor.Rendering.Universal.ShaderGUI;
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
            Cutscene,
            ActiveLevel,
            SpawnBoss,
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

        private bool cutscenePlayed = false;
        private int _cutscenesCompleted = 0;
        
        public event Action<bool> OnPauseStatusChanged;
        public GameState CurrentGameState { get; private set; }
        public static event Action<GameState> OnGameStateChanged;

        [Networked]
        public bool BossSpawning { get; set; }
        
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
        private void Initialise()
        {
            IsPausable = true; //for testing
            var gameUIObj = Instantiate(Resources.Load<GameObject>("GameUI"), transform);
            _gameUI = gameUIObj.GetComponentInChildren<GameUI>();
            _gameUI.RegisterLoadToMainMenu(LoadMainMenu);
            _gameUI.RegisterLoadNextLevel(LoadNextLevel);
            _gameUI.OnCutsceneCompleted += OnCutsceneCompleted;
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
            Initialise();
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
            
            if(timeLeft.TotalSeconds <= 10)
                AudioManager.Instance.PlaySFX(AudioConstants.Clock);
            
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
                case GameState.Cutscene:
                    PlayCutscene();
                    break;
                case GameState.ActiveLevel:
                    StartLevel();
                    break;
                case GameState.SpawnBoss:
                    SpawnBoss();
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

        private void PlayCutscene()
        {
            if (!Runner.IsServer)
                return;
            
            _cutscenesCompleted = 0;
            RPC_PlayCutsceneForAll();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayCutsceneForAll()
        {
            _gameUI.PlayCutscene();
        }

        private void OnCutsceneCompleted()
        {
            if (Runner.IsServer)
            {
                _cutscenesCompleted += 1;
                CheckIfCutsceneCompletedForAll();
            }
            else
            {
                RPC_CutsceneCompleted();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_CutsceneCompleted()
        {
            _cutscenesCompleted += 1;
            CheckIfCutsceneCompletedForAll();
        }
        
        private void CheckIfCutsceneCompletedForAll()
        {
            if (_cutscenesCompleted < NetworkManager.Instance.ConnectedPlayers.Count)
                return;
            RPC_HideCutsceneGUI();
            UpdateGameState(GameState.ActiveLevel);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_HideCutsceneGUI()
        {
            _gameUI.HideCutscenePlayer();
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
            Cursor.visible = pause;
            Cursor.lockState = pause?CursorLockMode.None : CursorLockMode.Locked;
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
            _gameUI.SetBossHealth(false, 0);
            bool result = await NetworkManager.Instance.LoadSceneNetworked(LevelManager.LevelSceneIndx, false);
            if (!result)
            {
                Debug.LogError("Failed to load level");
                return;
            }

            //quick-hack to load a scene and then play cutscene :p
            //I know I'm loadin the same scene twice but fk it.
            if (NetworkManager.Instance.ShowCutsceneBeforeFirstLevel && !cutscenePlayed)
            {
                cutscenePlayed = true;
                UpdateGameState(GameState.Cutscene);
                return;
            }
            
            AudioManager.Instance.PlayBackgroundMusic(LevelManager.BGMKey);
            AudioManager.Instance.PlayAmbiance(LevelManager.AmbianceKey);
            
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

            await Task.Delay(100);
            
            foreach (var spawner in FindObjectsOfType<GenericEnemySpawner>())
            {
                var no = Runner.Spawn(spawner.Prefab, spawner.transform.position, spawner.transform.rotation);
                no.GetComponent<NetworkRigidbody3D>().Teleport(spawner.transform.position);
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
            // hacky but it will do
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            
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

        
        
        //-----------------------------------------------------------
        // Boss Spawn
        //-----------------------------------------------------------
        private void SpawnBoss()
        {
            BossSpawning = true;
            _timer.StopTimer();
            RPC_ShowBossSpawnVisual();
            StartCoroutine(TeleportPlayerToArena());
        }

        IEnumerator TeleportPlayerToArena()
        {
            yield return new WaitForSeconds(1.5f);
            var bossBattlePlayerStarts = FindObjectsOfType<PlayerBossFightStartLocator>();
            foreach (var player in _players)
            {
                var loc = bossBattlePlayerStarts[_players.IndexOf(player)];
                player.MarkForTeleport(loc.SpawnPosition, loc.SpawnRotation);
            }
        }

        private void LockArena()
        {
            //todo 
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private async void RPC_ShowBossSpawnVisual()
        {
            _gameUI.ShowGameTimer(false);
            _gameUI.HideObjectives(); // no longer needed
            AudioManager.Instance.PlaySFX(AudioConstants.BossSummon);
            await Task.Delay(1500);
            LockArena();
            AlterCollector collectible = FindObjectOfType<AlterCollector>();
            var alterCamObjC = collectible.AlterCamClose;
            var alterCamObjF = collectible.AlterCamFar;
            var alterGraphic = collectible.AlterGraphic;
            Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition()
            {
                m_Style = CinemachineBlendDefinition.Style.Cut
            };
            
            var wiggler = alterGraphic.GetComponentInChildren<Wiggle>();
            alterCamObjC.SetActive(true);
            alterCamObjF.SetActive(false);
            
            wiggler.enabled = true;
            StartCoroutine(BossSpawnCoroutine());
            AudioManager.Instance.PlaySFX(LevelManager.BossSFXKey);
        }

        IEnumerator BossSpawnCoroutine()
        {
            yield return new WaitForSeconds(3);
            if (!Runner.IsServer)
                yield break;
            SpawnBossInstanceOnServer();
            yield return new WaitForEndOfFrame();
            RPC_UpdateMusic();
            yield return new WaitForSeconds(3);
            RPC_SpawnExplosionAndHideAlterGraphic();
            BossSpawning = false;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateMusic()
        {
            AudioManager.Instance.PlayBackgroundMusic(LevelManager.BossMusic);
        }
        
        private void SpawnBossInstanceOnServer()
        {
            if (LevelManager.BossToSpawn.Equals(default))
            {
                UpdateGameState(GameState.Win);
                return;
            }
            var pos = Vector3.zero;
            var rot = Quaternion.identity;
            var spawner = FindObjectOfType<BossSpawner>();
            if (spawner != null)
            {
                pos = spawner.SpawnPos;
                rot = spawner.SpawnRotation;
            }
            Runner.Spawn(LevelManager.BossToSpawn, pos, rot);
            RPC_SpawnFX();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnFX()
        {
            AlterCollector collectible = FindObjectOfType<AlterCollector>();
            Instantiate(collectible.SpawnEffect, collectible.SpawnFXTRF.position, Quaternion.identity);
            Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition()
            {
                m_Style = CinemachineBlendDefinition.Style.EaseInOut,
                m_Time = 0.5f,
            };
            var alterGraphic = collectible.AlterGraphic;
            var alterCamClose = collectible.AlterCamClose;
            var alterCamFar = collectible.AlterCamFar;
            
            alterCamClose.SetActive(false);
            alterCamFar.SetActive(true);
            alterGraphic.gameObject.SetActive(false);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnExplosionAndHideAlterGraphic()
        {
            Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend = new CinemachineBlendDefinition()
            {
                m_Style = CinemachineBlendDefinition.Style.Cut
            };
            AlterCollector collectible = FindObjectOfType<AlterCollector>();
            collectible.transform.parent.gameObject.SetActive(false);
        }
        
        //-----------------------------------------------------------
        // Boss Spawn End
        //-----------------------------------------------------------
        
        
        
        public void UpdateScore(int player, string enemyKey)
        {
            if(LevelManager.ScoreMap.ContainsKey(enemyKey))
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

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
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
            {
                AudioManager.Instance.PlaySFX(AudioConstants.ObjectiveComplete, syncNetwork:true);
                objectivesMap.Remove(key);
            }
            
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
                UpdateGameState(GameState.SpawnBoss);
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
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            AudioManager.Instance.PlayBackgroundMusic(AudioConstants.PostRound);
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
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            AudioManager.Instance.PlayBackgroundMusic("post_round");
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