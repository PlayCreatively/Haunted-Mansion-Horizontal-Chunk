using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAudioManager : MonoBehaviour
{
    public static FMODAudioManager Instance { get; private set; } //Singleton instance

    //Event Instances for enemy SFX
    EventInstance landingOnTheGhostInstance;
    EventInstance landingOnTheSpiderInstance;
    EventInstance landingOnTheGreenGooInstance;
    EventInstance landingOnTheLTrashMonsterInstance;
    EventInstance landingOnTheMTrashMonsterInstance;
    EventInstance landingOnTheSTrashMonsterInstance;
    EventInstance landingOnTheMummyInstance;
    EventInstance landingOnTheCarpetMonsterInstance;

    //Event instances for character SFX
    EventInstance jumpingOffTheBalconyInstance;
    EventInstance onDashStartsInstance;
    EventInstance itemThrownInstance;
    EventInstance itemPickedUpInstance;
    EventInstance itemDroppedInstance;
    EventInstance stunnedInstance;

    //Event instances for new spawn SFX
    EventInstance newGhostSpawnedInstance;
    EventInstance newMummySpawnedInstance;
    EventInstance newGreenGooSpawnedInstance;
    EventInstance newSpiderSpawnedInstance;
    EventInstance newTrashMonsterSpawnedInstance;

    //Event instances for environment
    EventInstance bgMusicInstance;

    //Event instances for room
    EventInstance roomCleanedInstance;
    EventInstance roomCheckOutInstance; // TODO: Implement in FMOD
    EventInstance roomBookedInstance; // TODO: Implement in FMOD
    EventInstance ResourcePlacedInRoomInstance; // TODO: Implement in FMOD

    //Assigning paths
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //==============//
        landingOnTheGhostInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GHOST/land on ghost");
        landingOnTheSpiderInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/SPIDER/land on spider");
        landingOnTheGreenGooInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GOO/land on green goo");
        landingOnTheLTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/TRASH MONSTER/land on l trash monster");
        landingOnTheMTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/TRASH MONSTER/land on m trash monster");
        landingOnTheMTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/TRASH MONSTER/land on s trash monster");
        landingOnTheMummyInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/MUMMY/land on mummy");
        landingOnTheCarpetMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/CARPET/land on carpet monster");
        //==============//
        newGhostSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GHOST/new ghost spawned");
        newMummySpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/MUMMY/new mummy spawned");
        newGreenGooSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GOO/new green goo spawned");
        newSpiderSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/SPIDER/new spider spawned");
        newTrashMonsterSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/TRASH MONSTER/new trash monster spawned");
        //==============//
        jumpingOffTheBalconyInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/jump off the balcony");
        onDashStartsInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/on dash starts");
        itemPickedUpInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item picked up");
        itemThrownInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item thrown");
        itemDroppedInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item dropped");
        stunnedInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/stunned");
        //==============//
        bgMusicInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/bg music");
        roomCleanedInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/room cleaned");
    }

    //Starting looped tracks
    void Start()
    {
        bgMusicInstance.start();
    }

    //Public methods: MONSTERS

    public void TriggerLandingOnTheGhostSfx(int ghostHp) //Plays a SFX based on ghost's HP. Ghost HP [0;2]
    {
        landingOnTheGhostInstance.setParameterByName("Ghost HP", ghostHp);
        landingOnTheGhostInstance.start();
    }
    public void TriggerNewGhostSpawnedSfx() //Plays a ghost spawn SFX. No parameter
    {
        newGhostSpawnedInstance.start();
    }
    public void TriggerLandingOnTheSpiderSfx() //Plays a spider SFX. No parameter
    {
        landingOnTheSpiderInstance.start();
    }
    public void TriggerNewSpiderSpawnedSfx() //Plays a spider spawn SFX. No parameter
    {
        newSpiderSpawnedInstance.start();
    }
    public void TriggerLandingOnTheGreenGooSfx(int greenGooHp) //Plays a SFX based on green goo's HP. Green Goo HP [0;2]
    {
        landingOnTheGreenGooInstance.setParameterByName("Green Goo HP", greenGooHp);
        landingOnTheGreenGooInstance.start();
    }
    public void TriggerNewGreenGooSpawnedSfx() //Plays a green goo spawn SFX. No parameter
    {
        newGreenGooSpawnedInstance.start();
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
    public void TriggerNewTrashMonsterSpawnedSfx() //Plays a trash monster spawn SFX. No parameter
    {
        newTrashMonsterSpawnedInstance.start();
    }
    public void TriggerLandingOnTheMummySfx(int mummyHp) //Plays a SFX based on mummy's HP. Mummy HP [0;2]
    {
        landingOnTheMummyInstance.setParameterByName("Mummy HP", mummyHp);
        landingOnTheMummyInstance.start();
    }
    public void TriggerNewMummySpawnedSfx() //Plays a mummy spawn SFX. No parameter
    {
        newMummySpawnedInstance.start();
    }
    public void TriggerLandingOnTheCarpetMonster() //Plays a landing on the carpet monster SFX. No parameter
    {
        landingOnTheCarpetMonsterInstance.start();
    }

    //Public methods: CHARACTER
    //Plays an item thrown SFX. No parameter
    public void TriggerItemThrownSfx() => itemThrownInstance.start();
    //Plays a jumping off the balcony SFX. No parameter
    public void TriggerJumpingOffTheBalconySfx() => jumpingOffTheBalconyInstance.start();
    //Plays a dash SFX. No parameter
    public void TriggerOnDashStartsSfx() => onDashStartsInstance.start();
    //Plays an item dropped SFX. No parameter
    public void TriggerItemDroppedSfx() => itemDroppedInstance.start();
    //Plays an item picked up SFX. No parameter
    public void TriggerItemPickedUpSfx() => itemPickedUpInstance.start();
    //Plays a stunned SFX. No parameter
    public void TriggerStunnedSfx() => stunnedInstance.start();

    ///----Public methods: Environment-----///
    //Plays room cleaned SFX. No parameter
    public void TriggerRoomCleanedSfx() => roomCleanedInstance.start();

    //Plays room check out SFX. No parameter
    public void TriggerRoomCheckOutSfx() => roomCheckOutInstance.start();

    //Plays room booked SFX. No parameter
    public void TriggerRoomBookedSfx() => roomBookedInstance.start();

    public void TriggerResourcePlacedInRoom() => ResourcePlacedInRoomInstance.start(); 


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
        landingOnTheCarpetMonsterInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheCarpetMonsterInstance.release();
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
        stunnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        stunnedInstance.release();
        //==============//
        newTrashMonsterSpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newTrashMonsterSpawnedInstance.release();
        newMummySpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newMummySpawnedInstance.release();
        newGhostSpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newGhostSpawnedInstance.release();
        newSpiderSpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newSpiderSpawnedInstance.release();
        newGreenGooSpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newGreenGooSpawnedInstance.release();
    }
}