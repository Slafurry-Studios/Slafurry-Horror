using Slafurry.Utils.UI;
using TMPro;
using UnityEngine;

public class InspectHUD : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    public GameObject inspectPanel;

    [Header("Inspect Object")]
    public Transform examinePoint;

    [Header("NeedsToHide")]
    [SerializeField] private CanvasGroup[] hideGroups;

    [Tooltip("Jarak object dari examine point")]
    public float inspectDistance = 0.5f;

    [Tooltip("Kecepatan rotate object dengan mouse")]
    public float rotationSpeed = 100f;

    [Tooltip("Rotasi awal objek relatif ke kamera saat mulai di-inspect")]
    public Vector3 defaultInspectEuler = new Vector3(0f, 180f, 0f);

    private FirstPersonLook firstPersonLook;
    private UIFade uIFade;

    private Transform inspectedItem;
    private Transform inspectPivot;
    private InspectableObject currentSource;


    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;

    private Vector3 previousMousePosition;

    private bool playerMovementLocked;

    void Awake()
    {
        firstPersonLook = FindAnyObjectByType<FirstPersonLook>();
        uIFade = GetComponentInChildren<UIFade>();
    }

    void Update()
    {
        if (inspectedItem == null)
            return;

        HandleRotation();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void ReceiveData(
        string title,
        string description,
        GameObject target,
        InspectableObject source = null)
    {
        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        currentSource = source;

        firstPersonLook?.LockRotation();
        LockPlayerMovement();

        if (PlayerInteract.instance != null)
            PlayerInteract.instance.SetInteractionEnabled(false);

        InspectObject(target.transform);

        uIFade?.FadeIn();
    }

    public void ReceiveData(
        string title,
        string description,
        Transform target,
        InspectableObject source = null)
    {
        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        currentSource = source;

        firstPersonLook?.LockRotation();
        LockPlayerMovement();

        if (PlayerInteract.instance != null)
            PlayerInteract.instance.SetInteractionEnabled(false);

        InspectObject(target);

        uIFade?.FadeIn();
    }

    private void InspectObject(Transform target)
    {
        if (target == null)
            return;

        if (inspectedItem != null)
            return;

        inspectedItem = target;

        originalParent = target.parent;
        originalLocalPosition = target.localPosition;
        originalLocalRotation = target.localRotation;
        originalLocalScale = target.localScale;

        // fix bug to make them face camera oninspect
        GameObject pivotObject = new GameObject("InspectPivot");
        inspectPivot = pivotObject.transform;

        inspectPivot.SetParent(examinePoint, false);

        inspectPivot.localPosition = new Vector3(
            0f,
            0f,
            inspectDistance
        );

        inspectPivot.localRotation = Quaternion.identity;

        target.SetParent(inspectPivot, false);

        target.localRotation = Quaternion.Euler(defaultInspectEuler);
        target.localPosition = Vector3.zero;

        Bounds bounds = CalculateBounds(target);

        target.localPosition -=
            inspectPivot.InverseTransformPoint(bounds.center);

        // Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var cg in hideGroups)
        {
            cg.alpha = 0f;
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta =
                Input.mousePosition - previousMousePosition;

            float speed = rotationSpeed * Time.deltaTime;

            inspectPivot.Rotate(
                examinePoint.up,
                -delta.x * speed,
                Space.World
            );

            inspectPivot.Rotate(
                examinePoint.right,
                delta.y * speed,
                Space.World
            );

            previousMousePosition = Input.mousePosition;
        }
    }

    public void Hide()
    {
        bool wasInspecting = inspectedItem != null;
        InspectableObject source = currentSource;

        RestoreObject();

        currentSource = null;

        firstPersonLook?.UnlockRotation();
        firstPersonLook?.HideCursor();
        UnlockPlayerMovement();

        if (PlayerInteract.instance != null)
            PlayerInteract.instance.SetInteractionEnabled(true);

        uIFade?.FadeOut();

        if (wasInspecting && source != null)
            source.NotifyInspectClosed();

        foreach (var cg in hideGroups)
        {
            cg.alpha = 1f;
        }
    }

    private void LockPlayerMovement()
    {
        if (playerMovementLocked) return;
        if (FirstPersonMovement.instance == null) return;

        FirstPersonMovement.instance.LockMovement();
        playerMovementLocked = true;
    }

    private void UnlockPlayerMovement()
    {
        if (!playerMovementLocked) return;
        playerMovementLocked = false;

        if (FirstPersonMovement.instance == null) return;
        FirstPersonMovement.instance.UnlockMovement();
    }

    void OnDisable()
    {
        // jangan sampai player ketinggalan kekunci kalau HUD-nya dimatikan / scene ganti
        UnlockPlayerMovement();
    }

    private void RestoreObject()
    {
        if (inspectedItem == null)
            return;

        inspectedItem.SetParent(originalParent, true);

        inspectedItem.localPosition = originalLocalPosition;
        inspectedItem.localRotation = originalLocalRotation;
        inspectedItem.localScale = originalLocalScale;

        if (inspectPivot != null)
        {
            Destroy(inspectPivot.gameObject);
        }

        inspectedItem = null;
        inspectPivot = null;
    }

    private Bounds CalculateBounds(Transform target)
    {
        Renderer[] renderers =
            target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return new Bounds(
                target.position,
                Vector3.zero
            );
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
}