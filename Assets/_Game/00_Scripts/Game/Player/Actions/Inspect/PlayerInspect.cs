using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Slafurry.System.Pause;

public class PlayerInspect : MonoBehaviour
{
    [Header("References")]
    public Transform examinePoint;
    public Camera mainCamera;
    public Light examineLight;

    [Header("Inspect Settings")]
    public Vector2 screenOffset = new Vector2(-0.3f, 0f);
    public float rotationSpeed = 100f;
    public float examineDistance = 0.5f;
    public float examineScaleMultiplier = 1f;
    public bool smoothTransition = true;
    public float moveLerpSpeed = 8f;
    public float returnDuration = 0.4f;

    [Header("Events")]
    public UnityEvent OnInspectStarted = new UnityEvent();
    public UnityEvent OnInspectEnded = new UnityEvent();

    [System.Serializable]
    public class InspectInfoEvent : UnityEvent<string, string> { }

    public InspectInfoEvent OnInspectInfoChanged =
        new InspectInfoEvent();

    public bool IsInspecting { get; private set; }

    private const string InspectPauseKey = "Inspect";

    private Transform inspectedItem;
    private Transform pivot;

    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private Vector3 originalScale;

    private Rigidbody itemRigidbody;
    private bool itemHadGravity;
    private Collider[] itemColliders;

    private Vector3 previousMousePosition;
    private Coroutine returnRoutine;

    private int isolationLayer = -1;

    private readonly Dictionary<Transform, int> originalLayers =
        new Dictionary<Transform, int>();

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        isolationLayer = LayerMask.NameToLayer("ExamineIsolated");

        if (isolationLayer == -1)
        {
            Debug.LogWarning(
                "[PlayerInspect] Layer 'ExamineIsolated' belum dibuat."
            );
        }

        if (examineLight != null)
            examineLight.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsInspecting)
            return;

        if (smoothTransition)
            SmoothMoveToExaminePoint();

        HandleInspectRotation();

        if (Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            EndInspect();
        }
    }

    public void StartInspect(Transform item)
    {
        if (item == null || IsInspecting)
            return;

        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        IsInspecting = true;
        inspectedItem = item;

        Pause.On(InspectPauseKey);

        CacheAndSetIsolationLayer(item);

        if (examineLight != null)
        {
            examineLight.gameObject.SetActive(true);

            examineLight.cullingMask =
                isolationLayer != -1
                    ? 1 << isolationLayer
                    : ~0;
        }

        ExamineInfo info =
            item.GetComponentInParent<ExamineInfo>();

        string title =
            info != null && !string.IsNullOrEmpty(info.title)
                ? info.title
                : FormatObjectName(item.name);

        string description =
            info != null
                ? info.description
                : string.Empty;

        OnInspectInfoChanged.Invoke(title, description);

        Transform t = item;

        originalParent = t.parent;
        originalLocalPos = t.localPosition;
        originalLocalRot = t.localRotation;
        originalScale = t.localScale;

        itemRigidbody = t.GetComponent<Rigidbody>();

        if (itemRigidbody != null)
        {
            itemHadGravity = itemRigidbody.useGravity;

            itemRigidbody.useGravity = false;
            itemRigidbody.velocity = Vector3.zero;
            itemRigidbody.isKinematic = true;
        }

        itemColliders =
            t.GetComponentsInChildren<Collider>();

        foreach (Collider col in itemColliders)
            col.enabled = false;

        CreatePivotAtBoundsCenter(t);

        t.SetParent(pivot, true);
        t.localScale =
            originalScale * examineScaleMultiplier;

        if (!smoothTransition)
        {
            pivot.localPosition = new Vector3(
                screenOffset.x,
                screenOffset.y,
                examineDistance
            );
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnInspectStarted.Invoke();
    }

    public void EndInspect()
    {
        if (!IsInspecting || inspectedItem == null)
            return;

        Pause.Off(InspectPauseKey);

        RestoreIsolationLayer();

        Transform t = inspectedItem;

        t.SetParent(originalParent, true);

        if (pivot != null)
            Destroy(pivot.gameObject);

        pivot = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (examineLight != null)
            examineLight.gameObject.SetActive(false);

        if (smoothTransition)
        {
            returnRoutine = StartCoroutine(
                ReturnToOriginalTransform(
                    t,
                    itemRigidbody,
                    itemHadGravity,
                    itemColliders
                )
            );
        }
        else
        {
            FinishReturn(
                t,
                itemRigidbody,
                itemHadGravity,
                itemColliders
            );
        }

        inspectedItem = null;
        IsInspecting = false;

        OnInspectEnded.Invoke();
    }

    private string FormatObjectName(string rawName)
    {
        return rawName
            .Replace("(Clone)", "")
            .Replace("_", " ")
            .Trim();
    }

    private void CacheAndSetIsolationLayer(Transform item)
    {
        originalLayers.Clear();

        if (isolationLayer == -1)
            return;

        Transform[] transforms =
            item.GetComponentsInChildren<Transform>(true);

        foreach (Transform tr in transforms)
        {
            originalLayers[tr] = tr.gameObject.layer;
            tr.gameObject.layer = isolationLayer;
        }
    }

    private void RestoreIsolationLayer()
    {
        foreach (var pair in originalLayers)
        {
            if (pair.Key != null)
                pair.Key.gameObject.layer = pair.Value;
        }

        originalLayers.Clear();
    }

    private void CreatePivotAtBoundsCenter(Transform item)
    {
        Bounds bounds = CalculateBounds(item);

        GameObject pivotObject =
            new GameObject("InspectPivot");

        pivot = pivotObject.transform;

        pivot.position = bounds.center;
        pivot.rotation = Quaternion.identity;

        pivot.SetParent(
            examinePoint,
            true
        );
    }

    private Bounds CalculateBounds(Transform item)
    {
        Renderer[] renderers =
            item.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(
                item.position,
                Vector3.zero
            );

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    private void SmoothMoveToExaminePoint()
    {
        if (pivot == null)
            return;

        Vector3 targetLocalPos =
            new Vector3(
                screenOffset.x,
                screenOffset.y,
                examineDistance
            );

        pivot.localPosition =
            Vector3.Lerp(
                pivot.localPosition,
                targetLocalPos,
                Time.unscaledDeltaTime * moveLerpSpeed
            );
    }

    private void HandleInspectRotation()
    {
        if (pivot == null)
            return;

        if (Input.GetMouseButtonDown(0))
            previousMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta =
                Input.mousePosition - previousMousePosition;

            float rotX =
                delta.y *
                rotationSpeed *
                Time.unscaledDeltaTime;

            float rotY =
                -delta.x *
                rotationSpeed *
                Time.unscaledDeltaTime;

            pivot.rotation =
                Quaternion.Euler(rotX, rotY, 0f) *
                pivot.rotation;

            previousMousePosition =
                Input.mousePosition;
        }
    }

    private IEnumerator ReturnToOriginalTransform(
        Transform t,
        Rigidbody rb,
        bool hadGravity,
        Collider[] colliders)
    {
        Vector3 startPos = t.localPosition;
        Quaternion startRot = t.localRotation;
        Vector3 startScale = t.localScale;

        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / returnDuration
                );

            t.localPosition =
                Vector3.Lerp(
                    startPos,
                    originalLocalPos,
                    progress
                );

            t.localRotation =
                Quaternion.Slerp(
                    startRot,
                    originalLocalRot,
                    progress
                );

            t.localScale =
                Vector3.Lerp(
                    startScale,
                    originalScale,
                    progress
                );

            yield return null;
        }

        FinishReturn(
            t,
            rb,
            hadGravity,
            colliders
        );

        returnRoutine = null;
    }

    private void FinishReturn(
        Transform t,
        Rigidbody rb,
        bool hadGravity,
        Collider[] colliders)
    {
        t.localPosition = originalLocalPos;
        t.localRotation = originalLocalRot;
        t.localScale = originalScale;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = hadGravity;
        }

        if (colliders != null)
        {
            foreach (Collider col in colliders)
                col.enabled = true;
        }
    }
}