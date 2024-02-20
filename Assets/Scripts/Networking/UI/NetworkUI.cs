using System;
using Networking.Behaviours;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Networking.UI
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private NetworkManager _networkSpawner;
        [SerializeField] private GameObject waitPanel;
        
        [FormerlySerializedAs("hostButton")]
        [Header("Main menu")]
        [SerializeField] private Button _hostButton;
        [FormerlySerializedAs("joinButton")] [SerializeField] private Button _joinButton;

        [FormerlySerializedAs("hostMenu")]
        [Header("Host Menu")]
        [SerializeField] private GameObject _hostMenu;
        [FormerlySerializedAs("hostSessionNameField")] [SerializeField] private TMPro.TMP_InputField _hostSessionNameField;
        [FormerlySerializedAs("hostMenuStartButton")] [SerializeField] private Button _hostMenuStartButton;
        [FormerlySerializedAs("hostMenuCloseButton")] [SerializeField] private Button _hostMenuCloseButton;
        
        [FormerlySerializedAs("joinMenu")]
        [Header("Join Menu")]
        [SerializeField] private GameObject _joinMenu;
        [FormerlySerializedAs("joinMenuCloseButton")] [SerializeField] private Button _joinMenuCloseButton;
        [FormerlySerializedAs("sessionsParent")] [SerializeField] private Transform _sessionsParent;
        [FormerlySerializedAs("sessionButtonTemplate")] [SerializeField] private Transform _sessionButtonTemplate;


        private void Start()
        {
            _hostButton.onClick.AddListener(OnClickMainMenuHost);
            _joinButton.onClick.AddListener(OnClickMainMenuJoin);
            
            _hostMenuCloseButton.onClick.AddListener(OnClickHostMenuClose);
            _hostMenuStartButton.onClick.AddListener(OnClickHostMenuStart);
            
            _joinMenuCloseButton.onClick.AddListener(OnClickJoinMenuClose);
            _hostSessionNameField.onValueChanged.AddListener((string str) =>
            {
                    _hostMenuStartButton.interactable = !String.IsNullOrEmpty(str);
            });

            _networkSpawner.OnConnectedToLobby += () =>
            {
                waitPanel.SetActive(false);
            };
        }

        private void OnClickMainMenuHost()
        {
            _hostMenuStartButton.interactable = false;
            _hostSessionNameField.text = string.Empty;
            _hostMenu.gameObject.SetActive(true);
        }

        private void OnClickHostMenuClose()
        {
            _hostMenu.gameObject.SetActive(false);
        }
        
        private void OnClickHostMenuStart()
        {
            if (String.IsNullOrEmpty(_hostSessionNameField.text))
                return;
            
            _networkSpawner.HostSession(_hostSessionNameField.text);
        }
        
        private void OnClickMainMenuJoin()
        {
            _networkSpawner.OnAvailableSessionsListUpdated += PopulateSessions;
            _joinMenu.gameObject.SetActive(true);
            PopulateSessions();
        }
        
        private void OnClickJoinMenuClose()
        {
            _networkSpawner.OnAvailableSessionsListUpdated -= PopulateSessions;
            _joinMenu.gameObject.SetActive(false);
        }
        
        private void PopulateSessions()
        {
            var sessions = _networkSpawner.AvailableSessions;
            foreach(Transform child in _sessionsParent)
            {
                if (child == _sessionButtonTemplate)
                    continue;
                
                Destroy(child.gameObject);
            }

            foreach (var session in sessions)
            {
                var s = session;
                if(s.PlayerCount >= s.MaxPlayers)
                    continue;
                
                var go = Instantiate(_sessionButtonTemplate.gameObject, _sessionsParent);
                go.SetActive(true);
                go.GetComponentInChildren<TMPro.TMP_Text>().text = $"{s.Name} ({s.PlayerCount.ToString()}/{s.MaxPlayers.ToString()})";
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _networkSpawner.JoinSession(s.Name);
                });
            }
        }
    }
}