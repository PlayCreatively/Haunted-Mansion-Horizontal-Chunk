using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MusicManager : MonoBehaviour
{
    [EventRef]
    public string loopEvent; // Drag your event:/Loop Playing here

    public int BGMusicValue = 1; // Editable in Inspector at runtime!

    private EventInstance musicInstance;

    void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(loopEvent);
        musicInstance.start();
        musicInstance.setParameterByName("BGMusic", BGMusicValue);
    }

    void Update()
    {
        // This checks every frame and applies the current value from Inspector
        musicInstance.setParameterByName("BGMusic", BGMusicValue);
    }

    private void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }
}