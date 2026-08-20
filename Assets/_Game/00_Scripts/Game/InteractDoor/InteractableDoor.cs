using System.Collections;
using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [Header("Pivot")]
    [SerializeField] private Transform doorPivot;

    [Header("Open / Close")]
    [SerializeField] private float openAngleY = 90f;
    [SerializeField] private float openCloseDuration = 0.5f;

    public bool IsOpen { get; private set; }

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine moveRoutine;

    private void Start()
    {
        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;

        openRotation =
            Quaternion.Euler(0f, openAngleY, 0f) *
            closedRotation;
    }

    // =========================================================
    // ACTIONS
    // =========================================================

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        MoveTo(openRotation);
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        MoveTo(closedRotation);
    }

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void MoveTo(Quaternion target)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(RotateDoor(target));
    }

    private IEnumerator RotateDoor(Quaternion target)
    {
        Quaternion start = doorPivot.localRotation;
        float elapsed = 0f;

        if (openCloseDuration <= 0f)
        {
            doorPivot.localRotation = target;
            moveRoutine = null;
            yield break;
        }

        while (elapsed < openCloseDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / openCloseDuration);

            doorPivot.localRotation =
                Quaternion.Slerp(
                    start,
                    target,
                    progress
                );

            yield return null;
        }

        doorPivot.localRotation = target;
        moveRoutine = null;
    }
}