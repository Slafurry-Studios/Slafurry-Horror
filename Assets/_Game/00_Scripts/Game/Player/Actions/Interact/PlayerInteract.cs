using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInteract : MonoBehaviour
{
    public static PlayerInteract instance;

    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }

    [Header("References")]
    public Transform viewTransform;
    public Camera viewCamera;

    [Header("Detection")]
    public float interactRange = 3f;
    public LayerMask interactableMask;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Prompt")]
    public TMP_Text promptText;

    [Header("Events")]
    public StringEvent OnPromptShown = new StringEvent();
    public UnityEvent OnPromptHidden = new UnityEvent();
    public UnityEvent OnInteracted = new UnityEvent();

    public bool InteractionEnabled { get; private set; } = true;

    private InteractableObjectEvent currentInteractable;
    private Coroutine temporaryMessageRoutine;

    private void Awake()
    {
        instance = this;

        if (viewCamera == null)
            viewCamera = Camera.main;

        if (viewCamera != null && viewTransform == null)
            viewTransform = viewCamera.transform;

        if (viewTransform == null)
            viewTransform = transform;

        if (promptText != null)
            promptText.enabled = false;
    }

    private void Update()
    {
        if (!InteractionEnabled)
        {
            ClearCurrentInteractable();
            return;
        }

        DetectInteractable();

        if (currentInteractable != null &&
            currentInteractable.CanInteract() &&
            Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact();
            OnInteracted.Invoke();
            HidePrompt();
        }
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        InteractionEnabled = isEnabled;

        if (!isEnabled)
            ClearCurrentInteractable();
    }

    public void ShowTemporaryMessage(string message, float duration)
    {
        if (temporaryMessageRoutine != null)
            StopCoroutine(temporaryMessageRoutine);

        temporaryMessageRoutine = StartCoroutine(
            ShowTemporaryMessageRoutine(message, duration)
        );
    }

    private IEnumerator ShowTemporaryMessageRoutine(string message, float duration)
    {
        ShowPrompt(message);

        yield return new WaitForSeconds(duration);

        if (currentInteractable == null)
            HidePrompt();

        temporaryMessageRoutine = null;
    }

    private void DetectInteractable()
    {
        if (viewCamera == null)
        {
            ClearCurrentInteractable();
            return;
        }

        Ray ray = new Ray(
            viewTransform.position,
            viewTransform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactRange,
            interactableMask))
        {
            InteractableObjectEvent interactable =
                hit.transform.GetComponentInParent<InteractableObjectEvent>();

            if (interactable != null && interactable.CanInteract())
            {
                currentInteractable = interactable;
                ShowPrompt(interactable.GetPromptText());
                return;
            }
        }

        ClearCurrentInteractable();
    }

    private void ClearCurrentInteractable()
    {
        currentInteractable = null;

        if (temporaryMessageRoutine == null)
            HidePrompt();
    }

    private void ShowPrompt(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
            promptText.enabled = true;
        }

        OnPromptShown.Invoke(text);
    }

    private void HidePrompt()
    {
        if (promptText != null)
            promptText.enabled = false;

        OnPromptHidden.Invoke();
    }
}