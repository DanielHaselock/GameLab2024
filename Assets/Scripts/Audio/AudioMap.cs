using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(menuName = "GameLabs/AudioMap", fileName = "AudioMap")]
    public class AudioMap : ScriptableObject
    {
        [SerializeField]
        private SerializableDictionary<string, AudioClip> _bgClips;

        [SerializeField] 
        private SerializableDictionary<string, AudioClip> _ambianceClips;

        [SerializeField]
        private SerializableDictionary<string, List<AudioClip>> _sfxClips;

        [SerializeField] public string InitialBackgroundMusic;
        [SerializeField] public string InitialAmbiance;
        
        public AudioClip GetBackgroundClip(string backgroundName)
        {
            if (!_bgClips.ContainsKey(backgroundName))
                return null;
            return _bgClips[backgroundName];
        }
        
        public AudioClip GetAmbianceClip(string ambianceName)
        {
            if (!_ambianceClips.ContainsKey(ambianceName))
                return null;
            return _ambianceClips[ambianceName];
        }

        public AudioClip GetSFX(string sfxName, bool random=true)
        {
            if (!_sfxClips.ContainsKey(sfxName))
                return null;
            if (_sfxClips[sfxName].Count <= 0)
                return null;
            
            if (!random)
                return _sfxClips[sfxName][0];

            return _sfxClips[sfxName][Random.Range(0, _sfxClips[sfxName].Count)];
        }
    }
}