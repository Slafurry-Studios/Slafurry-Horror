using UnityEngine;

public class TakeItemTrigger : BaseTrigger
{
    [Header("Information")]
    [SerializeField] public Sprite icon;
    [SerializeField] public string itemName;
    [SerializeField] public string description;

    [Header("Inventory")]
    private PlayerInventory inventory;

    void Awake()
    {
        inventory = FindAnyObjectByType<PlayerInventory>();    
    }

    public void TakeItem()
    {
        if (!CanTrigger())
            return;

        if (!inventory.Add(gameObject))
            return;

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        gameObject.SetActive(false);

        AddTriggerCount();
    }
}