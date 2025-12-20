using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Slots")]
    [SerializeField] private InventorySlotUI[] slotUIs;

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
    }


    private void OnDisable()
    {
        if (inventory == null) return;
        inventory.OnChanged -= Refresh;
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
        }
    }
}
