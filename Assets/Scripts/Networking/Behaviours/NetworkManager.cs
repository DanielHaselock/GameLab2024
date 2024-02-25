using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Networking.Data;
using Networking.UI;
using Networking.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Behaviours
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        private const string GAME_LOBBY = "GAME_LOBBY";
        private const string NETWORK_OBJ_SO_NAME = "NetworkProperties";
        private const string NETWORK_UI = "NetworkCanvas";
        private NetworkProperties _networkPropertiesRef;
        private NetworkRunner _runner;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private PlayerInputData _playerInput = new PlayerInputData();

        private List<PlayerRef> _connectedPlayers;
        
        private NetworkUI _connectionUI;
        
        public Action OnAvailableSessionsListUpdated;
        public Action OnConnectedToLobby;
        public List<SessionInfo> AvailableSessions { get; private set; }

        private bool _gameStarted = false;
        
        private void Start()
        {
            _connectedPlayers = new List<PlayerRef>();
            _networkPropertiesRef = Resources.Load<NetworkProperties>(NETWORK_OBJ_SO_NAME);
            _connectionUI = Instantiate(Resources.Load<GameObject>(NETWORK_UI)).GetComponent<NetworkUI>();
            _connectionUI.Initialise(this);
            
            AvailableSessions = new List<SessionInfo>();
            ConnectToLobby();
        }

        public List<PlayerRef> ConnectedPlayers => _connectedPlayers;
        
        private void TryInitNetworkRunner()
        {
            if (_runner == null)
            {
                NetworkLogger.Log("Creating new Runner");
                _runner = gameObject.AddComponent<NetworkRunner>();
                _runner.ProvideInput = true;
            }
        }

        public void HostSession(String sessionName)
        {
            _connectionUI.gameObject.SetActive(false);
            LaunchSession(sessionName, GameMode.Host);
        }
        
        public void JoinSession(String sessionName)
        {
            _connectionUI.gameObject.SetActive(false);
            LaunchSession(sessionName, GameMode.Client);
        }
        
        private async void ConnectToLobby()
        {
            TryInitNetworkRunner();
            var res = await _runner.JoinSessionLobby(SessionLobby.ClientServer, GAME_LOBBY);
            if(!res.Ok)
                Destroy(_runner);
            else
            {
                NetworkLogger.Log("Connected to Lobby!");
                OnConnectedToLobby?.Invoke();
            }
        }
        
        private async void LaunchSession(String sessionName, GameMode mode)
        {
            TryInitNetworkRunner();
            var scene = SceneRef.FromIndex(0);
            var sceneInfo = new NetworkSceneInfo();
            
            if (scene.IsValid) {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }

            var res = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = sessionName,
                Scene = sceneInfo,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                PlayerCount = 2
            });

            if (!res.Ok)
            {
                Destroy(_runner);
            }
        }
        
        #region Callbacks
        
        
        public void OnObjectExitAOI(Fusion.NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(Fusion.NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        
        public async void OnPlayerJoined(Fusion.NetworkRunner runner, PlayerRef player)
        {
            
            if (!runner.IsServer)
                return;

            NetworkLogger.Log($"People in Session: {runner.SessionInfo.PlayerCount} / {runner.SessionInfo.MaxPlayers}");
            _connectedPlayers.Add(player);
            //if (runner.SessionInfo.PlayerCount < runner.SessionInfo.MaxPlayers)
            //    return;

            if (!_gameStarted)
            {
                NetworkLogger.Log("All Players Joined!!, changing the scene to game");
                var scene = runner.LoadScene(SceneRef.FromIndex(_networkPropertiesRef.GameSceneIndex));
                while (!scene.IsDone)
                {
                    await Task.Yield();
                }
                _gameStarted = true;
            }

            foreach (var cp in _connectedPlayers)
            {
                var playerRef = runner.Spawn(_networkPropertiesRef.PlayerPrefab,
                    new Vector3(/*_connectedPlayers.IndexOf(cp) * 10*/ 0, 2, 0), Quaternion.identity, cp);

                if(!_spawnedCharacters.ContainsKey(cp))
                {
                    _spawnedCharacters.Add(cp, playerRef);
                }
                
            } 
            // wait for scene to load.
        }

        public void OnPlayerLeft(Fusion.NetworkRunner runner, PlayerRef player)
        {
            _connectedPlayers.Remove(player);
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        public void OnInput(Fusion.NetworkRunner runner, NetworkInput input)
        {
            _playerInput.Poll();
            input.Set(_playerInput);
        }
        
        
        public void OnInputMissing(Fusion.NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(Fusion.NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(Fusion.NetworkRunner runner)
        {

        }

        public void OnDisconnectedFromServer(Fusion.NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(Fusion.NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(Fusion.NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(Fusion.NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(Fusion.NetworkRunner runner, List<SessionInfo> sessionList)
        {
            AvailableSessions = sessionList;
            OnAvailableSessionsListUpdated?.Invoke();
        }
        
        public void OnCustomAuthenticationResponse(Fusion.NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(Fusion.NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(Fusion.NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(Fusion.NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(Fusion.NetworkRunner runner) { }
        public void OnSceneLoadStart(Fusion.NetworkRunner runner){ }
        #endregion

        public void CheckMainPlayer()
        {

            Debug.Log("LOCAL PLAYER " + _runner.LocalPlayer.ToString());

            Debug.Log("Success!!!");
            _spawnedCharacters.TryGetValue(_runner.LocalPlayer, out NetworkObject obj);
            
        }
    }
}
