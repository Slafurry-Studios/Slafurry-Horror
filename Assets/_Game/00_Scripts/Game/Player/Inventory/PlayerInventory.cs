using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    public float notificationDuration = 2f;

    // Case-insensitive + trim biar typo kapital/spasi gak bikin item dianggap beda
    private readonly HashSet<string> collectedItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    void Awake()
    {
        instance = this;
    }

    public void CollectItem(string itemName, string displayLabel = null)
    {
        string key = Normalize(itemName);
        if (string.IsNullOrEmpty(key) || collectedItems.Contains(key)) return;
        collectedItems.Add(key);

        string label = string.IsNullOrEmpty(displayLabel) ? itemName : displayLabel;
        if (PlayerInteractor.instance != null)
        {
            PlayerInteractor.instance.ShowTemporaryMessage($"{label} collected", notificationDuration);
        }
    }

    public bool HasItem(string itemName)
    {
        return collectedItems.Contains(Normalize(itemName));
    }

    private string Normalize(string name)
    {
        return name == null ? string.Empty : name.Trim();
    }
}