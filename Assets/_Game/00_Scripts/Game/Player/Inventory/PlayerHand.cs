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

    public int CurrentSlot => currentSlot;
    public GameObject CurrentItem => currentItem;
    void Start()
    {
        PlayerHand data = PlayerManager.Instance.PlayerHand;
        int targetSlot = data != null ? data.CurrentSlot : -1;

        currentSlot = -1;
        currentItem = null;

        PlayerManager.Instance.SetPlayerHand(this);

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

    public void DetachCarriedItemForSceneTransition()
    {
        if (currentItem == null)
            return;

        PlayerManager manager = PlayerManager.Instance;

        if (manager == null || manager.InventoryContainer == null)
            return;

        currentItem.transform.SetParent(manager.InventoryContainer);
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

    private void Unequip()
    {
        if (currentItem != null)
            currentItem.SetActive(false);

        currentItem = null;
        currentSlot = -1;
    }
    private void Equip(GameObject item, int slot)
    {
        if (currentItem != null)
            currentItem.SetActive(false);

        currentSlot = slot;
        currentItem = item;

        Rigidbody rb = currentItem.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        currentItem.transform.SetParent(handTransform);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        currentItem.SetActive(true);
    }

    public void DropCurrentItem()
    {
        if (currentItem == null || inventory == null)
            return;

        GameObject item = currentItem;

        inventory.Remove(item);

        currentItem = null;
        currentSlot = -1;

        item.transform.SetParent(null);

        if (dropTransform != null)
        {
            item.transform.position = dropTransform.position;
            item.transform.rotation = dropTransform.rotation;
        }

        item.SetActive(true);

        Rigidbody rb = item.GetComponent<Rigidbody>();

        if (rb == null)
            rb = item.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.detectCollisions = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

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
        return currentItem;
    }

    public int GetCurrentSlot()
    {
        return currentSlot;
    }
}