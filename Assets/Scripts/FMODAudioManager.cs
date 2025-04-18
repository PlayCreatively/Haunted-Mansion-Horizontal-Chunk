using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAudioManager : MonoBehaviour
{
    //Event Instances
    private EventInstance landingOnTheGhostInstance;


    // Assigning paths
    void Awake()
    {
        landingOnTheGhostInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on ghost");
    }

    //Starting looped tracks
    void Start()
    {
        //exampleInstance.start();
    }

    //Public methods

    public void TriggerLandingOnTheGhostSfx(int ghostHp) //Plays a SFX based on ghost's HP
    {
        landingOnTheGhostInstance.setParameterByName("Ghost HP", ghostHp);
        landingOnTheGhostInstance.start();
    }
}