using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private Transform handTransform;
    [SerializeField] private Transform dropTransform;

    [Header("Drop")]
    [SerializeField] private float dropForce = 2f;

    public int currentSlot = -1;

    private GameObject currentItem;
    private GameObject currentOriginalItem;

    public int CurrentSlot => currentSlot;
    public GameObject CurrentItem => currentItem;

    private void Start()
    {
        PlayerManager manager = PlayerManager.Instance;

        inventory = manager.PlayerInventory;

        int targetSlot = manager.CurrentSlot;

        manager.SetPlayerHand(this);

        if (targetSlot >= 0)
            SelectSlot(targetSlot);
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectSlot(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectSlot(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectSlot(2);

        if (Input.GetKeyDown(KeyCode.Q))
            DropCurrentItem();
    }

    public void SelectSlot(int slot)
    {
        if (inventory == null)
            return;

        if (slot == currentSlot && currentItem != null)
        {
            Unequip();
            return;
        }

        GameObject item = inventory.GetItem(slot);

        if (item == null)
            return;

        Equip(item, slot);
    }

    private void Equip(GameObject originalItem, int slot)
    {
        Unequip();

        currentSlot = slot;
        currentOriginalItem = originalItem;

        // Original tetap berada di inventory.
        // Yang masuk ke tangan hanyalah clone.
        currentItem = Instantiate(
            originalItem,
            handTransform
        );

        currentItem.name = originalItem.name + " (Hand)";

        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        currentItem.transform.localScale = originalItem.transform.localScale;

        Rigidbody rb = currentItem.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        currentItem.SetActive(true);

        PlayerManager.Instance.SetCurrentSlot(slot);
    }

    private void Unequip()
    {
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }

        currentOriginalItem = null;
        currentSlot = -1;

        PlayerManager.Instance.SetCurrentSlot(-1);
    }

    public void DropCurrentItem()
    {
        if (currentOriginalItem == null || inventory == null)
            return;

        GameObject item = currentOriginalItem;

        // Remove dari inventory terlebih dahulu.
        inventory.Remove(item);

        // Hapus clone yang ada di tangan.
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }

        currentOriginalItem = null;
        currentSlot = -1;

        PlayerManager.Instance.SetCurrentSlot(-1);

        // Keluarkan original dari InventoryContainer.
        item.transform.SetParent(null);

        // Tentukan posisi drop.
        if (dropTransform != null)
        {
            item.transform.SetPositionAndRotation(
                dropTransform.position,
                dropTransform.rotation
            );
        }

        // Aktifkan object asli.
        item.SetActive(true);

        // Aktifkan physics.
        Rigidbody rb = item.GetComponent<Rigidbody>();

        if (rb == null)
            rb = item.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.detectCollisions = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Lempar sedikit ke depan.
        if (dropTransform != null)
        {
            rb.AddForce(
                dropTransform.forward * dropForce,
                ForceMode.Impulse
            );
        }
    }

    public GameObject GetCurrentItem()
    {
        return currentOriginalItem;
    }

    public int GetCurrentSlot()
    {
        return currentSlot;
    }
}