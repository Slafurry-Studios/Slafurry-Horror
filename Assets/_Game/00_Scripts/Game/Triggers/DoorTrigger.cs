using System.Collections;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform doorPivot;

    [Header("Door")]
    [SerializeField] private float openAngleY = 90f;
    [SerializeField] private float openCloseDuration = 0.5f;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine moveRoutine;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;
        openRotation = Quaternion.Euler(0f, openAngleY, 0f) * closedRotation;
    }

    public void Open()
    {
        if (isOpen)
            return;

        SetDoorState(true);
    }

    public void Close()
    {
        if (!isOpen)
            return;

        SetDoorState(false);
    }

    public void Toggle()
    {
        SetDoorState(!isOpen);
    }

    private void SetDoorState(bool open)
    {
        isOpen = open;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        Quaternion target = open ? openRotation : closedRotation;
        moveRoutine = StartCoroutine(RotateDoor(target));
    }

    private IEnumerator RotateDoor(Quaternion target)
    {
        Quaternion start = doorPivot.localRotation;
        float elapsed = 0f;

        while (elapsed < openCloseDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / openCloseDuration
            );

            doorPivot.localRotation =
                Quaternion.Slerp(start, target, progress);

            yield return null;
        }

        doorPivot.localRotation = target;
        moveRoutine = null;
    }
}