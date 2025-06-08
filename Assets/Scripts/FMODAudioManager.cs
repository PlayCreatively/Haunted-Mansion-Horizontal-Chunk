using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAudioManager : MonoBehaviour
{
    public static FMODAudioManager Instance { get; private set; } //Singleton instance

    #region LandingOnEnemiesInstances
    EventInstance landingOnTheGhostInstance;
    EventInstance landingOnTheGreenGooInstance;
    EventInstance landingOnTheMummyInstance;
    EventInstance landingOnTheWormMonsterInstance;
    #endregion
    #region CharacterSFXInstances
    EventInstance jumpingOffTheBalconyInstance;
    EventInstance onDashStartsInstance;
    EventInstance itemThrownInstance;
    EventInstance itemPickedUpInstance;
    EventInstance itemDroppedInstance;
    EventInstance stunnedInstance;
    EventInstance itemSelectionInTheBagInstance;
    EventInstance uiHoverInstance;
    EventInstance uiSelectInstance;
    #endregion
    #region NewEnemySpawnsSFXInstances
    EventInstance newGhostSpawnedInstance;
    EventInstance newMummySpawnedInstance;
    EventInstance newGreenGooSpawnedInstance;
    EventInstance newWormMonsterSpawnedInstance;
    #endregion
    #region EnvironmentSFXInstances
    EventInstance bgMusicInstance;
    EventInstance mainMenuLeaderboardInstance;
    EventInstance loseMenuInstance;
    EventInstance gameOverInstance; //Rename where needed!
    EventInstance runningOutOfTimeInstance;
    EventInstance laundryDoneInstance;
    EventInstance laundryStartInstance;
    EventInstance scoreFlyInstance;
    EventInstance anotherCalculationInstance;
    #endregion
    #region RoomSFXInstances
    EventInstance roomCleanedInstance;
    EventInstance roomCheckInInstance;
    EventInstance roomBookedInstance;
    EventInstance resourcePlacedInRoomInstance;
    EventInstance resourcePlacedInMachineInstance;

    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            //DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else return;

            #region LandingOnMonstersPaths
            landingOnTheGhostInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GHOST/land on ghost");
        landingOnTheGreenGooInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GOO/land on green goo");
        landingOnTheMummyInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/MUMMY/land on mummy");
        landingOnTheWormMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/WORM/land on worm");
        #endregion
        #region NewMonsterSpawnsPaths
        newGhostSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GHOST/new ghost spawned");
        newMummySpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/MUMMY/new mummy spawned");
        newGreenGooSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GOO/new green goo spawned");
        newWormMonsterSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/WORM/new worm spawned");
        #endregion
        #region CharacterPaths
        jumpingOffTheBalconyInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/jump off the balcony");
        onDashStartsInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/on dash starts");
        itemPickedUpInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item picked up");
        itemThrownInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item thrown");
        itemDroppedInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item dropped");
        stunnedInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/stunned");
        itemSelectionInTheBagInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item selection in the bag");
        uiHoverInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/ui hover");
        uiSelectInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/ui select");
        #endregion
        #region EnvironmentAndRoomsPaths
        bgMusicInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/bg music");
        mainMenuLeaderboardInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/menu and leaderboard");
        loseMenuInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/fail menu");
        gameOverInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/game over");
        roomCheckInInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/room check in");
        roomBookedInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/room booked");
        resourcePlacedInRoomInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/resource placed in the room");
        resourcePlacedInMachineInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/resource placed in the machine");
        roomCleanedInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/room cleaned");
        runningOutOfTimeInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/ticktack");
        laundryDoneInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/laundry done");
        laundryStartInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/laundry start");
        scoreFlyInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/score fly");
        anotherCalculationInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/other calculation");

        #endregion

        mainMenuLeaderboardInstance.start();
    }
    void Start()
    {
    }

    #region GHOST
    public void TriggerLandingOnTheGhostSfx(int ghostHp) //Plays a SFX based on ghost's HP. Ghost HP [0;2]
    {
        landingOnTheGhostInstance.setParameterByName("Ghost HP", ghostHp);
        landingOnTheGhostInstance.start();
    }
    public void TriggerNewGhostSpawnedSfx() //Plays a ghost spawn SFX. No parameter
    {
        newGhostSpawnedInstance.start();
    }
    #endregion
    #region GREENGOO
    public void TriggerLandingOnTheGreenGooSfx(int greenGooHp) //Plays a SFX based on green goo's HP. Green Goo HP [0;2]
    {
        landingOnTheGreenGooInstance.setParameterByName("Green Goo HP", greenGooHp);
        landingOnTheGreenGooInstance.start();
    }
    public void TriggerNewGreenGooSpawnedSfx() //Plays a green goo spawn SFX. No parameter
    {
        newGreenGooSpawnedInstance.start();
    }
    #endregion
    #region WORM
    private void TriggerLandingOnTheWormMonsterSfx(int hp)
    {
        landingOnTheWormMonsterInstance.setParameterByName("Towel HP", hp);
        landingOnTheWormMonsterInstance.start(); // TODO: Implement in FMOD
    }
    public void TriggerNewWormMonsterSpawnedSfx() //Plays a worm monster spawn SFX. No parameter
    {
        newWormMonsterSpawnedInstance.start();
    }
    #endregion
    #region MUMMY
    public void TriggerLandingOnTheMummySfx(int mummyHp) //Plays a SFX based on mummy's HP. Mummy HP [0;2]
    {
        landingOnTheMummyInstance.setParameterByName("Mummy HP", mummyHp);
        landingOnTheMummyInstance.start();
    }
    public void TriggerNewMummySpawnedSfx() //Plays a mummy spawn SFX. No parameter
    {
        newMummySpawnedInstance.start();
    }
    #endregion
    #region CHARACTERSFX
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
    //Plays an item selection in the bag SFX. No parameter
    public void TriggerItemSelectionInTheBagSfx() => itemSelectionInTheBagInstance.start();
    public void TriggerUIHoverSfx() => uiHoverInstance.start();
    public void TriggerUISelectSfx() => uiSelectInstance.start();
    #endregion
    #region ROOMSFX
    //Plays room cleaned SFX. No parameter
    public void TriggerRoomCleanedSfx() => roomCleanedInstance.start();
    //Plays room booked SFX. No parameter
    public void TriggerRoomBookedSfx() => roomBookedInstance.start();
    public void TriggerResourcePlacedInRoom() => resourcePlacedInRoomInstance.start();
    public void TriggerResourcePlacedInMachine() => resourcePlacedInMachineInstance.start();

    public void TriggerRoomCheckInSfx() => roomCheckInInstance.start();
    #endregion
    #region ENVIRONMENTSFX
    public void TriggerGameOver()
    {
        gameOverInstance.start();
        bgMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void UpdateRunningOutOfTimeSfx(bool active) //Plays a SFX based on how much time is left.
    {
        if (active)
        {
            runningOutOfTimeInstance.start();
            bgMusicInstance.setParameterByName("Music Speed Up", 1);
        }

        else
        {
            runningOutOfTimeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bgMusicInstance.setParameterByName("Music Speed Up", 0);

        }

    }

    public void StartMainTheme() => bgMusicInstance.start();
    public void StopMainTheme() => bgMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    public void StartMenuLeaderboardTheme() => mainMenuLeaderboardInstance.start();
    public void StopMenuLeaderboardTheme() => mainMenuLeaderboardInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    public void StartLoseMenuTheme() => loseMenuInstance.start();
    public void StopLoseMenuTheme() => loseMenuInstance.stop(0);

    public void TriggerTotalCalculationSfx() => scoreFlyInstance.start();

    public void TriggerAnotherCalculationSfx(float pitch)
    {
        anotherCalculationInstance.getPlaybackState(out var state);
        if (pitch != 1)
        {
            if(state != FMOD.Studio.PLAYBACK_STATE.PLAYING)
                anotherCalculationInstance.start();

            anotherCalculationInstance.setParameterByName("Pitch 2", pitch, ignoreseekspeed: true);
        }
        else
        {
            anotherCalculationInstance.stop(0);   
        }
    }


    //Plays a laundry done SFX. No parameter
    public void TriggerLaundryDoneSfx() => laundryDoneInstance.start();
    public void TriggerLaundryStartSfx() => laundryStartInstance.start();
    #endregion
    public void TriggerLandingOnEnemySfx(EnemyType type, int hp)
    {
        switch (type)
        {
            case EnemyType.Ghost:
                TriggerLandingOnTheGhostSfx(hp);
                break;
            case EnemyType.Mummy:
                TriggerLandingOnTheMummySfx(hp);
                break;
            case EnemyType.Goo:
                TriggerLandingOnTheGreenGooSfx(hp);
                break;
            case EnemyType.TowelMonster:
                TriggerLandingOnTheWormMonsterSfx(hp);
                break;
            default:
                Debug.LogError("Invalid enemy type");
                break;
        }
    }
    private void OnDestroy()
    {
        #region EnvironmentAndRoomsDestroys
        roomCheckInInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        roomCheckInInstance.release();
        roomBookedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        roomBookedInstance.release();
        resourcePlacedInRoomInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        resourcePlacedInRoomInstance.release();
        resourcePlacedInMachineInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        resourcePlacedInMachineInstance.release();
        roomCleanedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        roomCleanedInstance.release();
        bgMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        bgMusicInstance.release();
        mainMenuLeaderboardInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        mainMenuLeaderboardInstance.release();
        loseMenuInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        loseMenuInstance.release();
        gameOverInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        gameOverInstance.release();
        runningOutOfTimeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        runningOutOfTimeInstance.release();
        laundryDoneInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        laundryDoneInstance.release();
        laundryStartInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        laundryStartInstance.release();
        #endregion
        #region LandingOnEnemiesDestroys
        landingOnTheGhostInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheGhostInstance.release();
        landingOnTheMummyInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheMummyInstance.release();
        landingOnTheGreenGooInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheGreenGooInstance.release();
        landingOnTheWormMonsterInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        landingOnTheWormMonsterInstance.release();
        #endregion
        #region CharacterDestroys
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
        itemSelectionInTheBagInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        itemSelectionInTheBagInstance.release();
        uiHoverInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        uiHoverInstance.release();
        uiSelectInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        uiSelectInstance.release();
        #endregion
        #region NewMonsterSpawnsDestroy
        newMummySpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newMummySpawnedInstance.release();
        newGhostSpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newGhostSpawnedInstance.release();
        newGreenGooSpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newGreenGooSpawnedInstance.release();
        newWormMonsterSpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        newWormMonsterSpawnedInstance.release();
        #endregion
    }
}