using System.Collections;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Slots")]
    [SerializeField] private InventorySlotUI[] slotUIs;

    [Header("Collider Slots (matching index)")]
    [SerializeField] private ColliderSlotUI[] colliderSlotUIs;

    private bool _subscribed = false;

    private void OnEnable()
    {
        StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        Unsubscribe();
        SetColliderSlotsActive(false);
    }

    private IEnumerator BindWhenReady()
    {
        yield return null;
        yield return null;

        inventory = PlayerInventory.Instance;
        if (inventory == null) yield break;

        SubscribeOnce();

        if (slotUIs != null)
        {
            for (int i = 0; i < slotUIs.Length; i++)
            {
                if (slotUIs[i] != null)
                    slotUIs[i].Bind(inventory, i);
            }
        }

        Refresh();
        SetColliderSlotsActive(false);
    }

    private void SubscribeOnce()
    {
        if (_subscribed) return;
        _subscribed = true;
        inventory.OnChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;
        _subscribed = false;

        if (inventory != null)
            inventory.OnChanged -= Refresh;
    }

    public void Refresh()
    {
        if (inventory == null || slotUIs == null) return;

        int n = Mathf.Min(inventory.Capacity, slotUIs.Length);
        for (int i = 0; i < n; i++)
        {
            var item = inventory.GetItem(i);
            if (slotUIs[i] != null)
                slotUIs[i].SetItem(item);

            if (colliderSlotUIs != null && i < colliderSlotUIs.Length && colliderSlotUIs[i] != null)
            {
                colliderSlotUIs[i].SetVisualsActive(item != null);
            }
        }
    }

    private void SetColliderSlotsActive(bool active)
    {
        if (colliderSlotUIs == null) return;
        for (int i = 0; i < colliderSlotUIs.Length; i++)
        {
            if (colliderSlotUIs[i] == null) continue;
            colliderSlotUIs[i].SetVisualsActive(active);
        }
    }
}
