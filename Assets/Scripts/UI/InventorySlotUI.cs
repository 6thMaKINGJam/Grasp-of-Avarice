using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject emptyOverlay;

    private PlayerInventory _inventory;

    public void Bind(PlayerInventory inventory, int index)
    {
        _inventory = inventory;
    }

    public void SetItem(ItemData item)
    {
        if (iconImage != null)
        {
            iconImage.enabled = (item != null);
            iconImage.sprite = item != null ? item.icon : null;
        }

        if (emptyOverlay != null)
            emptyOverlay.SetActive(item == null);
    }
}
