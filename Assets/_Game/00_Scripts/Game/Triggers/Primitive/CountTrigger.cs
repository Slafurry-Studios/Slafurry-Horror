using UnityEngine;
using UnityEngine.Events;

public class CountTrigger : BaseTrigger
{
    [Header("Count")]
    [SerializeField] private int targetCount = 3;
    [SerializeField] private int currentCount;

    [Header("Event")]
    [SerializeField] private UnityEvent onReached;

    public void TriggerCount()
    {
        if (!CanTrigger())
            return;

        currentCount++;

        if (currentCount >= targetCount)
        {
            onReached?.Invoke();
            AddTriggerCount();
        }
    }

    public void ResetCount()
    {
        currentCount = 0;
    }
}