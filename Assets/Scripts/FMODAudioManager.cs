using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODAudioManager : MonoBehaviour
{
    public static FMODAudioManager Instance { get; private set; } //Singleton instance
    
    #region LandingOnEnemiesInstances
        EventInstance landingOnTheGhostInstance;
        EventInstance landingOnTheSpiderInstance;
        EventInstance landingOnTheGreenGooInstance;
        EventInstance landingOnTheLTrashMonsterInstance;
        EventInstance landingOnTheMTrashMonsterInstance;
        EventInstance landingOnTheSTrashMonsterInstance;
        EventInstance landingOnTheMummyInstance;
        EventInstance landingOnTheCarpetMonsterInstance;
        EventInstance landingOnTheWormMonsterInstance;
    #endregion
    #region CharacterSFXInstances
        EventInstance jumpingOffTheBalconyInstance;
        EventInstance onDashStartsInstance;
        EventInstance itemThrownInstance;
        EventInstance itemPickedUpInstance;
        EventInstance itemDroppedInstance;
        EventInstance stunnedInstance;
    #endregion
    #region NewEnemySpawnsSFXInstances
        EventInstance newGhostSpawnedInstance;
        EventInstance newMummySpawnedInstance;
        EventInstance newGreenGooSpawnedInstance;
        EventInstance newSpiderSpawnedInstance;
        EventInstance newTrashMonsterSpawnedInstance;
        EventInstance newWormMonsterSpawnedInstance;
    #endregion
    #region EnvironmentSFXInstances
        EventInstance bgMusicInstance;
        EventInstance gameOverInstance; //Rename where needed!
        EventInstance runningOutOfTimeInstance;
    #endregion
    #region RoomSFXInstances
        EventInstance roomCleanedInstance;
        EventInstance roomCheckOutInstance; 
        EventInstance roomBookedInstance;
        EventInstance resourcePlacedInRoomInstance;
    #endregion
    
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
        
        #region LandingOnMonstersPaths
                landingOnTheGhostInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GHOST/land on ghost");
                landingOnTheSpiderInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/SPIDER/land on spider");
                landingOnTheGreenGooInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GOO/land on green goo");
                landingOnTheLTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/TRASH MONSTER/land on l trash monster");
                landingOnTheMTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/TRASH MONSTER/land on m trash monster");
                landingOnTheMTrashMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/TRASH MONSTER/land on s trash monster");
                landingOnTheMummyInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/MUMMY/land on mummy");
                landingOnTheCarpetMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/CARPET/land on carpet monster");
                landingOnTheWormMonsterInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/WORM/land on worm");
        #endregion
        #region NewMonsterSpawnsPaths
                newGhostSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GHOST/new ghost spawned");
                newMummySpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/MUMMY/new mummy spawned");
                newGreenGooSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/GOO/new green goo spawned");
                newSpiderSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/SPIDER/new spider spawned");
                newTrashMonsterSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/TRASH MONSTER/new trash monster spawned");
                newWormMonsterSpawnedInstance = RuntimeManager.CreateInstance("event:/Monsters SFX Events/WORM/new worm spawned");
        #endregion
        #region CharacterPaths
                jumpingOffTheBalconyInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/jump off the balcony");
                onDashStartsInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/on dash starts");
                itemPickedUpInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item picked up");
                itemThrownInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item thrown");
                itemDroppedInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/item dropped");
                stunnedInstance = RuntimeManager.CreateInstance("event:/Character SFX Events/stunned");
        #endregion
        #region EnvironmentAndRoomsPaths
                bgMusicInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/bg music");
                gameOverInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/game over");
                roomCheckOutInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/room check out");
                roomBookedInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/room booked");
                resourcePlacedInRoomInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/resource placed in the room");
                roomCleanedInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/room cleaned");
                runningOutOfTimeInstance = RuntimeManager.CreateInstance("event:/Environment SFX Events/running out of time");
        #endregion
    }
    void Start()
    {
        bgMusicInstance.start();
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
    #region SPIDER
        public void TriggerLandingOnTheSpiderSfx() //Plays a spider SFX. No parameter
        {
            landingOnTheSpiderInstance.start();
        }
        public void TriggerNewSpiderSpawnedSfx() //Plays a spider spawn SFX. No parameter
        {
            newSpiderSpawnedInstance.start();
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
    #region TRASHMONSTER
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
    #endregion
    #region WORM
        private void TriggerLandingOnTheWormMonsterSfx(int hp)
        {
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
    #region CARPET
        public void TriggerLandingOnTheCarpetMonster() //Plays a landing on the carpet monster SFX. No parameter
        {
            landingOnTheCarpetMonsterInstance.start();
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
    #endregion
    #region ROOMSFX
        //Plays room cleaned SFX. No parameter
        public void TriggerRoomCleanedSfx() => roomCleanedInstance.start();
        //Plays room check out SFX. No parameter
        public void TriggerRoomCheckOutSfx() => roomCheckOutInstance.start();
        //Plays room booked SFX. No parameter
        public void TriggerRoomBookedSfx() => roomBookedInstance.start();
        public void TriggerResourcePlacedInRoom() => resourcePlacedInRoomInstance.start();
    #endregion
    #region ENVIRONMENTSFX
        public void TriggerGameOver()
        {
            gameOverInstance.start();
            bgMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        public void TriggerRunningOutOfTimeSfx(int timeLeft) //Plays a SFX based on how much time is left. Time Left [??]
        {
            runningOutOfTimeInstance.setParameterByName("Time Left", timeLeft);
            runningOutOfTimeInstance.start();
        }
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
            case EnemyType.Spider:
                TriggerLandingOnTheSpiderSfx();
                break;
            case EnemyType.Goo:
                TriggerLandingOnTheGreenGooSfx(hp);
                break;
            case EnemyType.Trash:
                TriggerLandingOnTheLTrashMonsterSfx(hp);
                break;
            case EnemyType.Worm:
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
                roomCheckOutInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                roomCheckOutInstance.release();
                roomBookedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                roomBookedInstance.release();
                resourcePlacedInRoomInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                resourcePlacedInRoomInstance.release();
                roomCleanedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                roomCleanedInstance.release();
                bgMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                bgMusicInstance.release();
                gameOverInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                gameOverInstance.release();
                runningOutOfTimeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                runningOutOfTimeInstance.release();
        #endregion
        #region LandingOnEnemiesDestroys
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
        #endregion
        #region NewMonsterSpawnsDestroy
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
                newWormMonsterSpawnedInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                newWormMonsterSpawnedInstance.release();
        #endregion
    }
}