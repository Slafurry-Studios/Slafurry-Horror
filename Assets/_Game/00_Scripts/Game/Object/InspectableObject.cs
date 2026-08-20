using UnityEngine;
using UnityEngine.Events;

public class InspectableObject : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class InspectEvent : UnityEvent<string, string, GameObject> { }

    public string promptText = "Press E to Inspect";

    [Header("Inspect Data")]
    public string title;

    [TextArea]
    public string description;

    [Header("Events")]
    public InspectEvent OnInspect;

    [SerializeField] private InspectHUD inspectHUD;

    void Awake()
    {
        inspectHUD = FindAnyObjectByType<InspectHUD>();
    }

    public bool CanInteract()
    {
        return true;
    }

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact()
    {
        OnInspect?.Invoke(title, description, gameObject);
        inspectHUD?.ReceiveData(title, description, gameObject);
    }
}