using UnityEngine;

public class InspectableItem : MonoBehaviour
{
    public string promptLabel = "Press E to Inspect";

    [Header("Jarak Inspect")]
    public float examineDistance = 1.2f;
    [Tooltip("Batas paling dekat & paling jauh saat di-scroll zoom.")]
    public float minExamineDistance = 0.6f;
    public float maxExamineDistance = 2.5f;

    public float examineScaleMultiplier = 1f;
}