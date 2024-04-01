using System;
using System.Collections;
using System.Collections.Generic;
using Networking.Behaviours;
using Utils;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        private AudioSource _bgSource;
        private AudioSource _ambianceSource;
        private AudioMixerGroup _sfxMixerGroup;

        private HashSet<string> _sfxMemory;

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMap _map;
        [SerializeField] private float _bgFadeDuration = 0.5f;

        [RuntimeInitializeOnLoadMethod]
        private static void SpawnAudioManager()
        {
            var manager = Resources.Load<GameObject>("AudioManager");
            Instantiate(manager);
        }

        private void OnDestroy()
        {
            if (this == Instance && NetworkManager.Instance != null)
            {
                NetworkManager.Instance.DeRegisterToGeneralNetworkEvents("PlaySFX", OnNetworkSFXPlayRequested);
            }
        }

        private void Start()
        {
            _sfxMemory = new HashSet<string>();
            
            if(NetworkManager.Instance != null)
                NetworkManager.Instance.RegisterToGeneralNetworkEvents("PlaySFX", OnNetworkSFXPlayRequested);
            
            _bgSource = new GameObject("BackgroundMusic").AddComponent<AudioSource>();
            _bgSource.transform.parent = transform;

            _ambianceSource = new GameObject("Ambience").AddComponent<AudioSource>();
            _ambianceSource.transform.parent = transform;

            _bgSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("Background")[0];
            _ambianceSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("Ambiance")[0];

            _sfxMixerGroup = _mixer.FindMatchingGroups("SFX")[0];

            PlayBackgroundMusic(_map.InitialBackgroundMusic);
            PlaySFX(_map.InitialAmbiance);
        }
        
        public void PlayBackgroundMusic(string bgName, bool doFadeTransition = true)
        {
            var clip = _map.GetBackgroundClip(bgName);
            if (clip == null)
                return;

            if (!doFadeTransition || _bgFadeDuration <= 0 || _bgSource.clip == null)
            {
                _bgSource.loop = true;
                _bgSource.clip = clip;
                _bgSource.Play();
            }
            else
            {
                var current = _bgSource;
                _bgSource = _bgSource.gameObject.AddComponent<AudioSource>();
                _bgSource.outputAudioMixerGroup = current.outputAudioMixerGroup;
                _bgSource.volume = 0;
                _bgSource.loop = true;
                _bgSource.clip = clip;
                _bgSource.Play();

                StartCoroutine(FadeAudioSourceRoutine(current, false, _bgFadeDuration / 2));
                StartCoroutine(FadeAudioSourceRoutine(_bgSource, true, _bgFadeDuration / 2));
            }
        }

        private IEnumerator FadeAudioSourceRoutine(AudioSource _src, bool fadeIn, float dur)
        {
            float final = fadeIn ? 1f : 0f;
            float current = fadeIn ? 0 : 1;
            float timeStep = 0;
            while (timeStep <= 1)
            {
                timeStep += Time.deltaTime / dur;
                _src.volume = Mathf.Lerp(current, final, timeStep);
                yield return new WaitForEndOfFrame();
            }

            if (!fadeIn)
                Destroy(_src);
        }

        public void PlayAmbiance(string ambName)
        {
            var clip = _map.GetAmbianceClip(ambName);
            if (clip == null)
                return;

            _ambianceSource.loop = true;
            _ambianceSource.clip = clip;
            _ambianceSource.Play();
        }

        public void PlaySFX(string sfxName, bool random = true, bool syncNetwork=false)
        {
            if (syncNetwork)
            {
                if (NetworkManager.Instance == null)
                {
                    PlaySFXLocal(sfxName, random);
                    return;
                }
                
                NetworkManager.Instance.SendGlobalSimpleNetworkMessage(new NetworkEvent() 
                { 
                   EventName = "PlaySFX",
                   EventData = $"{sfxName};{(random?"1":"0")}" 
                });
            }
            else
            {
                PlaySFXLocal(sfxName, random);
            }
        }

        private void OnNetworkSFXPlayRequested(NetworkEvent eventData)
        {
            var split = eventData.EventData.Split(";");
            var sfx = split[0];
            var random = split[1] == "1";
            PlaySFXLocal(sfx, random);
        }
        
        private void PlaySFXLocal(string sfxName, bool random)
        {
            if(_sfxMemory.Contains(sfxName))
                return;
            var clip = _map.GetSFX(sfxName, random);
            if (clip == null)
                return;
            if(transform.Find($"SFX_{clip.name}") != null)
                return;
            var sfx = new GameObject($"SFX_{clip.name}").AddComponent<AudioSource>();
            sfx.transform.parent = transform;
            sfx.outputAudioMixerGroup = _sfxMixerGroup;
            sfx.PlayOneShot(clip);
            StartCoroutine(RemoveKeyDelayed(sfxName, 0.25f));
            Destroy(sfx.gameObject, clip.length);
        }

        IEnumerator RemoveKeyDelayed(string key, float delay)
        {
            yield return new WaitForSeconds(delay);
            _sfxMemory.Remove(key);
        }
    }
}
