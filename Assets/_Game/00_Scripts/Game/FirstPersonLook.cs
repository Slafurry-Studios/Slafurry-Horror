using UnityEngine;
using Slafurry.System.Pause;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2f;
    public float smoothing = 1.5f;
    Vector2 velocity;
    Vector2 frameVelocity;
    [Header("Lock Rotasi")]
    public bool rotasiDikunci = false;
    void Start()
    {
        if (FirstPersonMovement.instance != null)
        {
            character = FirstPersonMovement.instance.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SinkronkanVelocityDenganRotasiSekarang();
    }
    void Update()
    {
        if (rotasiDikunci || Pause.IsPaused)
        {
            frameVelocity = Vector2.zero;
            return;
        }
        if (character == null)
        {
            return;
        }
        Vector2 mouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );
        Vector2 rawFrameVelocity = Vector2.Scale(
            mouseDelta,
            Vector2.one * sensitivity
        );
        frameVelocity = Vector2.Lerp(
            frameVelocity,
            rawFrameVelocity,
            1f / smoothing
        );
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90f, 90f);
        character.localRotation = Quaternion.AngleAxis(
            velocity.x,
            Vector3.up
        );
        transform.rotation = character.rotation * Quaternion.AngleAxis(
            -velocity.y,
            Vector3.right
        );
    }
    public void LockRotasi()
    {
        rotasiDikunci = true;
        frameVelocity = Vector2.zero;
    }
    public void UnlockRotasi()
    {
        SinkronkanVelocityDenganRotasiSekarang();
        frameVelocity = Vector2.zero;
        rotasiDikunci = false;
    }
    private void SinkronkanVelocityDenganRotasiSekarang()
    {
        float cameraPitch = transform.localEulerAngles.x;
        if (cameraPitch > 180f)
        {
            cameraPitch -= 360f;
        }
        velocity.y = -cameraPitch;
        if (character != null)
        {
            float characterYaw = character.localEulerAngles.y;
            if (characterYaw > 180f)
            {
                characterYaw -= 360f;
            }
            velocity.x = characterYaw;
        }
    }
}