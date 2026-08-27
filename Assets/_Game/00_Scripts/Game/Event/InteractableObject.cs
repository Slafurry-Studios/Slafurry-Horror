using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [TextArea(1, 2)]
    [SerializeField] private string prompt = "Press E to interact";
    [SerializeField] private string failPromptText = "You need the required item to interact";

    [Header("Unity Event")]
    [SerializeField] private UnityEvent onInteract;

    [Header("Requirement")]
    [SerializeField] private bool requireCondition;
    [SerializeField] private GameObject requiredItem;

    [Tooltip("All requirements must pass before interaction is allowed.")]
    [SerializeField] private UnityEvent onRequirementFailed;

    [Header("Behavior")]
    [SerializeField] private bool interactOnce;

    private PlayerHand playerHand;

    private bool hasBeenInteracted;

    void Awake()
    {
        playerHand = FindAnyObjectByType<PlayerHand>();
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        if (requireCondition && playerHand.GetCurrentItem() != requiredItem)
        {
            onRequirementFailed?.Invoke();

            PlayerInteract.instance.ShowFailMessage(
                failPromptText,
                2f
            );

            return;
        }

        hasBeenInteracted = true;
        onInteract?.Invoke();
    }

    public bool CanInteract()
    {
        return !interactOnce || !hasBeenInteracted;
    }

    public string GetPromptText()
    {
        return prompt;
    }
}