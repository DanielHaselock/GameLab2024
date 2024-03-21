using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audio;
using Networking.Utils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class AudioManager : SingletonBehaviour<AudioManager>
{
    private AudioSource _bgSource;
    private AudioSource _ambianceSource;
    private AudioMixerGroup _sfxMixerGroup;

    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private AudioMap _map;
    [SerializeField] private float _bgFadeDuration = 0.5f;
    
    [RuntimeInitializeOnLoadMethod]
    private static void SpawnAudioManager()
    {
        var manager = Resources.Load<GameObject>("AudioManager");
        Instantiate(manager);
    }
    
    private void Start()
    {
        _bgSource = new GameObject("BackgroundMusic").AddComponent<AudioSource>();
        _bgSource.transform.parent = transform;
        
        _ambianceSource = new GameObject("Ambience").AddComponent<AudioSource>();
        _ambianceSource.transform.parent = transform;
        
        _bgSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("Background")[0];
        _ambianceSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("Ambiance")[0];
        
        _sfxMixerGroup =  _mixer.FindMatchingGroups("SFX")[0];
        
        PlayBackgroundMusic(_map.InitialBackgroundMusic);
        PlaySFX(_map.InitialAmbiance);
    }

    private bool toggle;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(toggle)
                PlayBackgroundMusic("test1");
            else
                PlayBackgroundMusic("test2");

            toggle = !toggle;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            PlaySFX("testSFX");
        }
    }

    public void PlayBackgroundMusic(string bgName, bool doFadeTransition=true)
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
        if(!fadeIn)
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
    
    public void PlaySFX(string sfxName, bool random=true)
    {
        var clip = _map.GetSFX(sfxName, random);
        if (clip == null)
            return;
        var sfx = new GameObject($"SFX_{clip.name}").AddComponent<AudioSource>();
        sfx.transform.parent = transform;
        sfx.outputAudioMixerGroup = _sfxMixerGroup;
        sfx.PlayOneShot(clip);
        Destroy(sfx.gameObject, clip.length);
    }
}
