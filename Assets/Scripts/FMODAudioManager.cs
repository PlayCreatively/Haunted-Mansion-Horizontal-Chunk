using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAudioManager : MonoBehaviour
{
    //Event Instances for enemy SFX
    private EventInstance landingOnTheGhostInstance;
    private EventInstance landingOnTheSpiderInstance;
    private EventInstance landingOnTheGreenGooInstance;
    private EventInstance landingOnTheLTrashMonsterInstance;
    private EventInstance landingOnTheMTrashMonsterInstance;
    private EventInstance landingOnTheSTrashMonsterInstance;
    private EventInstance landingOnTheMummyInstance;
    
    //Event instances for character SFX
    
    
    // Assigning paths
    void Awake()
    {
        landingOnTheGhostInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on ghost");
        landingOnTheSpiderInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on spider");
        landingOnTheGreenGooInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on green goo");
        landingOnTheLTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on l trash monster");
        landingOnTheMTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on m trash monster");
        landingOnTheMTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on s trash monster");
        landingOnTheMummyInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on mummy");
    }

    //Starting looped tracks
    void Start()
    {
        //exampleInstance.start();
    }

    //Public methods

    public void TriggerLandingOnTheGhostSfx(int ghostHp) //Plays a SFX based on ghost's HP. Ghost HP [0;2]
    {
        landingOnTheGhostInstance.setParameterByName("Ghost HP", ghostHp);
        landingOnTheGhostInstance.start();
    }
    public void TriggerLandingOnTheSpiderSfx() //Plays a spider SFX. No parameter
    {
        landingOnTheSpiderInstance.start();
    }
    public void TriggerLandingOnTheGreenGooSfx(int greenGooHp) //Plays a SFX based on green goo's HP. Green Goo HP [0;2]
    {
        landingOnTheGreenGooInstance.setParameterByName("Green Goo HP", greenGooHp);
        landingOnTheGreenGooInstance.start();
    }
    public void TriggerLandingOnTheLTrashMonsterSfx(int lTrashMonsterHp) //Plays a SFX based on L trash monster's HP. L Trash Monster HP [0;2]
    {
        landingOnTheLTrashMonsterInstance.setParameterByName("L Trash Monster HP", lTrashMonsterHp);
        landingOnTheLTrashMonsterInstance.start();
    }
    public void TriggerLandingOnTheMTrashMonsterSfx(int mTrashMonsterHp) //Plays a SFX based on M trash monster's HP. M Trash Monster HP [0;1]
    {
        landingOnTheMTrashMonsterInstance.setParameterByName("M Trash Monster HP", mTrashMonsterHp);
        landingOnTheMTrashMonsterInstance.start();
    }
    public void TriggerLandingOnTheSTrashMonsterSfx() //Plays a S trash monster SFX. No parameter
    {
        landingOnTheSTrashMonsterInstance.start();
    }
    public void TriggerLandingOnTheMummySfx(int mummyHp) //Plays a SFX based on mummy's HP. Mummy HP [0;2]
    {
        landingOnTheMummyInstance.setParameterByName("Mummy HP", mummyHp);
        landingOnTheMummyInstance.start();
    }
    
    //Cleanup
    private void OnDestroy()
    {
        landingOnTheGhostInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheGhostInstance.release();
        
        landingOnTheMummyInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheMummyInstance.release();
        
        landingOnTheSpiderInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheSpiderInstance.release();
        
        landingOnTheGreenGooInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheGreenGooInstance.release();
        
        landingOnTheLTrashMonsterInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheLTrashMonsterInstance.release();
        
        landingOnTheMTrashMonsterInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheMTrashMonsterInstance.release();
        
        landingOnTheSTrashMonsterInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheSTrashMonsterInstance.release();
    }
}