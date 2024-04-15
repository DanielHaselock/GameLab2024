using System.Collections;
using UnityEngine;

public class SpatialAudioController : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioSource footStepSource;
    [SerializeField] private float footStepDelay;

    [Header("Revive")]
    [SerializeField] private AudioSource reviveSource;
    
    
    private Coroutine _footStepsRoutine;

    public void PlayRevive()
    {
        if(!reviveSource.isPlaying)
            reviveSource.Play();
    }

    public void StopRevive()
    {
        if(reviveSource.isPlaying)
            reviveSource.Stop();
    }
    
    public void PlayFootStep()
    {
        if (_footStepsRoutine != null)
        {
            return;
        }

        _footStepsRoutine = StartCoroutine(FootStepRoutine());
    }

    public void StopFootStep()
    {
        if (_footStepsRoutine != null)
        {
            StopCoroutine(_footStepsRoutine);
            footStepSource.Stop();
            _footStepsRoutine = null;
            return;
        }
    }

    private IEnumerator FootStepRoutine()
    {
        while (true)
        {
            footStepSource.Play();
            yield return new WaitForSeconds(footStepDelay);
        }
    }
}