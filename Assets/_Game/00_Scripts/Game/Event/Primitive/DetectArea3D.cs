using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class DetectArea3D : MonoBehaviour
{
    [Header("On Trigger Enter")]
    [TagSelector][SerializeField] private string enteredObjectTag = "Untagged";
    [SerializeField] private UnityEvent onTriggerEnter;
    [SerializeField] private UnityEvent onTriggerStay;
    [SerializeField] private UnityEvent onTriggerExit;
    [SerializeField] 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enteredObjectTag))
        {
            onTriggerEnter.Invoke();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(enteredObjectTag))
        {
            onTriggerStay.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enteredObjectTag))
        {
            onTriggerExit.Invoke();
        }
    }
}