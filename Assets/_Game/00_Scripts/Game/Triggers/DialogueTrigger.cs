using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : BaseTrigger
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private UnityEvent onComplete;
    public void TryPlay()
    {
        if (!CanTrigger()) return;

        if (DialoguePlayer.instance == null) return;
        if (!DialoguePlayer.instance.Play(dialogue, onComplete)) return;

        AddTriggerCount();
    }
}