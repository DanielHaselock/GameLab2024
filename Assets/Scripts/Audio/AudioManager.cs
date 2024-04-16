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
        [Serializable]
        class SVec3
        {
            public float x;
            public float y;
            public float z;

            public SVec3(Vector3 pos)
            {
                this.x = pos.x;
                this.y = pos.y;
                this.z = pos.z;
            }
            
            public string ToJson()
            {
                return JsonUtility.ToJson(this);
            }

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }

            public static SVec3 FromJson(string json)
            {
                return JsonUtility.FromJson<SVec3>(json);
            }
        }
        
        private AudioSource _bgSource;
        private AudioSource _ambianceSource;
        private AudioMixerGroup _sfxMixerGroup;

        private HashSet<string> _sfxMemory;

        private bool muted = false;

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMap _map;

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
        }
        
        public void PlayBackgroundMusic(string bgName)
        {
            if (string.IsNullOrEmpty(bgName))
            {
                _bgSource.Stop();
                return;
            }
            
            var clip = _map.GetBackgroundClip(bgName);
            if (clip == null)
                return;

            _bgSource.loop = true;
            _bgSource.clip = clip;
            _bgSource.Play();
        }
        
        public void PlayAmbiance(string ambName)
        {
            if (string.IsNullOrEmpty(ambName))
            {
                _ambianceSource.Stop();
                return;
            }
            
            var clip = _map.GetAmbianceClip(ambName);
            if (clip == null)
                return;

            _ambianceSource.loop = true;
            _ambianceSource.clip = clip;
            _ambianceSource.Play();
        }

        public void PlaySFX(string sfxName, bool random = true, bool syncNetwork = false)
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
        
        public void PlaySFX3D(string sfxName, Vector3 position,bool random = true, bool syncNetwork = false)
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
                    EventData = $"{sfxName};{(random?"1":"0")};{(new SVec3(position).ToJson())}" 
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
            if (split.Length > 2)
            {
                var position = SVec3.FromJson(split[2]).ToVector3();
                PlaySFXLocal3D(sfx, random, position);
            }
            else
                PlaySFXLocal(sfx, random);
        }
        
        private void PlaySFXLocal(string sfxName, bool random)
        {
            // if(_sfxMemory.Contains(sfxName))
            //     return;
            
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
        
        private void PlaySFXLocal3D(string sfxName, bool random, Vector3 position)
        {
            // if(_sfxMemory.Contains(sfxName))
            //     return;
            
            var clip = _map.GetSFX(sfxName, random);
            if (clip == null)
                return;
            if(transform.Find($"SFX_{clip.name}") != null)
                return;
            var sfx = new GameObject($"SFX_{clip.name}").AddComponent<AudioSource>();
            sfx.transform.position = position;
            sfx.maxDistance = 4;
            sfx.minDistance = 2;
            sfx.spatialize = true;
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

        public void MuteBGAndAmbiance(bool mute)
        {
            if (mute)
            {
                _bgSource.volume = 0;
                _ambianceSource.volume = 0;
            }
            else
            {
                _bgSource.volume = 1;
                _ambianceSource.volume = 1;
            }
        }
    }
}
