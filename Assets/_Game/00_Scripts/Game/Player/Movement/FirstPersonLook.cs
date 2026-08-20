using UnityEngine;
using Slafurry.System.Pause;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform character;

    [Tooltip("Transform that actually gets the pitch rotation applied (usually the camera or its holder). " +
             "Leave empty to use this script's own transform - so this can still sit directly on the camera " +
             "if you want, but it no longer has to.")]
    [SerializeField] private Transform cameraPivot;

    public float sensitivity = 2f;
    public float smoothing = 1.5f;

    [Header("Rotation Lock")]
    public bool rotationLocked = false;

    private Vector2 velocity;
    private Vector2 frameVelocity;

    void Awake()
    {
        if (cameraPivot == null) cameraPivot = transform;
    }

    void Start()
    {
        if (FirstPersonMovement.instance != null)
        {
            character = FirstPersonMovement.instance.transform;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SyncVelocityWithCurrentRotation();
    }

    void Update()
    {
        if (rotationLocked || Pause.IsPaused)
        {
            frameVelocity = Vector2.zero;
            return;
        }
        if (character == null) return;

        Vector2 mouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1f / smoothing);

        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90f, 90f);

        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
        cameraPivot.rotation = character.rotation * Quaternion.AngleAxis(-velocity.y, Vector3.right);
    }

    public void LockRotation()
    {
        rotationLocked = true;
        frameVelocity = Vector2.zero;
    }

    public void UnlockRotation()
    {
        SyncVelocityWithCurrentRotation();
        frameVelocity = Vector2.zero;
        rotationLocked = false;
    }

    private void SyncVelocityWithCurrentRotation()
    {
        float cameraPitch = cameraPivot.localEulerAngles.x;
        if (cameraPitch > 180f) cameraPitch -= 360f;
        velocity.y = -cameraPitch;

        if (character != null)
        {
            float characterYaw = character.localEulerAngles.y;
            if (characterYaw > 180f) characterYaw -= 360f;
            velocity.x = characterYaw;
        }
    }
}