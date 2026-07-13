using Slafurry.System.Audio;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("Game Object")]
    [SerializeField] private GameObject settingsMenu;

    [Header("UI References")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle bobbingToggle;
    [SerializeField] private Toggle camShakeToggle;

    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";

    public void OnEnable()
    {
        musicSlider.value = PlayerPrefs.GetFloat(MusicKey, 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat(SFXKey, 0.5f);

        bobbingToggle.isOn = PlayerPrefs.GetInt("Bobbing", 1) == 1;
        camShakeToggle.isOn = PlayerPrefs.GetInt("CamShake", 1) == 1;
    }
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
    }

    public void SetMusicVolume(float value)
    {
        AudioSystem.Instance.UpdateMusicVolume(value);
        PlayerPrefs.Save();

    }

    public void SetSFXVolume(float value)
    {
        AudioSystem.Instance.UpdateSFXVolume(value);
        PlayerPrefs.Save();

    }

    public void SetBobbing(bool value)
    {
        PlayerPrefs.SetInt("Bobbing", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetCamShake(bool value)
    {
        PlayerPrefs.SetInt("CamShake", value ? 1 : 0);
        PlayerPrefs.Save();

    }
}