using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Networking.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Behaviours
{
    public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        private const string GAME_LOBBY = "GAME_LOBBY";
        
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        [SerializeField] private GameObject _connectionUI;
        
        private NetworkRunner _runner;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

        public System.Action OnAvailableSessionsListUpdated;
        public System.Action OnConnectedToLobby;
        
        public List<SessionInfo> AvailableSessions { get; private set; }
        
        private void Start()
        {
            AvailableSessions = new List<SessionInfo>();
            ConnectToLobby();
        }

        private void TryInitNetworkRunner()
        {
            if (_runner == null)
            {
                Debug.Log("Creating new Runner");
                _runner = gameObject.AddComponent<NetworkRunner>();
                _runner.ProvideInput = true;
            }
        }

        public void HostSession(String sessionName)
        {
            LaunchSession(sessionName, GameMode.Host);
        }
        
        public void JoinSession(String sessionName)
        {
            LaunchSession(sessionName, GameMode.Client);
        }
        
        private async void ConnectToLobby()
        {
            TryInitNetworkRunner();
            var res = await _runner.JoinSessionLobby(SessionLobby.ClientServer);
            if(!res.Ok)
                Destroy(_runner);
        }
        
        private async void LaunchSession(String sessionName, GameMode mode)
        {
            TryInitNetworkRunner();
            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid) {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }
            
            var res = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = sessionName,
                Scene = scene,
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

        public void OnPlayerJoined(Fusion.NetworkRunner runner, PlayerRef player)
        {
            if(runner.SessionInfo.Name.Equals(GAME_LOBBY))
            {
                OnConnectedToLobby?.Invoke();
                return;
            }
            
            _connectionUI.gameObject.SetActive(false);
            if (!runner.IsServer)
                return;

            if (runner.SessionInfo.PlayerCount >= runner.SessionInfo.MaxPlayers)
            {
                Debug.Log("All Players Joined!!");
            }
            
            var pos = new Vector3(player.RawEncoded % runner.Config.Simulation.PlayerCount * 3, 1, 0);
            var playerInstance = runner.Spawn(_playerPrefab, pos, Quaternion.identity, player);
            _spawnedCharacters.Add(player, playerInstance);
        }

        public void OnPlayerLeft(Fusion.NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        public void OnInput(Fusion.NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            data.direction += new Vector3(Input.GetAxis("Horizontal"),0,0);
            data.direction += new Vector3(0,0,Input.GetAxis("Vertical"));
            input.Set(data);
        }
        
        
        public void OnInputMissing(Fusion.NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(Fusion.NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(Fusion.NetworkRunner runner)
        { }

        public void OnDisconnectedFromServer(Fusion.NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(Fusion.NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(Fusion.NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(Fusion.NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(Fusion.NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Debug.Log($"sessions: {sessionList}");
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
    }
}
