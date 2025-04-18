using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAudioManager : MonoBehaviour
{
    public static FMODAudioManager Instance { get; private set; } //Singleton instance

    //Event Instances for enemy SFX
    private EventInstance landingOnTheGhostInstance;
    private EventInstance landingOnTheSpiderInstance;
    private EventInstance landingOnTheGreenGooInstance;
    private EventInstance landingOnTheLTrashMonsterInstance;
    private EventInstance landingOnTheMTrashMonsterInstance;
    private EventInstance landingOnTheSTrashMonsterInstance;
    private EventInstance landingOnTheMummyInstance;
    
    //Event instances for character SFX
    private EventInstance jumpingOffTheBalconyInstance;
    private EventInstance onDashStartsInstance;
    private EventInstance itemThrownInstance;
    private EventInstance itemPickedUpInstance;
    private EventInstance itemDroppedInstance;
    
    // Assigning paths
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        landingOnTheGhostInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on ghost");
        landingOnTheSpiderInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on spider");
        landingOnTheGreenGooInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on green goo");
        landingOnTheLTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on l trash monster");
        landingOnTheMTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on m trash monster");
        landingOnTheMTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on s trash monster");
        landingOnTheMummyInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/land on mummy");

        jumpingOffTheBalconyInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/jump off the balcony");
        onDashStartsInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/on dash starts");
        itemPickedUpInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item picked up");
        itemThrownInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item thrown");
        itemDroppedInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item dropped");
    }

    //Starting looped tracks
    void Start()
    {
        //exampleInstance.start();
    }

    //Public methods: MONSTERS

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
    
    //Public methods: CHARACTER
    public void TriggerItemThrownSfx() //Plays a throwing SFX. No parameter
    {
        itemThrownInstance.start();
    }
    public void TriggerJumpingOffTheBalconySfx() //Plays a landing SFX. No parameter
    {
        jumpingOffTheBalconyInstance.start();
    }
    public void TriggerOnDashStartsSfx() //Plays a dash SFX. No parameter
    {
        onDashStartsInstance.start();
    }
    public void TriggerItemDroppedSfx(int itemType) //Plays a SFX based on item's type. Item Type [0;4]
    {
        itemDroppedInstance.setParameterByName("Item Type", itemType);
        itemDroppedInstance.start();
    }
    public void TriggerItemPickedUpSfx(int itemType) //Plays a SFX based on item's type. Item Type [0;4]
    {
        itemPickedUpInstance.setParameterByName("Item Type", itemType);
        itemPickedUpInstance.start();
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
        //==============//
        onDashStartsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        onDashStartsInstance.release();
        jumpingOffTheBalconyInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        jumpingOffTheBalconyInstance.release();
        itemDroppedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        itemDroppedInstance.release();
        itemThrownInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        itemThrownInstance.release();
        itemPickedUpInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        itemPickedUpInstance.release();
    }
}