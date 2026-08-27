using UnityEngine;
using Slafurry.System.Pause;

public class PauseDebug : MonoBehaviour
{
    [Header("Pause Key")]
    [SerializeField] private string key = "Debug";

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.P;
    [SerializeField] private KeyCode resumeKey = KeyCode.O;
    [SerializeField] private KeyCode toggleKey = KeyCode.T;

    [Header("Info (Read Only)")]
    [SerializeField] private bool isPausedByThisKey;
    [SerializeField] private bool isGamePaused;

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            Pause.On(key);
            Debug.Log($"[PauseDebug] Pause requested with key: \"{key}\"");
        }

        if (Input.GetKeyDown(resumeKey))
        {
            Pause.Off(key);
            Debug.Log($"[PauseDebug] Resume requested with key: \"{key}\"");
        }

        if (Input.GetKeyDown(toggleKey))
        {
            Pause.Toggle(key);
            Debug.Log($"[PauseDebug] Toggle with key: \"{key}\"");
        }

        isPausedByThisKey = Pause.IsPausedBy(key);
        isGamePaused = Pause.IsPaused;
    }
}