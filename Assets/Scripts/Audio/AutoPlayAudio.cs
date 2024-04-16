using System;
using System.Collections;
using UnityEngine;

namespace Audio
{
    public class AutoPlayAudio : MonoBehaviour
    {
        [SerializeField] private string bgKey;
        [SerializeField] private string ambKey;

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => AudioManager.Instance == null);
            yield return new WaitForEndOfFrame();
            
            AudioManager.Instance.PlayBackgroundMusic(bgKey);
            AudioManager.Instance.PlayAmbiance(ambKey);
        }
    }
}