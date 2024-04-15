using System;
using System.Collections;
using Audio;
using Fusion;
using Networking.Behaviours;
using Networking.Data;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Networking.UI
{
    public class NetworkUI : MonoBehaviour
    {
        
        [SerializeField] private GameObject waitPanel;
        [SerializeField] private GameObject loadingScreen;

        [Header("NickName")]
        [SerializeField] private TMPro.TMP_InputField _nickNameField;
        
        [FormerlySerializedAs("hostButton")]
        [Header("Main menu")]
        [SerializeField] private Button _smartConnectButton;
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

        
        private NetworkManager _networkManager;
        private Coroutine _loadingScreenCoro;
        
        
        public void Initialise(NetworkManager manager)
        {
            _networkManager = manager;
            _smartConnectButton.onClick.AddListener(OnClickMainSmartConnect);
            _hostButton.onClick.AddListener(OnClickMainMenuHost);
            _joinButton.onClick.AddListener(OnClickMainMenuJoin);
            _hostMenuCloseButton.onClick.AddListener(OnClickHostMenuClose);
            _hostMenuStartButton.onClick.AddListener(OnClickHostMenuStart);
            _joinMenuCloseButton.onClick.AddListener(OnClickJoinMenuClose);
            var randName = $"Random{Random.Range(1, 300).ToString()}";
            _nickNameField.text = PlayerPrefs.GetString(Constants.MYUSERNAME_KEY,randName);
            NetworkManager.Instance.SetSessionUserNickName(_nickNameField.text);

            NetworkManager.Instance.OnPlayerConnected += OnPlayerConnected;
            if (_nickNameField.text.Equals(randName))
            {
                PlayerPrefs.SetString(Constants.MYUSERNAME_KEY, randName);
                PlayerPrefs.Save();
                NetworkManager.Instance.SetSessionUserNickName(randName);
            }
            _nickNameField.onValueChanged.AddListener((text) =>
            {
                AudioManager.Instance?.PlaySFX("type");
                NetworkManager.Instance.SetSessionUserNickName(text);
                Debug.Log("Username saved!!");
            });
            _hostSessionNameField.onValueChanged.AddListener((string str) =>
            { 
                AudioManager.Instance?.PlaySFX("type");
                _hostMenuStartButton.interactable = !String.IsNullOrEmpty(str);
            });
            _networkManager.OnConnectedToLobby += () =>
            {
                waitPanel.SetActive(false);
            };
        }

        public void ShowWait(bool show)
        {
            waitPanel.SetActive(show);
        }
        
        public void ShowLoadingScreen(bool show)
        {
            if(_loadingScreenCoro != null)
                StopCoroutine(_loadingScreenCoro);

            _loadingScreenCoro = StartCoroutine(FadeLoadingScreen(!show));
        }

        IEnumerator FadeLoadingScreen(bool fadeOut)
        {
            loadingScreen.SetActive(true);
            var cg = loadingScreen.GetComponent<CanvasGroup>();
            if (!fadeOut)
            {
                cg.alpha = 1;
                yield break;    
            }
            
            var curr = cg.alpha;
            var next = fadeOut ? 0 : 1;
            var t = 0f;
            while (t <= 1)
            {
                t += Time.deltaTime / 0.25f;
                cg.alpha = Mathf.Lerp(curr, next, t);
                yield return new WaitForEndOfFrame();
            }
            loadingScreen.SetActive(false);
        }
        
        private void OnPlayerConnected(int playerId)
        {
            if (NetworkManager.Instance.ConnectedPlayers.Count >= 2)
            {
                waitPanel.SetActive(false);
                return;
            }

            if (NetworkManager.Instance.ConnectedPlayers.Count > 0)
            {
                waitPanel.SetActive(true);
            }
        }
        
        private async void OnClickMainSmartConnect()
        {
            await NetworkManager.Instance.SmartConnect();
        }
        
        private void OnClickMainMenuHost()
        {
            AudioManager.Instance?.PlaySFX("click");
            _hostMenuStartButton.interactable = false;
            _hostSessionNameField.text = string.Empty;
            _hostMenu.gameObject.SetActive(true);
        }

        private void OnClickHostMenuClose()
        {
            AudioManager.Instance?.PlaySFX("click");
            _hostMenu.gameObject.SetActive(false);
        }
        
        private void OnClickHostMenuStart()
        {
            AudioManager.Instance?.PlaySFX("click");
            if (String.IsNullOrEmpty(_hostSessionNameField.text))
                return;
            
            _networkManager.HostSession(_hostSessionNameField.text);
        }
        
        private void OnClickMainMenuJoin()
        {
            AudioManager.Instance?.PlaySFX("click");
            _networkManager.OnAvailableSessionsListUpdated += PopulateSessions;
            _joinMenu.gameObject.SetActive(true);
            PopulateSessions();
        }
        
        private void OnClickJoinMenuClose()
        {
            AudioManager.Instance?.PlaySFX("click");
            _networkManager.OnAvailableSessionsListUpdated -= PopulateSessions;
            _joinMenu.gameObject.SetActive(false);
        }
        
        private void PopulateSessions()
        {
            var sessions = _networkManager.AvailableSessions;
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
                    _networkManager.JoinSession(s.Name);
                });
            }
        }

        private void OnDestroy()
        {
            PlayerPrefs.SetString(Constants.MYUSERNAME_KEY, _nickNameField.text);
            PlayerPrefs.Save();
        }
    }
}