using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DelayTrigger : BaseTrigger
{
    [SerializeField] private float delay = 1f;
    [SerializeField] private UnityEvent onComplete;

    private bool isWaiting;

    public void TriggerDelay()
    {
        if (!CanTrigger() || isWaiting) return;

        isWaiting = true;
        AddTriggerCount();

        StartCoroutine(DelayCoroutine());
    }

    private IEnumerator DelayCoroutine()
    {
        yield return new WaitForSeconds(delay);

        isWaiting = false;
        onComplete?.Invoke();
    }
}