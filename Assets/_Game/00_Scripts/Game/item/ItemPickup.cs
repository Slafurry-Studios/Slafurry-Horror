using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Tooltip("Harus sama dengan Required Item Name di pintu")]
    public string itemName = "Pisau";

    [Tooltip("Kosongkan kalau mau otomatis pakai itemName")]
    public string displayLabel;

    public string pickupPrompt = "Press E to Pick Up {0}";

    public string GetPrompt()
    {
        string label = string.IsNullOrEmpty(displayLabel) ? itemName : displayLabel;
        return string.Format(pickupPrompt, label);
    }

    public void Collect()
    {
        if (PlayerInventory.instance != null)
        {
            PlayerInventory.instance.CollectItem(itemName, displayLabel);
        }
        gameObject.SetActive(false);
    }
}