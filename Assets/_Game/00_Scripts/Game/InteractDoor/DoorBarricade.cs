using UnityEngine;

public class DoorBarricade : MonoBehaviour
{
    [Tooltip("Nama item buat lepas kayu ini, misal 'Crowbar'.")]
    public string requiredItemName = "Crowbar";

    [Header("Teks Prompt")]
    public string removePrompt = "Press E to Pry Off";
    [Tooltip("Muncul saat ditekan E, kalau ternyata belum punya item. {0} = nama item.")]
    public string needItemMessage = "Need {0}";

    public InteractableDoor parentDoor;

    [Header("Setelah Lepas")]
    [Tooltip("Layer yang dipakai setelah kayu lepas, supaya tidak kena raycast interact lagi tpi tetap ada collider fisik normal dengan laintai. Pastikan layer ini gak di centang di Interactable Mask yang ada di PlayerInteractor.")]
    public string layerAfterRemoved = "Default";

    void Awake()
    {
        if (parentDoor == null) parentDoor = GetComponentInParent<InteractableDoor>();
        if (parentDoor != null) parentDoor.RegisterBarricade(this);
    }

    public string GetPrompt() => removePrompt;

    public bool TryInteract(out string failMessage)
    {
        failMessage = null;

        if (!HasRequiredItem())
        {
            failMessage = string.Format(needItemMessage, requiredItemName);
            return false;
        }

        RemovePlank();
        return true;
    }

    private bool HasRequiredItem()
    {
        return PlayerInventory.instance != null && PlayerInventory.instance.HasItem(requiredItemName);
    }

    private void RemovePlank()
    {
        if (parentDoor != null) parentDoor.UnregisterBarricade(this);

        int newLayer = LayerMask.NameToLayer(layerAfterRemoved);
        if (newLayer != -1) gameObject.layer = newLayer;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(Vector3.down * 2f + Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
        }
        else
        {
            gameObject.SetActive(false); 
        }
    }
}