using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Slafurry.System.Pause;

public class PlayerInspect : MonoBehaviour
{
    [Header("Referensi")]
    public Transform viewTransform;
    public Transform examinePoint;
    public FirstPersonLook firstPersonLook;
    public FirstPersonMovement playerMovement;

    [Header("Kamera (Camera Stack untuk isolasi background)")]
    [Tooltip("Main Camera (Base) yang punya CinemachineBrain.")]
    public Camera mainCamera;
    [Tooltip("Overlay camera yang cuma render Quad penggelap layar.")]
    public Camera darkenOverlayCamera;
    [Tooltip("Overlay camera yang cuma render objek sedang diperiksa (layer ExamineIsolated).")]
    public Camera itemOverlayCamera;
    [Tooltip("WAJIB layer BARU yang KOSONG, tidak dipakai objek apapun secara permanen - dipakai sementara saat examine saja.")]
    public string isolationLayerName = "ExamineIsolated";
    [Tooltip("Opsional: lampu dekat examinePoint yang otomatis nyala selama examine.")]
    public Light examineLight;

    [Header("Posisi Objek di Layar")]
    [Tooltip("Geser objek dari tengah layar. X negatif = ke kiri, X positif = ke kanan, Y positif = ke atas.")]
    public Vector2 screenOffset = new Vector2(-0.3f, 0f);

    [Header("Deteksi Objek")]
    public float interactRange = 3f;
    public LayerMask interactableMask;
    public KeyCode interactKey = KeyCode.E;

    [Header("Rotasi Saat Inspect")]
    public float rotationSpeed = 100f;

    [Header("UI Prompt")]
    public TMP_Text promptText;
    public TMP_Text inspectPromptText;
    public string inspectExitText = "Press E to Back";

    [Header("UI Detail Examine (Judul & Deskripsi)")]
    public TMP_Text examineTitleText;
    public TMP_Text examineDescriptionText;

    [Header("Transisi")]
    public bool smoothTransition = true;
    public float moveLerpSpeed = 8f;
    public float returnDuration = 0.4f;

    public bool IsInspecting { get; private set; }

    private const string InspectPauseKey = "Inspect";

    private Transform currentTarget;
    private Transform inspectedItem;
    private Transform pivot;
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

    private int originalCullingMask;
    private int isolationLayer = -1;
    private readonly Dictionary<Transform, int> originalLayers = new Dictionary<Transform, int>();

    void Awake()
    {
        if (playerMovement == null) playerMovement = FirstPersonMovement.instance;
        if (viewTransform == null) viewTransform = transform;
        if (mainCamera == null) mainCamera = Camera.main;

        isolationLayer = LayerMask.NameToLayer(isolationLayerName);
        if (isolationLayer == -1)
        {
            Debug.LogWarning($"[PlayerInspect] Layer '{isolationLayerName}' belum dibuat.");
        }

        if (promptText != null) promptText.enabled = false;
        if (inspectPromptText != null) inspectPromptText.enabled = false;
        if (examineTitleText != null) examineTitleText.enabled = false;
        if (examineDescriptionText != null) examineDescriptionText.enabled = false;
        if (examineLight != null) examineLight.gameObject.SetActive(false);

        if (darkenOverlayCamera != null) darkenOverlayCamera.enabled = false;
        if (itemOverlayCamera != null) itemOverlayCamera.enabled = false;
    }

    void Update()
    {
        if (IsInspecting)
        {
            if (itemOverlayCamera != null) itemOverlayCamera.fieldOfView = mainCamera.fieldOfView;
            if (darkenOverlayCamera != null) darkenOverlayCamera.fieldOfView = mainCamera.fieldOfView;

            if (smoothTransition) SmoothMoveToExaminePoint();
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
            currentTarget = hit.transform;
            string format = InspectManager.instance != null ? InspectManager.instance.promptFormat : "Tekan E untuk periksa {0}";
            ShowPrompt(string.Format(format, FormatObjectName(currentTarget.name)));
            return;
        }
        currentTarget = null;
        HidePrompt();
    }

    private string FormatObjectName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Replace("_", " ").Trim();
    }

    private void StartInspect(Transform item)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        IsInspecting = true;
        inspectedItem = item;

        Pause.On(InspectPauseKey);

        CacheAndSetIsolationLayer(item);

        if (mainCamera != null)
        {
            originalCullingMask = mainCamera.cullingMask;
            mainCamera.cullingMask = originalCullingMask & ~(isolationLayer != -1 ? (1 << isolationLayer) : 0);
        }

        if (darkenOverlayCamera != null) darkenOverlayCamera.enabled = true;
        if (itemOverlayCamera != null)
        {
            itemOverlayCamera.enabled = true;
            itemOverlayCamera.fieldOfView = mainCamera.fieldOfView;
        }

        if (examineLight != null)
        {
            examineLight.gameObject.SetActive(true);
            examineLight.cullingMask = isolationLayer != -1 ? (1 << isolationLayer) : ~0;
        }

        ExamineInfo info = item.GetComponentInParent<ExamineInfo>();
        string title = (info != null && !string.IsNullOrEmpty(info.title)) ? info.title : FormatObjectName(item.name);
        string description = info != null ? info.description : "";

        if (examineTitleText != null)
        {
            examineTitleText.text = title;
            examineTitleText.enabled = true;
        }
        if (examineDescriptionText != null)
        {
            examineDescriptionText.text = description;
            examineDescriptionText.enabled = !string.IsNullOrEmpty(description);
        }

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

        itemColliders = t.GetComponentsInChildren<Collider>();
        foreach (Collider col in itemColliders) col.enabled = false;

        float scaleMultiplier = InspectManager.instance != null ? InspectManager.instance.examineScaleMultiplier : 1f;
        float distance = InspectManager.instance != null ? InspectManager.instance.examineDistance : 0.5f;

        CreatePivotAtBoundsCenter(t);
        t.SetParent(pivot, worldPositionStays: true);
        t.localScale = originalScale * scaleMultiplier;
        if (!smoothTransition) pivot.localPosition = new Vector3(screenOffset.x, screenOffset.y, distance);

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

    private void CacheAndSetIsolationLayer(Transform item)
    {
        originalLayers.Clear();
        if (isolationLayer == -1) return;

        Transform[] allTransforms = item.GetComponentsInChildren<Transform>(true);
        foreach (Transform tr in allTransforms)
        {
            originalLayers[tr] = tr.gameObject.layer;
            tr.gameObject.layer = isolationLayer;
        }
    }

    private void RestoreIsolationLayer()
    {
        foreach (var kvp in originalLayers)
        {
            if (kvp.Key != null) kvp.Key.gameObject.layer = kvp.Value;
        }
        originalLayers.Clear();
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
        float distance = InspectManager.instance != null ? InspectManager.instance.examineDistance : 0.5f;
        Vector3 targetLocalPos = new Vector3(screenOffset.x, screenOffset.y, distance);
        pivot.localPosition = Vector3.Lerp(pivot.localPosition, targetLocalPos, Time.unscaledDeltaTime * moveLerpSpeed);
    }

    private void HandleInspectRotation()
    {
        if (Input.GetMouseButtonDown(0)) previousMousePosition = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            float rotX = delta.y * rotationSpeed * Time.unscaledDeltaTime;
            float rotY = -delta.x * rotationSpeed * Time.unscaledDeltaTime;
            pivot.rotation = Quaternion.Euler(rotX, rotY, 0) * pivot.rotation;
            previousMousePosition = Input.mousePosition;
        }
    }

    private void EndInspect()
    {
        Pause.Off(InspectPauseKey);

        if (mainCamera != null) mainCamera.cullingMask = originalCullingMask;
        if (darkenOverlayCamera != null) darkenOverlayCamera.enabled = false;
        if (itemOverlayCamera != null) itemOverlayCamera.enabled = false;
        if (examineLight != null) examineLight.gameObject.SetActive(false);

        RestoreIsolationLayer();

        if (examineTitleText != null) examineTitleText.enabled = false;
        if (examineDescriptionText != null) examineDescriptionText.enabled = false;

        Transform t = inspectedItem;
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