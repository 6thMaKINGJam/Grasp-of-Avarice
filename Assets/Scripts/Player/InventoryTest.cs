using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public PlayerInventory inventory;
    public ItemData testItem;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            inventory.TryAdd(testItem);

        if (Input.GetKeyDown(KeyCode.Q))
            inventory.TryRemoveLastFilled(out _);
    }
}
