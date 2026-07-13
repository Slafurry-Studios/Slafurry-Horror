using Slafurry.System.Audio;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    [Header("SFX Source")]
    [SerializeField] private GameObject sfxSource;

    private GameObject Source => sfxSource != null ? sfxSource : gameObject;

    public void PlayMusic(string trackName)
    {
        Audio.PlayMusic(trackName);
    }

    public void StopMusic(float fadeDuration)
    {
        Audio.StopMusic(fadeDuration);
    }

    public void PlaySfx(string categoryKey)
    {
        if (!TryParse(categoryKey, out string category, out string key)) return;

        Audio.PlaySFX3D(category, key, Source.transform.position);
    }

    public void StopSFX(string categoryKey)
    {
        if (!TryParse(categoryKey, out string category, out string key)) return;

        if (!string.IsNullOrEmpty(key)) Audio.StopSFX(category, key);
        else if (!string.IsNullOrEmpty(category)) Audio.StopSFX(category);
    }

    public void StopAllSFX()
    {
        Audio.StopSFX();
    }

    private bool TryParse(string categoryKey, out string category, out string key)
    {
        category = "";
        key = "";

        if (string.IsNullOrEmpty(categoryKey))
        {
            Debug.LogWarning($"[{nameof(AudioTrigger)}] categoryKey kosong.");
            return false;
        }

        var parts = categoryKey.Split(':');
        category = parts.Length > 0 ? parts[0] : "";
        key = parts.Length > 1 ? parts[1] : "";

        if (string.IsNullOrEmpty(category))
        {
            Debug.LogWarning($"[{nameof(AudioTrigger)}] Format salah: '{categoryKey}'. Gunakan 'category:key' atau 'category'.");
            return false;
        }

        return true;
    }
}