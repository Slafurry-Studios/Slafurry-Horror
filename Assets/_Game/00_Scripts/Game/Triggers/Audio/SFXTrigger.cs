using Slafurry.System.Audio;
using UnityEngine;

public class SFXTrigger : BaseTrigger
{
    [Header("SFX Source")]
    [SerializeField] private GameObject sfxSource;

    [Header("Audio Data")]
    [SerializeField] private string category;
    [SerializeField] private string key;

    private GameObject Source => sfxSource != null ? sfxSource : gameObject;
    public void PlaySfx()
    {
        if (!CanTrigger()) return;
        Audio.PlaySFX3D(category, key, Source.transform.position);
        AddTriggerCount();
    }

    public void StopSFX()
    {
        if (!CanTrigger()) return;
        if (!string.IsNullOrEmpty(key)) Audio.StopSFX(category, key);
        else if (!string.IsNullOrEmpty(category)) Audio.StopSFX(category);
        AddTriggerCount();
    }

    public void StopAllSFX()
    {
        if (!CanTrigger()) return;
        Audio.StopSFX();
        AddTriggerCount();
    }
}