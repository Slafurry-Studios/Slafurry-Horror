using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerInspect : MonoBehaviour
{
    [Header("Referensi")]
    public Transform viewTransform;
    public Transform examinePoint;
    public FirstPersonLook firstPersonLook;
    public FirstPersonMovement playerMovement;

    [Header("Deteksi Objek")]
    public float interactRange = 3f;
    public LayerMask interactableMask;
    public KeyCode interactKey = KeyCode.E;

    [Header("Rotasi Saat Inspect")]
    public float rotationSpeed = 100f;

    [Header("Zoom")]
    public float zoomSpeed = 0.5f;
    [Header("UI")]
    public TMP_Text promptText;          // Press E to Inspect
    public TMP_Text inspectPromptText;
    public string inspectExitText = "Press E to Back";   // Press E to Back

    [Header("Transisi")]
    public bool smoothTransition = true;
    public float moveLerpSpeed = 8f;
    public float returnDuration = 0.4f;

    public bool IsInspecting { get; private set; }

    private InspectableItem currentTarget;
    private InspectableItem inspectedItem;
    private Transform pivot;
    private float currentExamineDistance;

    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private Vector3 originalScale;

    private Rigidbody itemRigidbody;
    private bool itemHadGravity;
    private Collider[] itemColliders;
    private System.Func<float> freezeMovementFunc;
    private Vector3 previousMousePosition;
    private Coroutine returnRoutine;

    void Awake()
    {
        if (playerMovement == null) playerMovement = FirstPersonMovement.instance;
        if (viewTransform == null) viewTransform = transform;

        if (promptText != null)
            promptText.enabled = false;

        if (inspectPromptText != null)
            inspectPromptText.enabled = false;
    }

    void Update()
    {
        if (IsInspecting)
        {
            if (smoothTransition) SmoothMoveToExaminePoint();
            HandleZoom();
            HandleInspectRotation();

            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Escape))
            {
                EndInspect();
            }
            return;
        }

        DetectInteractable();
        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            StartInspect(currentTarget);
        }
    }

    private void DetectInteractable()
    {
        Ray ray = new Ray(viewTransform.position, viewTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            InspectableItem item = hit.collider.GetComponent<InspectableItem>();
            if (item != null)
            {
                currentTarget = item;
                ShowPrompt(item.promptLabel);
                return;
            }
        }
        currentTarget = null;
        HidePrompt();
    }

    private void StartInspect(InspectableItem item)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        IsInspecting = true;
        inspectedItem = item;
        currentExamineDistance = item.examineDistance;

        Transform t = item.transform;
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

        itemColliders = t.GetComponentsInChildren<Collider>();
        foreach (Collider col in itemColliders) col.enabled = false;

        CreatePivotAtBoundsCenter(t);
        t.SetParent(pivot, worldPositionStays: true);
        t.localScale = originalScale * item.examineScaleMultiplier;

        if (!smoothTransition) pivot.localPosition = Vector3.forward * currentExamineDistance;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (firstPersonLook != null) firstPersonLook.LockRotasi();
        if (playerMovement != null)
        {
            freezeMovementFunc = () => 0f;
            playerMovement.speedOverrides.Add(freezeMovementFunc);
        }

        HidePrompt();
        ShowInspectPrompt(inspectExitText);
    }

    private void CreatePivotAtBoundsCenter(Transform item)
    {
        Bounds bounds = CalculateBounds(item);

        GameObject pivotGO = new GameObject("InspectPivot");
        pivot = pivotGO.transform;
        pivot.position = bounds.center;
        pivot.rotation = Quaternion.identity;
        pivot.SetParent(examinePoint, worldPositionStays: true);
    }

    private Bounds CalculateBounds(Transform item)
    {
        Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(item.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private void SmoothMoveToExaminePoint()
    {
        Vector3 targetLocalPos = Vector3.forward * currentExamineDistance;
        pivot.localPosition = Vector3.Lerp(pivot.localPosition, targetLocalPos, Time.deltaTime * moveLerpSpeed);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.0001f) return;

        currentExamineDistance = Mathf.Clamp(
            currentExamineDistance - scroll * zoomSpeed,
            inspectedItem.minExamineDistance,
            inspectedItem.maxExamineDistance
        );
    }

    private void HandleInspectRotation()
    {
        if (Input.GetMouseButtonDown(0)) previousMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            float rotX = delta.y * rotationSpeed * Time.deltaTime;
            float rotY = -delta.x * rotationSpeed * Time.deltaTime;

            pivot.rotation = Quaternion.Euler(rotX, rotY, 0) * pivot.rotation;
            previousMousePosition = Input.mousePosition;
        }
    }

    private void EndInspect()
    {
        Transform t = inspectedItem.transform;
        t.SetParent(originalParent, worldPositionStays: true);
        Destroy(pivot.gameObject);
        pivot = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (firstPersonLook != null) firstPersonLook.UnlockRotasi();
        if (playerMovement != null && freezeMovementFunc != null)
        {
            playerMovement.speedOverrides.Remove(freezeMovementFunc);
            freezeMovementFunc = null;
        }

        if (smoothTransition)
            returnRoutine = StartCoroutine(ReturnToOriginalTransform(t, itemRigidbody, itemHadGravity, itemColliders));
        else
            FinishReturn(t, itemRigidbody, itemHadGravity, itemColliders);

        HideInspectPrompt();

        inspectedItem = null;
        IsInspecting = false;
    }

    private IEnumerator ReturnToOriginalTransform(Transform t, Rigidbody rb, bool hadGravity, Collider[] colliders)
    {
        Vector3 startPos = t.localPosition;
        Quaternion startRot = t.localRotation;
        Vector3 startScale = t.localScale;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / returnDuration);

            t.localPosition = Vector3.Lerp(startPos, originalLocalPos, p);
            t.localRotation = Quaternion.Slerp(startRot, originalLocalRot, p);
            t.localScale = Vector3.Lerp(startScale, originalScale, p);

            yield return null;
        }

        FinishReturn(t, rb, hadGravity, colliders);
        returnRoutine = null;
    }

    private void FinishReturn(Transform t, Rigidbody rb, bool hadGravity, Collider[] colliders)
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
            foreach (Collider col in colliders) col.enabled = true;
        }
    }

    private void ShowPrompt(string text)
    {
        if (promptText == null) return;
        promptText.text = text;
        promptText.enabled = true;
    }

    private void ShowInspectPrompt(string text)
    {
        if (inspectPromptText == null) return;

        inspectPromptText.text = text;
        inspectPromptText.enabled = true;
    }

    private void HideInspectPrompt()
    {
        if (inspectPromptText == null) return;

        inspectPromptText.enabled = false;
    }

    private void HidePrompt()
    {
        if (promptText == null) return;
        promptText.enabled = false;
    }
}