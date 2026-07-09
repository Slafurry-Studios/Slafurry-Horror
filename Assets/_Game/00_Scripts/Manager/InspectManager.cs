using UnityEngine;
public class InspectManager : MonoBehaviour
{
    public static InspectManager instance;
    [Header("Pengaturan Default Semua Objek Inspectable")]
    [Tooltip("Gunakan {0} untuk menyisipkan nama objek secara otomatis.")]
    public string promptFormat = "Tekan E untuk periksa {0}";
    public float examineDistance = 0.5f;
    public float examineScaleMultiplier = 1f;
    void Awake()
    {
        instance = this;
    }
}