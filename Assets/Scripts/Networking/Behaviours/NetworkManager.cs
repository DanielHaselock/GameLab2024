using System;
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

namespace Networking.Behaviours {
public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _synchedDataPrefab;
    
    private NetworkProperties _networkPropertiesRef;
    private NetworkRunner _runner;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private PlayerInputData _playerInput = new PlayerInputData();
    private List<PlayerRef> _connectedPlayers;
    private NetworkUI _connectionUI;
    private bool _gameStarted = false;
    private string _sessionUserNickName;
    
    private NetworkManagerSynchedHelper _netSynchedHelper;
    public List<SessionInfo> AvailableSessions { get; private set; }
    public List<PlayerRef> ConnectedPlayers => _connectedPlayers;
    public bool IsServer => _runner && _runner.IsServer;
    
    /// <summary>
    /// Invoked when session list is updated,
    /// use AvailableSessions property to get updated list
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
    /// Invoked On Server, when all the player controllers are spawned and before OnGameStarted
    /// </summary>
    public Action OnPlayersSpawned;
    /// <summary>
    /// Invoked On Server, when all the player controllers are spawned and ready to start game
    /// </summary>
    public Action OnGameStarted;
    /// <summary>
    /// Invoked On Server, when GameOver method is invoked
    /// </summary>
    public Action OnGameOver;
    
    /// <summary>
    /// Invoked when a Timer is started
    /// <param name="Time"> TimeSpan with Timer Duration </param>
    /// </summary>
    public Action<TimeSpan> OnTimerStarted;
    /// <summary>
    /// Invoked During Timer Tick
    ///<param name="Remaining time"> TimeSpan with time remaining on the timer </param>
    /// </summary>
    public Action<TimeSpan> OnTimerTick;
    /// <summary>
    /// Invoked when a Timer ends, NOTE: NOT INVOKED WHEN TIMER IS STOPPED
    /// </summary>
    public Action OnTimerEnded;
    

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

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _connectedPlayers = new List<PlayerRef>();
        _networkPropertiesRef = Resources.Load<NetworkProperties>(Constants.NETWORK_OBJ_SO_NAME);
        _connectionUI = Instantiate(Resources.Load<GameObject>(Constants.NETWORK_UI)).GetComponent<NetworkUI>();
        _connectionUI.Initialise(this);
        AvailableSessions = new List<SessionInfo>();
        ConnectToLobby();
    }

    private async void ConnectToLobby()
    {
        TryInitNetworkRunner();
        var res = await _runner.JoinSessionLobby(SessionLobby.ClientServer, Constants.GAME_LOBBY);
        if (!res.Ok)
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
    
    private void TryInitNetworkRunner()    {
        if (_runner == null)
        {
            NetworkLogger.Log("Creating new Runner");
            _runner = gameObject.AddComponent<NetworkRunner>();
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
    /// Starts The Global Timer, Can only be invoked by the server
    /// </summary>
    /// <param name="time"></param>
    public void StartTimer(TimeSpan time)
    {
        _netSynchedHelper?.StartTimer(time);
    }
    
    /// <summary>
    /// Stops The Global Timer, Can only be invoked by the server
    /// </summary>
    public void StopTimer()
    {
        _netSynchedHelper?.StopTimer();
    }
    
    /// <summary>
    /// Host a Game Session
    /// </summary>
    /// <param name="sessionName"></param>
    public void HostSession(String sessionName)
    {
        _connectionUI.gameObject.SetActive(false);
        LaunchSession(sessionName, GameMode.Host);
    }

    /// <summary>
    /// Join a Game Session
    /// </summary>
    /// <param name="sessionName"></param>
    public void JoinSession(String sessionName)
    {
        _connectionUI.gameObject.SetActive(false);
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
        await SetupNetworkSynchedHelper(runner);
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
        await OnPlayerJoinedOnServer(runner, player);
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
        _netSynchedHelper.OnTimerStarted -= OnTimerStarted;
        _netSynchedHelper.OnTimerTick -= OnTimerTick;
        _netSynchedHelper.OnTimerEnded -= OnTimerEnded;
        
        _netSynchedHelper.OnTimerStarted += OnTimerStarted;
        _netSynchedHelper.OnTimerTick += OnTimerTick;
        _netSynchedHelper.OnTimerEnded += OnTimerEnded;
    }
    
    private async Task OnPlayerJoinedOnServer(NetworkRunner runner, PlayerRef player)
    {
        NetworkLogger.Log($"People in Session: {runner.SessionInfo.PlayerCount} / {runner.SessionInfo.MaxPlayers}");
        if (runner.SessionInfo.PlayerCount < runner.SessionInfo.MaxPlayers)
            return;

        if (!_gameStarted)
        {
            NetworkLogger.Log("All Players Joined!!, changing the scene to game");
            var scene = runner.LoadScene(SceneRef.FromIndex(_networkPropertiesRef.GameSceneIndex));
            // wait for scene to load.
            while (!scene.IsDone)
            {
                await Task.Yield();
            }
        }

        foreach (var playerRef in _connectedPlayers)
        {
            var playerNetObj = _runner.Spawn(_networkPropertiesRef.PlayerPrefab,
                new Vector3( 0, 2, 0), Quaternion.identity, playerRef);

            _runner.SetPlayerObject(playerRef, playerNetObj);

            if (!_spawnedCharacters.ContainsKey(playerRef))
            {
                _spawnedCharacters.Add(playerRef, playerNetObj);
            }
        }
        OnPlayersSpawned?.Invoke();
        _gameStarted = true;
        OnGameStarted?.Invoke();
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
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
        
        var id = player.PlayerId;
        OnPlayerDisconnected?.Invoke(id);
    }
    
    public void OnInput(Fusion.NetworkRunner runner, NetworkInput input)
    {
        _playerInput.Poll();
        input.Set(_playerInput);
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
        AvailableSessions = sessionList;
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
    public void CheckMainPlayer()
    {

        Debug.Log("LOCAL PLAYER " + _runner.LocalPlayer.ToString());

        Debug.Log("Success!!!");
        _spawnedCharacters.TryGetValue(_runner.LocalPlayer, out NetworkObject obj);

    }
}
}
