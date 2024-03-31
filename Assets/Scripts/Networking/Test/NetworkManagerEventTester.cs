using System;
using Fusion;
using Networking.Behaviours;
using Networking.Utils;
using UnityEngine;

namespace Networking.Tests
{
    public class NetworkManagerEventTester : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
            NetworkManager.Instance.OnConnectedToLobby += () =>
            {
                NetworkLogger.Log("NMT:::Connected To Lobby");
            };
            
            NetworkManager.Instance.OnPlayerConnected += (id) =>
            {
                NetworkLogger.Log($"NMT:::Player Connected {id} NMIT::NICKNAME: {NetworkManager.Instance.GetPlayerNickNameById(id)}");
            };
            
            NetworkManager.Instance.OnPlayerDisconnected += (id) =>
            {
                NetworkLogger.Log($"NMT:::Player Disconnected {id} NMIT::NICKNAME: {NetworkManager.Instance.GetPlayerNickNameById(id)}");
            };
            
            NetworkManager.Instance.OnGameStarted += () =>
            {
                NetworkLogger.Log($"NMT:::Game Started");
            };
            
            NetworkManager.Instance.OnGameOver += () =>
            {
                NetworkLogger.Log($"NMT:::Game Over");
            };
            

            NetworkManager.Instance.OnTimerStarted += (t) =>
            {
                NetworkLogger.Log($"NMT:::OnTimerStarted {t.TotalSeconds}");
            };
            
            NetworkManager.Instance.OnTimerTick += (t) =>
            {
                NetworkLogger.Log($"NMT:::Tick, Remaining {t.TotalSeconds}");
            };
            
            NetworkManager.Instance.OnTimerEnded += () =>
            {
                NetworkLogger.Log($"NMT:::Timer Complete");
            };
            
        }

        private void Update()
        {
            //if(!NetworkManager.Instance.IsServer) This causes bugs
            //    return;
            
            //if(Input.GetButtonDown("Jump")) Deprecated Inputs
            //    NetworkManager.Instance.GameOver();
            
            //if(Input.GetButtonDown("Fire1"))
            //    NetworkManager.Instance.StartTimer(TimeSpan.FromSeconds(5));
            
            //if(Input.GetButtonDown("Fire2"))
            //    NetworkManager.Instance.StopTimer();
            
        }
    }
}