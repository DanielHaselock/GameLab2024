using System;
using System.Collections;
using Fusion;
using Networking.Utils;
using Unity.VisualScripting;
using UnityEngine;

namespace Networking.Behaviours
{
    public class NetworkManagerSynchedHelper : NetworkBehaviour
    {
        [Networked, Capacity(10)] private NetworkDictionary<int, string> _userNickNames => default;
        [Networked] private int _readyUserCount { get; set; }
        public Action<NetworkEvent> OnSimpleNetworkMessageRecieved;
        
        public bool AllUsersReady(int expectedPlayerCount)
        {
            return expectedPlayerCount.Equals(_readyUserCount);
        }

        public void InitialiseUser(int playerId, string nickname)
        {
            if (Runner.IsServer)
            {
                _userNickNames.Add(playerId, nickname);
                _readyUserCount += 1;
                return;
            }

            RPC_InitClientOnServer(playerId, nickname);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_InitClientOnServer(int playerId, string nickname)
        {
            _userNickNames.Add(playerId, nickname);
            _readyUserCount += 1;
        }
        
        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public override void Spawned()
        {
            base.Spawned();
        }

        public string GetPlayerNickNameById(int id)
        {
            if (_userNickNames.ContainsKey(id))
                return _userNickNames[id];
            return string.Empty;
        }
        
        public void AddUserNickName(int id, string nickname)
        {
            _userNickNames.Set(id, nickname);
        }

        public void RemoveUserNickName(int id)
        {
            _userNickNames.Remove(id);
        }

        
        public void SendGlobalSimpleNetworkMessage(NetworkEvent eventData)
        {
            RPC_SendGlobalSimpleNetworkMessage(eventData.EventName, eventData.EventData);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_SendGlobalSimpleNetworkMessage(string eventName, string eventData)
        {
            OnSimpleNetworkMessageRecieved?.Invoke(new NetworkEvent()
            {
                EventName = eventName,
                EventData = eventData
            });
        }
    }
}