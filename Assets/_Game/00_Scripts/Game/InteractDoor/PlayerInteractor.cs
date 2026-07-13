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
    [Tooltip("Centang semua layer yang bisa diinteract: Door, Item (kayu pake layer Door yang sama)")]
    public LayerMask interactableMask;
    public KeyCode interactKey = KeyCode.E;
    private InteractableDoor currentDoor;
    private ItemPickup currentItem;
    private DoorBarricade currentBarricade;
    private InteractableDoor failMessageDoor;
    private string pendingDoorFailMessage;
    private DoorBarricade failMessageBarricade;
    private string pendingBarricadeFailMessage;
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
            if (currentBarricade != null)
            {
                HandleBarricadeInteract(currentBarricade);
            }
            else if (currentDoor != null)
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
            ClearDoorFailMessage();
        }
        else
        {
            failMessageDoor = door;
            pendingDoorFailMessage = failReason;
            ShowActionText(failReason);
        }
    }
    private void HandleBarricadeInteract(DoorBarricade barricade)
    {
        bool success = barricade.TryInteract(out string failReason);
        if (success)
        {
            ClearBarricadeFailMessage();
            currentBarricade = null;
            HideActionText();
        }
        else
        {
            failMessageBarricade = barricade;
            pendingBarricadeFailMessage = failReason;
            ShowActionText(failReason);
        }
    }
    private void DetectInteractable()
    {
        Ray ray = new Ray(viewTransform.position, viewTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            DoorBarricade barricade = hit.collider.GetComponentInParent<DoorBarricade>();
            if (barricade != null)
            {
                currentBarricade = barricade;
                currentDoor = null;
                currentItem = null;
                ClearDoorFailMessage();
                if (failMessageBarricade == barricade && !string.IsNullOrEmpty(pendingBarricadeFailMessage))
                    ShowActionText(pendingBarricadeFailMessage);
                else
                    ShowActionText(barricade.GetPrompt());
                return;
            }

            InteractableDoor door = hit.collider.GetComponentInParent<InteractableDoor>();
            if (door != null)
            {
                currentDoor = door;
                currentItem = null;
                currentBarricade = null;
                ClearBarricadeFailMessage();
                if (failMessageDoor == door && !string.IsNullOrEmpty(pendingDoorFailMessage))
                    ShowActionText(pendingDoorFailMessage);
                else
                    ShowActionText(door.GetPrompt());
                return;
            }
            ItemPickup item = hit.collider.GetComponentInParent<ItemPickup>();
            if (item != null)
            {
                currentItem = item;
                currentDoor = null;
                currentBarricade = null;
                ClearDoorFailMessage();
                ClearBarricadeFailMessage();
                ShowActionText(item.GetPrompt());
                return;
            }
        }
        currentDoor = null;
        currentItem = null;
        currentBarricade = null;
        ClearDoorFailMessage();
        ClearBarricadeFailMessage();
        HideActionText();
    }
    private void ClearDoorFailMessage()
    {
        failMessageDoor = null;
        pendingDoorFailMessage = null;
    }
    private void ClearBarricadeFailMessage()
    {
        failMessageBarricade = null;
        pendingBarricadeFailMessage = null;
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