using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int maxItems = 3;
    [SerializeField] private List<GameObject> items = new();

    [Header("Full Inventory Prompt")]
    [SerializeField] private string fullInventoryMessage = "Inventory is full.";
    private PromptText promptText;
    public int ItemCount => items.Count;
    public int MaxItems => maxItems;

    void Awake()
    {
        promptText = FindAnyObjectByType<PromptText>();
    }

    public bool Add(GameObject item)
    {
        if (item == null || items.Contains(item))
            return false;

        if (items.Count >= maxItems)
        {
            ShowFullInventoryMessage();
            return false;
        }

        items.Add(item);
        return true;
    }

    public void Remove(GameObject item)
    {
        if (item == null)
            return;

        items.Remove(item);
    }

    public bool Has(GameObject item)
    {
        return item != null && items.Contains(item);
    }

    public GameObject GetItem(int index)
    {
        if (index < 0 || index >= items.Count)
            return null;

        return items[index];
    }

    public void Clear()
    {
        items.Clear();
    }

    private void ShowFullInventoryMessage()
    {
        promptText.Show(fullInventoryMessage);
    }
}