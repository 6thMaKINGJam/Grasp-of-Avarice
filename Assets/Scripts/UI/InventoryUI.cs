using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Slots")]
    [SerializeField] private InventorySlotUI[] slotUIs;

    [Header("Collider Slots (matching index)")]
    [SerializeField] private ColliderSlotUI[] colliderSlotUIs;

    private void Awake()
    {
        inventory = PlayerInventory.Instance;
    }

    private void OnEnable()
    {
        inventory = PlayerInventory.Instance;

        if (inventory == null) return;

        inventory.OnChanged += Refresh;
        Refresh();
        SetColliderSlotsActive(false);
    }

    private void OnDisable()
    {
        if (inventory == null) return;
        inventory.OnChanged -= Refresh;
        SetColliderSlotsActive(true);
    }

    private void Start()
    {
        if (inventory == null) return;
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].Bind(inventory, i);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (inventory == null) return;

        int n = Mathf.Min(inventory.Capacity, slotUIs.Length);
        for (int i = 0; i < n; i++)
        {
            var item = inventory.GetItem(i);
            slotUIs[i].SetItem(item);
            if (item != null)
                colliderSlotUIs[i].SetVisualsActive(true);
            else
                colliderSlotUIs[i].SetVisualsActive(false);
        }
    }

    // slot 전체 끄고 켜고
    private void SetColliderSlotsActive(bool active)
    {
        if (colliderSlotUIs == null) return;
        int n = colliderSlotUIs.Length;
        for (int i = 0; i < n; i++)
        {
            if (colliderSlotUIs[i] == null) continue;
            colliderSlotUIs[i].SetVisualsActive(active);
        }
    }
}
