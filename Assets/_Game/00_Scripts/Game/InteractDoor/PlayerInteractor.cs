using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    public static PlayerInteractor instance;

    [Header("Referensi")]
    public Transform viewTransform;
    [Tooltip("Prompt interact, misal 'Press E to Open'")]
    public TMP_Text actionText;
    [Tooltip("Notifikasi terpisah, misal 'Pisau collected'")]
    public TMP_Text notificationText;
    [Tooltip("Berhentikan deteksi selama examine")]
    public PlayerInspect playerInspect;

    [Header("Deteksi Objek")]
    public float interactRange = 3f;
    [Tooltip("Centang semua layer yang bisa di-interact: Door, Item")]
    public LayerMask interactableMask;
    public KeyCode interactKey = KeyCode.E;

    private InteractableDoor currentDoor;
    private ItemPickup currentItem;

    // Pesan gagal (Locked/Need Item) hilang saat player gak ngarah ke pintu ini lagi
    private InteractableDoor failMessageDoor;
    private string pendingFailMessage;

    private Coroutine notificationRoutine;

    void Awake()
    {
        instance = this;
        if (viewTransform == null) viewTransform = transform;
        if (actionText != null) actionText.enabled = false;
        if (notificationText != null) notificationText.enabled = false;
    }

    void Update()
    {
        if (playerInspect != null && playerInspect.IsInspecting) return;

        DetectInteractable();

        if (Input.GetKeyDown(interactKey))
        {
            if (currentDoor != null)
            {
                HandleDoorInteract(currentDoor);
            }
            else if (currentItem != null)
            {
                currentItem.Collect();
                currentItem = null;
                HideActionText();
            }
        }
    }

    private void HandleDoorInteract(InteractableDoor door)
    {
        bool success = door.TryInteract(out string failReason);

        if (success)
        {
            ClearFailMessage();
        }
        else
        {
            failMessageDoor = door;
            pendingFailMessage = failReason;
            ShowActionText(failReason);
        }
    }

    private void DetectInteractable()
    {
        Ray ray = new Ray(viewTransform.position, viewTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            InteractableDoor door = hit.collider.GetComponentInParent<InteractableDoor>();
            if (door != null)
            {
                currentDoor = door;
                currentItem = null;

                if (failMessageDoor == door && !string.IsNullOrEmpty(pendingFailMessage))
                    ShowActionText(pendingFailMessage);
                else
                    ShowActionText(door.GetPrompt());

                return;
            }

            ItemPickup item = hit.collider.GetComponentInParent<ItemPickup>();
            if (item != null)
            {
                currentItem = item;
                currentDoor = null;
                ClearFailMessage();
                ShowActionText(item.GetPrompt());
                return;
            }
        }

        currentDoor = null;
        currentItem = null;
        ClearFailMessage();
        HideActionText();
    }

    private void ClearFailMessage()
    {
        failMessageDoor = null;
        pendingFailMessage = null;
    }

    private void ShowActionText(string text)
    {
        if (actionText == null) return;
        actionText.text = text;
        actionText.enabled = true;
    }

    private void HideActionText()
    {
        if (actionText == null) return;
        actionText.enabled = false;
    }

    // Dipanggil PlayerInventory untuk notifikasi item collected
    public void ShowTemporaryMessage(string text, float duration = 1.5f)
    {
        if (notificationRoutine != null) StopCoroutine(notificationRoutine);
        notificationRoutine = StartCoroutine(NotificationRoutine(text, duration));
    }

    private IEnumerator NotificationRoutine(string text, float duration)
    {
        if (notificationText == null) yield break;

        notificationText.text = text;
        notificationText.enabled = true;
        yield return new WaitForSeconds(duration);
        notificationText.enabled = false;
        notificationRoutine = null;
    }
}