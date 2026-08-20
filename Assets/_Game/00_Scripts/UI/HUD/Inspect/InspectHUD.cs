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

    [Tooltip("Jarak object dari examine point")]
    public float inspectDistance = 0.5f;

    [Tooltip("Kecepatan rotate object dengan mouse")]
    public float rotationSpeed = 100f;

    private FirstPersonLook firstPersonLook;
    private UIFade uIFade;

    private Transform inspectedItem;
    private Transform inspectPivot;
    

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;

    private Vector3 previousMousePosition;

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
        GameObject target)
    {
        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        firstPersonLook?.LockRotation();

        InspectObject(target.transform);

        uIFade?.FadeIn();
    }
    private void PickUpObject()
    {
        if (inspectedItem != null)
            return;
    }

    public void ReceiveData(
        string title,
        string description,
        Transform target)
    {
        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        firstPersonLook?.LockRotation();

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

        Bounds bounds = CalculateBounds(target);

        GameObject pivotObject = new GameObject("InspectPivot");
        inspectPivot = pivotObject.transform;

        inspectPivot.position = bounds.center;
        inspectPivot.rotation = Quaternion.identity;

        inspectPivot.SetParent(examinePoint, true);

        // Masukkan object ke pivot
        target.SetParent(inspectPivot, true);

        inspectPivot.localPosition = new Vector3(
            0f,
            0f,
            inspectDistance
        );

        // Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

            float rotateX =
                delta.y * rotationSpeed * Time.deltaTime;

            float rotateY =
                -delta.x * rotationSpeed * Time.deltaTime;

            inspectPivot.rotation =
                Quaternion.Euler(
                    rotateX,
                    rotateY,
                    0f
                ) * inspectPivot.rotation;

            previousMousePosition = Input.mousePosition;
        }
    }

    public void Hide()
    {
        RestoreObject();

        firstPersonLook?.UnlockRotation();
        firstPersonLook?.HideCursor();

        uIFade?.FadeOut();
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