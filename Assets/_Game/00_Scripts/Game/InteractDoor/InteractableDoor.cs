using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState
{
    CanOpen,
    RequiresItem,
    Locked
}

public class InteractableDoor : MonoBehaviour
{
    [Header("Kondisi Pintu")]
    public DoorState doorState = DoorState.CanOpen;

    [Tooltip("Harus sama dengan itemName di ItemPickup")]
    public string requiredItemName;

    [Header("Referensi Pivot")]
    public Transform doorPivot;

    [Header("Pengaturan Buka/Tutup")]
    public float openAngleY = 90f;
    public float openCloseDuration = 0.5f;

    [Header("Teks Prompt")]
    [Tooltip("Tampil terus walau pintu terkunci, biar gak bocor info. {0} = nama item yang dibutuhkan")]
    public string openPrompt = "Press E to Open";
    public string closePrompt = "Press E to Close";
    public string lockedMessage = "Locked";
    [Tooltip("{0} = nama item yang dibutuhkan.")]
    public string needItemMessage = "Need {0}";

    [Header("Barricade (kayu penghalang, opsional)")]
    [Tooltip("Muncul kalau masih ada DoorBarricade aktif yang menghalangi pintu ini.")]
    public string blockedMessage = "Something is blocking this door";

    public bool IsOpen { get; private set; }
    private bool unlockedByItem = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine moveRoutine;

    private readonly List<DoorBarricade> activeBarricades = new List<DoorBarricade>();

    void Start()
    {
        if (doorPivot == null) doorPivot = transform;
        closedRotation = doorPivot.localRotation;
        openRotation = Quaternion.Euler(0f, openAngleY, 0f) * closedRotation;
    }

    public void RegisterBarricade(DoorBarricade barricade)
    {
        if (!activeBarricades.Contains(barricade)) activeBarricades.Add(barricade);
    }

    public void UnregisterBarricade(DoorBarricade barricade)
    {
        activeBarricades.Remove(barricade);
    }

    public string GetPrompt()
    {
        return IsOpen ? closePrompt : openPrompt;
    }


    public bool TryInteract(out string failMessage)
    {
        failMessage = null;

        if (activeBarricades.Count > 0)
        {
            failMessage = blockedMessage;
            return false;
        }

        if (doorState == DoorState.Locked)
        {
            failMessage = lockedMessage;
            return false;
        }

        if (doorState == DoorState.RequiresItem && !unlockedByItem)
        {
            if (!HasRequiredItem())
            {
                failMessage = string.Format(needItemMessage, requiredItemName);
                return false;
            }
            unlockedByItem = true;
        }

        ToggleDoor();
        return true;
    }

    private bool HasRequiredItem()
    {
        return PlayerInventory.instance != null && PlayerInventory.instance.HasItem(requiredItemName);
    }

    private void ToggleDoor()
    {
        IsOpen = !IsOpen;
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(RotateDoor(IsOpen ? openRotation : closedRotation));
    }

    private IEnumerator RotateDoor(Quaternion target)
    {
        Quaternion start = doorPivot.localRotation;
        float elapsed = 0f;
        while (elapsed < openCloseDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / openCloseDuration);
            doorPivot.localRotation = Quaternion.Slerp(start, target, p);
            yield return null;
        }
        doorPivot.localRotation = target;
        moveRoutine = null;
    }
}