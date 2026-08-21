using Slafurry.System.Audio;
using UnityEngine;

public class MusicTrigger : BaseTrigger
{
    [Header("Music Track")]
    [SerializeField] private string trackName;

    public void PlayMusic()
    {
        if (!CanTrigger()) return;
        Audio.PlayMusic(trackName);
        AddTriggerCount();
    }

    public void StopMusic(float fadeDuration)
    {
        if (!CanTrigger()) return;
        Audio.StopMusic(fadeDuration);
        AddTriggerCount();
    }
}