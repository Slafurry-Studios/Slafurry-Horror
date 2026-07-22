using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LookAtController : MonoBehaviour
{
    public Transform objectToLookAt;
    public float headWeight = 1f;
    public float bodyWeight = 0.5f;

    [Header("Smooth Settings")]
    public float smoothSpeed = 3f; // kecepatan transisi

    private Animator animator;
    private bool isActive = false;

    private float currentLookWeight = 0f;

    // target sementara, dipakai untuk transisi keluar
    private Transform lastTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        float targetWeight = (isActive && objectToLookAt != null) ? 1f : 0f;

        // transisi halus
        currentLookWeight = Mathf.Lerp(currentLookWeight, targetWeight, Time.deltaTime * smoothSpeed);

        animator.SetLookAtWeight(currentLookWeight, bodyWeight, headWeight);

        // gunakan target lama sampai weight benar-benar 0
        if ((objectToLookAt != null || lastTarget != null) && currentLookWeight > 0.01f)
        {
            Transform target = objectToLookAt != null ? objectToLookAt : lastTarget;
            animator.SetLookAtPosition(target.position);
        }

        // kalau sudah habis weightnya, reset lastTarget
        if (currentLookWeight <= 0.01f)
        {
            lastTarget = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Face"))
        {
            isActive = true;
            objectToLookAt = other.transform;
            lastTarget = objectToLookAt;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Face"))
        {
            isActive = false;
            lastTarget = objectToLookAt; // simpan dulu
            objectToLookAt = null;       // kosongkan target aktif
        }
    }
}