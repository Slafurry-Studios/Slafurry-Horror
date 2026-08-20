using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private Transform handTransform;

    private int currentSlot = -1;
    private GameObject currentItem;

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
    }

    public void SelectSlot(int slot)
    {
        if (inventory == null)
            return;

        GameObject item = inventory.GetItem(slot);

        if (item == null)
            return;

        Equip(item, slot);
    }

    private void Equip(GameObject item, int slot)
    {
        if (currentItem != null)
            currentItem.SetActive(false);

        currentSlot = slot;
        currentItem = item;

        currentItem.transform.SetParent(handTransform);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        currentItem.SetActive(true);
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