using System.Collections;
using Slafurry.Core.Abstract;
using Slafurry.System.Scene;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    private PlayerInventory playerInventory;
    private PlayerHand playerHand;

    public PlayerInventory PlayerInventory => playerInventory;
    public PlayerHand PlayerHand => playerHand;


    [SerializeField] private Transform inventoryContainer;

    public Transform InventoryContainer => inventoryContainer;

    private int currentSlot = -1;

    public int CurrentSlot => currentSlot;

    public void SetCurrentSlot(int slot)
    {
        currentSlot = slot;
    }

    private void Start()
    {
        TrySubscribeToSceneLoader();
    }

    // Start() pada singleton persistent seperti ini hanya dieksekusi SEKALI
    // seumur hidup game. Kalau saat itu SceneLoader.Instance belum siap
    // (race condition antar-Awake/Start di scene pertama), subscription
    // akan gagal secara diam-diam dan tidak akan pernah dicoba ulang.
    // OnEnable dipanggil setiap kali object diaktifkan ulang, jadi kita
    // pakai ini sebagai kesempatan tambahan untuk retry subscribe.
    private void OnEnable()
    {
        TrySubscribeToSceneLoader();
    }

    private void TrySubscribeToSceneLoader()
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogWarning("[PlayerManager] SceneLoader.Instance belum siap, subscription OnSceneLoadStarted ditunda.");
            return;
        }

        SceneLoader.Instance.OnSceneLoadStarted -= HandleSceneLoadStarted;
        SceneLoader.Instance.OnSceneLoadStarted += HandleSceneLoadStarted;
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.OnSceneLoadStarted -= HandleSceneLoadStarted;
    }

    private void HandleSceneLoadStarted(string sceneName)
    {
        // playerHand?.DetachCarriedItemForSceneTransition();
    }

    public override IEnumerator Initialize()
    {
        yield return null;
    }

    public override void PostInitialize()
    {
    }

    protected override void OnSingletonAwake()
    {
        playerInventory = FindAnyObjectByType<PlayerInventory>();
        playerHand = FindAnyObjectByType<PlayerHand>();

        DontDestroyOnLoad(gameObject);

        // inventoryContainer wajib ikut persistent bersama PlayerManager.
        // Kalau Transform ini di-assign lewat Inspector sebagai objek yang
        // hidup di scene (bukan child dari PlayerManager), dia akan ikut
        // di-destroy setiap kali scene berganti, dan semua item yang
        // di-parent ke sini (termasuk yang "diselamatkan" via
        // DetachCarriedItemForSceneTransition) ikut lenyap.
        if (inventoryContainer != null && inventoryContainer.gameObject != gameObject)
        {
            DontDestroyOnLoad(inventoryContainer.gameObject);
        }
    }


    public void SetPlayerInventory(PlayerInventory playerInventory)
    {
        this.playerInventory = playerInventory;
    }

    public void SetPlayerHand(PlayerHand playerHand)
    {
        this.playerHand = playerHand;
    }
}