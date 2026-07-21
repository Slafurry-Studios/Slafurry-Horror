using Slafurry.System.Scene;
using UnityEngine;

public class ChangeSceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName = "NextScene";

    public void ChangeScene()
    {
        SceneLoader.Instance.LoadScene(sceneName);
    }
}