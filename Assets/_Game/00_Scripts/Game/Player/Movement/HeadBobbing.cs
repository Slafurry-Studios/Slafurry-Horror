using UnityEngine;
using Cinemachine;

public class HeadBobbing : MonoBehaviour
{
    [Header("Aktif/Nonaktif")]
    public bool enableBob = true;

    [Header("Bobbing saat jalan")]
    public float bobSpeed = 10f;
    public float bobAmountY = 0.045f;
    public float bobAmountX = 0.02f;

    [Header("Pengali saat lari")]
    public float runSpeedMultiplier = 1.6f;
    public float runAmountMultiplier = 1.7f;

    [Header("Transisi")]
    public float blendInSpeed = 10f;
    public float resetSpeed = 6f;
    public float minSpeedThreshold = 0.1f;

    [Header("Referensi")]
    public FirstPersonMovement playerMovement;
    public CinemachineVirtualCamera virtualCamera;

    private CinemachineTransposer transposer;
    private float bobTimer;
    private Vector3 currentBobOffset = Vector3.zero;
    private Vector3 lastAppliedOffset = Vector3.zero;

    void Start()
    {
        if (playerMovement == null) playerMovement = FirstPersonMovement.instance;
        if (virtualCamera != null) transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
    }

    void Update()
    {
        if (!enableBob || playerMovement == null || transposer == null) return;
        CheckMotion();
        ApplyBobToCamera();
    }

    private void CheckMotion()
    {
        Vector3 horizontalVelocity = playerMovement.HorizontalVelocity;
        bool isMoving = horizontalVelocity.magnitude > minSpeedThreshold && !playerMovement.IsCrouching;

        if (!isMoving)
        {
            ResetPosition();
            return;
        }

        float speedMul = playerMovement.IsRunning ? runSpeedMultiplier : 1f;
        float amountMul = playerMovement.IsRunning ? runAmountMultiplier : 1f;

        bobTimer += Time.deltaTime * bobSpeed * speedMul;
        Vector3 target = FootStepMotion(amountMul);
        currentBobOffset = Vector3.Lerp(currentBobOffset, target, Time.deltaTime * blendInSpeed);
    }

    private Vector3 FootStepMotion(float amountMultiplier)
    {
        float y = Mathf.Sin(bobTimer) * bobAmountY * amountMultiplier;
        float x = Mathf.Cos(bobTimer * 0.5f) * bobAmountX * amountMultiplier;
        return new Vector3(x, y, 0f);
    }

    private Vector3 FocusTarget()
    {
        return Vector3.zero;
    }

    private void ResetPosition()
    {
        bobTimer = 0f;
        currentBobOffset = Vector3.Lerp(currentBobOffset, FocusTarget(), Time.deltaTime * resetSpeed);
    }

    private void ApplyBobToCamera()
    {
        Vector3 offset = transposer.m_FollowOffset;
        offset -= lastAppliedOffset;
        offset += currentBobOffset;
        transposer.m_FollowOffset = offset;
        lastAppliedOffset = currentBobOffset;
    }
}