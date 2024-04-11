using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accessibility;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using Networking.Data;
using Networking.UI;
using Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Networking.Behaviours
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPrefabRef _synchedDataPrefab;

        private NetworkProperties _networkPropertiesRef;
        private NetworkRunner _runner;
        private PlayerNetworkedActions _playerInput = null;
        private List<PlayerRef> _connectedPlayers;
        private NetworkUI _connectionUI;

        private Dictionary<string, List<Action<NetworkEvent>>> _generalNetworkMessages;

        private bool _connectedToLobby = false;
        private bool _gameStarted = false;
        private string _sessionUserNickName;

        //server only
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharactersOnServer = new Dictionary<PlayerRef, NetworkObject>();

        private NetworkManagerSynchedHelper _netSynchedHelper;
        public List<SessionInfo> AvailableSessions { get; private set; }
        public List<PlayerRef> ConnectedPlayers => _connectedPlayers;
        public bool IsServer => _runner && _runner.IsServer;

        /// <summary>
        /// Invoked when session list is updated,
        /// use AvailableSessions property to get updated list__
        /// </summary>
        public Action OnAvailableSessionsListUpdated;
        /// <summary>
        /// Invoked when connected to global lobby
        /// </summary>
        public Action OnConnectedToLobby;
        /// <summary>
        /// Invoked when a player joins a session
        /// <param name="playerId">integer Id of player</param>
        /// </summary>
        public Action<int> OnPlayerConnected;
        /// <summary>
        /// Invoked when a player leaves a session
        /// <param name="playerId">integer Id of player</param>
        /// </summary>
        public Action<int> OnPlayerDisconnected;

        /// <summary>
        /// Invoked On Server, when all the player controllers are spawned and ready to start game
        /// </summary>
        public Action OnGameStarted;
        /// <summary>
        /// Invoked On Server, when GameOver method is invoked
        /// </summary>
        public Action OnGameOver;
        
        private static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<NetworkManager>();

                return _instance;
            }
        }

        public bool ShowCutsceneBeforeFirstLevel => _networkPropertiesRef.ShowCutscene;
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            _connectedPlayers = new List<PlayerRef>();
            _networkPropertiesRef = Resources.Load<NetworkProperties>(Constants.NETWORK_OBJ_SO_NAME);
            _connectionUI = Instantiate(Resources.Load<GameObject>(Constants.NETWORK_UI)).GetComponent<NetworkUI>();
            _connectionUI.transform.parent = transform;
            _connectionUI.Initialise(this);
            AvailableSessions = new List<SessionInfo>();
            ConnectToLobby();
            RegisterToGeneralNetworkEvents("loading_screen", LoadingScreenUpdate);
        }

        private void LoadingScreenUpdate(NetworkEvent data)
        {
            if(_runner.IsServer)
                return;
            
            if(!data.EventName.Equals("loading_screen"))
                return;
            
            if(data.EventData.Equals("1"))
                _connectionUI.ShowLoadingScreen(true);
            else
            {
                _connectionUI.ShowLoadingScreen(false);
            }
        }
        
        public async Task SmartConnect(float decisionDelayTime = 1f)
        {
            _connectionUI.ShowWait(true);
            if (!_connectedToLobby)
            {
                var wait = true;
                var func = new Action(() =>
                {
                    wait = false;
                });
                OnAvailableSessionsListUpdated += func;
                AvailableSessions.Clear();
                await ConnectToLobby();
                while (wait)
                    await Task.Yield();
                OnAvailableSessionsListUpdated -= func;
            }

            // start a game if not found, else join one.
            if (AvailableSessions.Count <= 0)
            {
                await Task.Delay(Mathf.RoundToInt(decisionDelayTime * 1000));
                if (AvailableSessions.Count <= 0)
                {
                    NetworkLogger.Log("Decision: Host");
                    CreateNewSession();
                }
                else
                    JoinRandomSession();
            }
            else
            {
                NetworkLogger.Log("Decision: Join Session");
                JoinRandomSession();
            }
        }

        private void JoinRandomSession()
        {
            var selectedSession = AvailableSessions[Random.Range(0, AvailableSessions.Count)];
            if (selectedSession == default)
                return;
            JoinSession(selectedSession.Name);
        }

        private void CreateNewSession()
        {
            HostSession($"Game_{Guid.NewGuid().ToString()}");
        }

        private async Task ConnectToLobby()
        {
            _connectedToLobby = false;
            TryInitNetworkRunner();
            var res = await _runner.JoinSessionLobby(SessionLobby.ClientServer, Constants.GAME_LOBBY);
            if (!res.Ok)
                Destroy(_runner);
            else
            {
                _connectedToLobby = true;
                NetworkLogger.Log("Connected to Lobby!");
                OnConnectedToLobby?.Invoke();
            }
        }

        private async void LaunchSession(String sessionName, GameMode mode)
        {
            TryInitNetworkRunner();
            var scene = SceneRef.FromIndex(0);
            var sceneInfo = new NetworkSceneInfo();

            if (scene.IsValid)
            {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }

            var res = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = sessionName,
                Scene = sceneInfo,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                PlayerCount = _networkPropertiesRef.MaxPlayers,
                SessionProperties = new Dictionary<string, SessionProperty>()
            });

            if (!res.Ok)
            {
                Destroy(_runner);
            }
        }

        private void TryInitNetworkRunner()
        {
            if (_runner == null)
            {
                NetworkLogger.Log("Creating new Runner");
                _runner = gameObject.AddComponent<NetworkRunner>();
                gameObject.AddComponent<RunnerSimulatePhysics3D>();
                _runner.ProvideInput = true;
            }
        }

        public void SetSessionUserNickName(string nickName)
        {
            _sessionUserNickName = nickName;
        }

        /// <summary>
        /// Returns a Player Reference, Using the PlayerId
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public PlayerRef GetPlayerRefById(int playerId)
        {
            return ConnectedPlayers.SingleOrDefault(a => a.PlayerId.Equals(playerId));
        }

        /// <summary>
        /// Returns the NickName of the Player, Using PlayerId
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public String GetPlayerNickNameById(int playerId)
        {
            return _netSynchedHelper.GetPlayerNickNameById(playerId);
        }
        
        /// <summary>
        /// Host a Game Session
        /// </summary>
        /// <param name="sessionName"></param>
        public void HostSession(String sessionName)
        {
            LaunchSession(sessionName, GameMode.Host);
        }

        /// <summary>
        /// Join a Game Session
        /// </summary>
        /// <param name="sessionName"></param>
        public void JoinSession(String sessionName)
        {
            LaunchSession(sessionName, GameMode.Client);
        }

        /// <summary>
        /// Invokes GameOver, will internally invoke OnGameOver Event
        /// </summary>
        public void GameOver()
        {
            if (!_runner.IsServer)
            {
                NetworkLogger.Error("Game Over can only be invoked by Server");
                return;
            }
            OnGameOver?.Invoke();
        }
        public void StartGame() {  
            if (!_runner.IsServer)
            {
                NetworkLogger.Error("Start Game can only be invoked by Server");
                return;
            }
            foreach (var spawnInfo in _networkPropertiesRef.NetworkObjectsToSpawnOnGameStart)
            {
                _runner.Spawn(spawnInfo.ObjectToSpawn, spawnInfo.Position, Quaternion.Euler(spawnInfo.Rotation));
            }
            _gameStarted = true;
            _connectionUI.ShowWait(false);
            OnGameStarted?.Invoke();
        }
        
        public void RegisterToGeneralNetworkEvents(string eventName, Action<NetworkEvent> action)
        {
            if (_generalNetworkMessages == null)
                _generalNetworkMessages = new Dictionary<string, List<Action<NetworkEvent>>>();

            if (_generalNetworkMessages.ContainsKey(eventName))
                _generalNetworkMessages[eventName].Add(action);
            else
                _generalNetworkMessages.Add(eventName, new List<Action<NetworkEvent>>() { action });
        }

        public void DeRegisterToGeneralNetworkEvents(string eventName, Action<NetworkEvent> action)
        {
            if (_generalNetworkMessages == null)
                return;

            if (_generalNetworkMessages.ContainsKey(eventName))
                _generalNetworkMessages[eventName].Remove(action);
        }

        public void SendGlobalSimpleNetworkMessage(NetworkEvent data)
        {
            _netSynchedHelper.SendGlobalSimpleNetworkMessage(data);
        }

        private void OnSimpleNetworkEventReceived(NetworkEvent data)
        {
            if (_generalNetworkMessages == null)
                return;
            if (!_generalNetworkMessages.ContainsKey(data.EventName))
                return;
            foreach (var evnt in _generalNetworkMessages[data.EventName])
            {
                evnt?.Invoke(data);
            }
        }

        #region Callbacks

        public void OnObjectExitAOI(Fusion.NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(Fusion.NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public async void OnPlayerJoined(Fusion.NetworkRunner runner, PlayerRef player)
        {
            //ignore these events when in game-lobby.
            if (runner.SessionInfo.Name.Equals(Constants.GAME_LOBBY))
            {
                return;
            }
            await SetupNetworkSynchedHelper(_runner);
            if (player == _runner.LocalPlayer)
            {
                _netSynchedHelper.InitialiseUser(player.PlayerId, _sessionUserNickName);
            }
            _connectedPlayers.Add(player);
            //wait for other user to be ready
            while (!_netSynchedHelper.AllUsersReady(runner.SessionInfo.PlayerCount))
            {
                await Task.Yield();
            }
            OnPlayerConnected?.Invoke(player.PlayerId);
            if (!runner.IsServer)
                return;
            OnPlayerJoinedOnServer(runner, player);
        }

        private async Task SetupNetworkSynchedHelper(NetworkRunner runner)
        {
            if (_netSynchedHelper == null)
            {
                if (runner.IsServer)
                {
                    var go = runner.Spawn(_synchedDataPrefab);
                    _netSynchedHelper = go.GetComponent<NetworkManagerSynchedHelper>();
                }
                else
                {
                    while (_netSynchedHelper == null)
                    {
                        _netSynchedHelper = FindObjectOfType<NetworkManagerSynchedHelper>();
                        await Task.Yield();
                    }
                }
                RegisterToNetSyncEvents();
            }
        }

        private void RegisterToNetSyncEvents()
        {
            //Todo: This is a Hack, clean it up later.
            _netSynchedHelper.OnSimpleNetworkMessageRecieved -= OnSimpleNetworkEventReceived;
            _netSynchedHelper.OnSimpleNetworkMessageRecieved += OnSimpleNetworkEventReceived;
        }

        private void OnPlayerJoinedOnServer(NetworkRunner runner, PlayerRef player)
        {
            NetworkLogger.Log($"People in Session: {runner.SessionInfo.PlayerCount.ToString()} / {runner.SessionInfo.MaxPlayers.ToString()}");
            if (runner.SessionInfo.PlayerCount < runner.SessionInfo.MaxPlayers)
                return;
           
            if (_gameStarted)
            {
                return;
            }
            StartGame();
        }

        public void SpawnPlayers(List<Vector3> positions, List<Quaternion> rotations)
        {
            if(positions.Count<_connectedPlayers.Count)
                throw new Exception("Not enough positions to spawn players");
            if(rotations.Count<_connectedPlayers.Count)
                throw new Exception("Not enough rotations to spawn players");
            var index = 0;

            var playerPrefabs = new List<NetworkPrefabRef>(_networkPropertiesRef.PlayerPrefabs);
            playerPrefabs.Shuffle();

            foreach (var playerRef in _connectedPlayers)
            {
                var position = positions[index];
                var rotation = rotations[index];
                var prefabToUse = playerPrefabs[0];
                playerPrefabs.RemoveAt(0);
                var playerNetObj = _runner.Spawn(prefabToUse,
                    position, rotation, playerRef);

                _runner.SetPlayerObject(playerRef, playerNetObj);

                if (!_spawnedCharactersOnServer.ContainsKey(playerRef))
                {
                    _spawnedCharactersOnServer.Add(playerRef, playerNetObj);
                }
                index++;
            }
        }
        public void OnPlayerLeft(Fusion.NetworkRunner runner, PlayerRef player)
        {
            if (runner.SessionInfo.Name.Equals(Constants.GAME_LOBBY))
                return;

            if (player == _runner.LocalPlayer)
            {
                _netSynchedHelper.RemoveUserNickName(player.PlayerId);
            }

            _connectedPlayers.Remove(player);
            if (!runner.IsServer)
                return;
            OnPlayerLeftOnServer(runner, player);
        }

        private void OnPlayerLeftOnServer(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedCharactersOnServer.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharactersOnServer.Remove(player);
            }

            var id = player.PlayerId;
            OnPlayerDisconnected?.Invoke(id);
        }

        public void OnInput(Fusion.NetworkRunner runner, NetworkInput input)
        {
            if (!_playerInput)
            {
                NetworkObject player = GetLocalPlayer();
                if (!player)
                    return;

                _playerInput = player.GetComponent<PlayerNetworkedActions>();
                if (!_playerInput)
                    return;
            }

            var inputdata = _playerInput.GetInputData();
            input.Set(inputdata);
            _playerInput.RefreshInputData();
        }


        public void OnInputMissing(Fusion.NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(Fusion.NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(Fusion.NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(Fusion.NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(Fusion.NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(Fusion.NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(Fusion.NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(Fusion.NetworkRunner runner, List<SessionInfo> sessionList)
        {
            var copy = new List<SessionInfo>(sessionList);
            // remove all sessions that's full.
            copy.RemoveAll(a => a.PlayerCount >= a.MaxPlayers);

            AvailableSessions = copy;
            OnAvailableSessionsListUpdated?.Invoke();
        }

        public void OnCustomAuthenticationResponse(Fusion.NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(Fusion.NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(Fusion.NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(Fusion.NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnSceneLoadDone(Fusion.NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(Fusion.NetworkRunner runner)
        {
        }

        #endregion
        public async Task<bool> LoadSceneNetworked(int sceneIndex, bool isAdditive)
        {
            _connectionUI.ShowLoadingScreen(true);
            SendGlobalSimpleNetworkMessage(new NetworkEvent()
            {
                EventName = "loading_screen",
                EventData = "1"
            });
            var loadSceneParameters = new LoadSceneParameters();
            loadSceneParameters.loadSceneMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            if (!_runner)
            {
                _connectionUI.ShowLoadingScreen(false);
                return false;
            }
            
            var scene = SceneRef.FromIndex(sceneIndex);
            if (!scene.IsValid)
            {
                _connectionUI.ShowLoadingScreen(false);
                return false;
            }

            await Task.Delay(1000);
            var task = _runner.LoadScene(scene, loadSceneParameters);
            while (!task.IsDone)
                await Task.Yield();
            
            await Task.Delay(1000);
            _connectionUI.ShowLoadingScreen(false);
            SendGlobalSimpleNetworkMessage(new NetworkEvent()
            {
                EventName = "loading_screen",
                EventData = "0"
            });
            return true;
        }
        
        public NetworkObject GetLocalPlayer()
        {
            return _runner.GetPlayerObject(_runner.LocalPlayer);
        }

        public void ResetGame()
        {
            _instance = null;
            Destroy(_connectionUI.gameObject);
            Destroy(_netSynchedHelper.gameObject);
            Destroy(this.gameObject);
            SceneManager.LoadScene(0);
        }
    }
}
