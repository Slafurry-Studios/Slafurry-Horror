using UnityEngine;

public class TakeItemTrigger : BaseTrigger
{
    [Header("Information")]
    [SerializeField] private string itemName;
    [SerializeField] private string description;

    [Header("Item")]
    [SerializeField] private GameObject itemObject;
    
    private PlayerInventory playerInventory;

    void Awake()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
    }

    public void TakeItem()
    {
        if (!CanTrigger()) return;

        if (itemObject != null) itemObject.SetActive(false);
        playerInventory.Add(itemObject);

        AddTriggerCount();
    }
}