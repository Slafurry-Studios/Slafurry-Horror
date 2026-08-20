using Slafurry.System.Scene;
using UnityEngine;

public class ChangeSceneTrigger : BaseTrigger
{
    [SerializeField] private string sceneName = "NextScene";

    public void ChangeScene()
    {
        if (!CanTrigger()) return;
        SceneLoader.Instance.LoadScene(sceneName);
        AddTriggerCount();
    }
}