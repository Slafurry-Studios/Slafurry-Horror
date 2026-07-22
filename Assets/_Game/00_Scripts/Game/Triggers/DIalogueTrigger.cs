using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private UnityEvent onComplete;
    private bool _played;
    
    public void TryPlay()
    {
        if (playOnce && _played) return;
        if (DialoguePlayer.instance == null) return;
        DialoguePlayer.instance.Play(dialogue, onComplete);
        _played = true;
    }
}