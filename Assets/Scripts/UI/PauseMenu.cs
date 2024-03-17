using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//NetworkBehaviour?
public class PauseMenu : MonoBehaviour 
{
    [SerializeField]
    private GameObject PauseScreen;
    [SerializeField]
    private bool _isPaused; //grab from player
    

    void Start()
    {
        PauseScreen.SetActive(false);

    }

    public void Pause()
    {
        _isPaused = true;
        PauseScreen.SetActive(_isPaused);
    }
    public void Resume()
    {
        _isPaused = false;
        PauseScreen.SetActive(_isPaused);
    }
    //public override void FixedUpdateNetwork()
    //{
        //Runner.DeltaTime
        //Fields marked with [Networked] have to be properties and have the {get; set;} stubs as they are used by Fusion to generate serialization code.
        //TickTimer?
        //public void Init()
        //        {
        //            life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        //        }
        //        Finally, FixedUpdateNetwork() must be updated to check if the timer has expired and if so, de - spawn the ball:

        //C#

        //if (life.Expired(Runner))
        //            Runner.Despawn(Object);
        //Other states such as variables in your scripts are not synchronized over the network. To have state synchronized over the network a[Networked] Property is needed.Networked Properties synchronize their state from the StateAuthority to all other clients.
        //To solve this problem, changes can be detected via a ChangeDector. Add a new ChangeDector to the script and initialize it in Spawned like this:
    //}
}
