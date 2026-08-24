using UnityEngine;

public class DropTrigger : BaseTrigger
{
    [Header("Drop")]
    [SerializeField] private float dropForce = 2f;
    [SerializeField] private Vector3 localForce = Vector3.down;

    public void Drop()
    {
        Debug.Log($"DROP → {gameObject.name}");

        if (!CanTrigger())
            return;

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        transform.SetParent(null);

        gameObject.SetActive(true);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.detectCollisions = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(
            transform.TransformDirection(localForce.normalized) * dropForce,
            ForceMode.Impulse
        );

        AddTriggerCount();
    }
}