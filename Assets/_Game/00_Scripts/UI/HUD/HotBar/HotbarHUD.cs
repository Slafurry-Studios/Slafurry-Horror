using UnityEngine;
using UnityEngine.UI;

public class HotbarHUD : MonoBehaviour
{
    [System.Serializable]
    public class HotbarSlot
    {
        public Image icon;
        public GameObject selectedIndicator;
    }

    [Header("References")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerHand playerHand;

    [Header("Slots")]
    [SerializeField] private HotbarSlot[] slots = new HotbarSlot[3];

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (inventory == null || playerHand == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            RefreshSlot(i);
        }

        RefreshSelection();
    }

    private void RefreshSlot(int index)
    {
        if (index >= inventory.ItemCount)
        {
            ClearSlot(index);
            return;
        }

        GameObject item = inventory.GetItem(index);

        if (item == null)
        {
            ClearSlot(index);
            return;
        }

        TakeItemTrigger itemData = item.GetComponent<TakeItemTrigger>();

        if (itemData == null || itemData.icon == null)
        {
            ClearSlot(index);
            return;
        }

        slots[index].icon.sprite = itemData.icon;
        slots[index].icon.enabled = true;
    }

    private void RefreshSelection()
    {
        int currentSlot = playerHand.GetCurrentSlot();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].selectedIndicator != null)
            {
                slots[i].selectedIndicator.SetActive(
                    i == currentSlot
                );
            }
        }
    }

    private void ClearSlot(int index)
    {
        if (slots[index].icon != null)
        {
            slots[index].icon.sprite = null;
            slots[index].icon.enabled = false;
        }

        if (slots[index].selectedIndicator != null)
        {
            slots[index].selectedIndicator.SetActive(false);
        }
    }
}