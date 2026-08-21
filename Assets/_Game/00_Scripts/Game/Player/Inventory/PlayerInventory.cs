using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int maxItems = 3;
    [SerializeField] private List<GameObject> items = new();

    [Header("Full Inventory Prompt")]
    [SerializeField] private string fullInventoryMessage = "Inventory is full.";
    private PromptText promptText;

    public List<GameObject> Items => items;
    public int ItemCount => items.Count;
    public int MaxItems => maxItems;

    private void Awake()
    {
        promptText = FindAnyObjectByType<PromptText>();

    }

    void Start()
    {
        PlayerInventory data = PlayerManager.Instance.PlayerInventory;
        items = data?.Items;

        PlayerManager.Instance.SetPlayerInventory(this);
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

        Transform container = PlayerManager.Instance.InventoryContainer;

        if (container != null)
            item.transform.SetParent(container);

        item.SetActive(false);

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