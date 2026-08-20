using UnityEngine;

public abstract class BaseTrigger : MonoBehaviour
{
    [Header("Trigger Limit")]
    [SerializeField] protected int playLimit;
    [SerializeField] protected bool unlimited = true;
    protected int currentPlayCount = 0;

    protected bool CanTrigger()
    {
        return currentPlayCount < playLimit || unlimited;
    }

    protected void AddTriggerCount(int count = 1)
    {
        currentPlayCount += count;
    }
}