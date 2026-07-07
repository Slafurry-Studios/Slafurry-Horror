using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonMovement : MonoBehaviour
{
    public static FirstPersonMovement instance;

    [Header("Movement")]
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Jump")]
    public bool canJump = true;
    public float jumpForce = 6f;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundedStickForce = -2f;

    [Header("Crouch")]
    public bool canCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchSpeed = 2.5f;
    public float crouchHeight = 1f;
    [Range(0f, 1.5f)]
    public float crouchCameraDrop = 0.5f;
    public LayerMask obstacleMask;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform cameraHolder;

    public bool IsRunning { get; private set; }
    public bool IsCrouching { get; private set; }
    public Vector3 HorizontalVelocity => lastHorizontalVelocity;
    public bool IsGrounded => controller.isGrounded;

    [HideInInspector] public CharacterController controller;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private CinemachineTransposer transposer;
    private float standOffsetY;
    private float standLocalY;

    private float standHeight;
    private Vector3 standCenter;

    private Vector3 verticalVelocity;
    private Vector3 lastHorizontalVelocity;

    void Awake()
    {
        instance = this;
        controller = GetComponent<CharacterController>();

        standHeight = controller.height;
        standCenter = controller.center;

        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null) standOffsetY = transposer.m_FollowOffset.y;
        }
        if (cameraHolder != null) standLocalY = cameraHolder.localPosition.y;
    }

    void Update()
    {
        if (canJump && Input.GetKeyDown(jumpKey)) Jump();
        if (canCrouch && Input.GetKeyDown(crouchKey)) ToggleCrouch();
        HandleMovement();
    }

    private void HandleMovement()
    {
        bool grounded = controller.isGrounded;

        float targetSpeed = IsRunning ? runSpeed : speed;
        if (IsCrouching) targetSpeed = crouchSpeed;
        if (speedOverrides.Count > 0) targetSpeed = speedOverrides[speedOverrides.Count - 1]();

        IsRunning = canRun && !IsCrouching && Input.GetKey(runningKey);

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 horizontalMove = transform.rotation * new Vector3(input.x, 0f, input.y) * targetSpeed;

        if (grounded && verticalVelocity.y < 0f) verticalVelocity.y = groundedStickForce;
        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move((horizontalMove + verticalVelocity) * Time.deltaTime);
        lastHorizontalVelocity = horizontalMove;
    }

    public void Jump()
    {
        if (!canJump || !controller.isGrounded || IsCrouching) return;
        verticalVelocity.y = jumpForce;
    }

    public void ToggleCrouch()
    {
        if (!canCrouch) return;

        if (IsCrouching)
        {
            if (!CanStandUp()) return;
            IsCrouching = false;
        }
        else
        {
            IsCrouching = true;
        }

        ApplyCrouchVisuals();
    }

    private bool CanStandUp()
    {
        float currentTop = controller.center.y + controller.height / 2f;
        float standTop = standCenter.y + standHeight / 2f;
        float extraHeightNeeded = standTop - currentTop;

        float skin = 0.05f;
        Vector3 rayOrigin = transform.position + Vector3.up * (currentTop - skin);

        bool blocked = Physics.Raycast(rayOrigin, Vector3.up, extraHeightNeeded + skin, obstacleMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    private void ApplyCrouchVisuals()
    {
        controller.height = IsCrouching ? crouchHeight : standHeight;

        float centerY = IsCrouching
            ? standCenter.y - (standHeight - crouchHeight) / 2f
            : standCenter.y;
        controller.center = new Vector3(standCenter.x, centerY, standCenter.z);

        if (transposer != null)
        {
            Vector3 offset = transposer.m_FollowOffset;
            offset.y = IsCrouching ? standOffsetY - crouchCameraDrop : standOffsetY;
            transposer.m_FollowOffset = offset;
        }
        else if (cameraHolder != null)
        {
            Vector3 pos = cameraHolder.localPosition;
            pos.y = IsCrouching ? standLocalY - crouchCameraDrop : standLocalY;
            cameraHolder.localPosition = pos;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (controller == null) return;

        float currentTop = controller.center.y + controller.height / 2f;
        float standTop = Application.isPlaying
            ? standCenter.y + standHeight / 2f
            : controller.center.y + controller.height / 2f; 

        Gizmos.color = IsCrouching ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * currentTop, controller.radius * 0.9f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * standTop, controller.radius * 0.9f);
    }
}