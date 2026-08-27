using Slafurry.System.Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    [Header("Reset Scene")]
    [SerializeField] private KeyCode resetKey = KeyCode.Q;

    private void Update()
    {
        if (Input.GetKeyDown(resetKey))
        {
            SceneSystem.Load(SceneManager.GetActiveScene().name);
        }
    }
}