using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class ObjectTrigger : MonoBehaviour
{
    [Header("On Trigger Enter")]
    [TagSelector] [SerializeField] private string enteredObjectTag = "Untagged";
    [SerializeField] private UnityEvent actions;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enteredObjectTag))
        {
            actions.Invoke();
        }
    }
}