using TMPro;
using UnityEngine;

public class PlayerInteractEvent : MonoBehaviour
{
    [Header("Referensi")]
    public Transform viewTransform;
    public Camera viewCamera;
    public PlayerInspect playerInspect;

    [Header("Keamanan / Backward Compatibility")]
    public bool disableDuringInspect = true;
    public bool preventOverlapWithOtherPrompts = true;

    [Header("Deteksi Objek")]
    public float interactRange = 3f;
    public LayerMask interactableMask;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Prompt")]
    public TMP_Text promptText;

    private InteractableObjectEvent currentInteractable;

    private void Awake()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera != null && viewTransform == null) viewTransform = viewCamera.transform;
        if (viewTransform == null) viewTransform = transform;

        if (promptText != null) promptText.enabled = false;
    }

    private void Update()
    {
        if (disableDuringInspect && playerInspect != null && playerInspect.IsInspecting)
        {
            currentInteractable = null;
            HidePrompt();
            return;
        }

        if (preventOverlapWithOtherPrompts && promptText != null && !promptText.enabled)
        {
            // Tidak melakukan apa-apa; guard ini menjaga agar prompt tidak muncul saat UI lain sedang aktif.
        }

        DetectInteractable();

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact();
            HidePrompt();
        }
    }

    private void DetectInteractable()
    {
        if (viewCamera == null)
        {
            currentInteractable = null;
            HidePrompt();
            return;
        }

        Ray ray = new Ray(viewTransform.position, viewTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            var interactable = hit.transform.GetComponentInParent<InteractableObjectEvent>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                ShowPrompt(interactable.GetPromptText());
                return;
            }
        }

        currentInteractable = null;
        HidePrompt();
    }

    private void ShowPrompt(string text)
    {
        if (promptText == null) return;
        promptText.text = text;
        promptText.enabled = true;
    }

    private void HidePrompt()
    {
        if (promptText == null) return;
        promptText.enabled = false;
    }
}
