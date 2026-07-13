using UnityEngine;

public class DialogueTriggerZone : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private bool playOnStart = false;   
    [SerializeField] private bool playOnce = true;
    [SerializeField] private string playerTag = "Player";

    private bool _played;

    private void Start()
    {
        if (playOnStart) TryPlay();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;   
        TryPlay();
    }

    private void TryPlay()
    {
        if (playOnce && _played) return;
        if (DialoguePlayer.instance == null) return;
        DialoguePlayer.instance.Play(dialogue);
        _played = true;
    }
}