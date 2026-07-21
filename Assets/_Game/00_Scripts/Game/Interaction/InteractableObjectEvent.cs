using UnityEngine;
using UnityEngine.Events;

public class InteractableObjectEvent : MonoBehaviour
{
    [Header("Prompt")]
    [TextArea(1, 2)]
    public string promptText = "Press E to interact";

    [Header("Unity Event")]
    public UnityEvent onInteract;

    [Header("Behavior")]
    public bool interactOnce = false;

    private bool hasBeenInteracted;

    public void Interact()
    {
        if (interactOnce && hasBeenInteracted)
        {
            return;
        }

        hasBeenInteracted = true;
        onInteract?.Invoke();
    }

    public string GetPromptText()
    {
        return promptText;
    }
}
