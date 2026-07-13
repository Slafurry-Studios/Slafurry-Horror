using Slafurry.System.Pause;
using Slafurry.System.Scene;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [Header("Pause Key")]
    [SerializeField] private string pauseKey = "Game_Pause";

    [Header("UI References")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject settings;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuScene = "MaiMenu";

    public void Continue()
    {
        Pause.Off(pauseKey);
        pauseUI.SetActive(false);
    }

    public void OpenSettings()
    {
        settings.SetActive(true);
    }

    public void BackToMenu()
    {
        Pause.ForceResume();
        SceneLoader.Instance.LoadScene(mainMenuScene);
    }
}