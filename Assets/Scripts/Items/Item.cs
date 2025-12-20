using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] public ItemData itemData;

    [Header("Runtime")]
    public int instanceID;
    public bool isPlayerNearby = false;

    [HideInInspector]
    public bool IsDroppedByPlayer = false;

    private void Awake()
    {
        if (instanceID == 0)
            instanceID = GetInstanceID();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (itemData == null)
        {
            Debug.LogWarning($"{gameObject.name}에 ItemData가 없습니다. 등록/획득 불가");
            return;
        }

        isPlayerNearby = true;
        Debug.Log($"{itemData.itemName}에 닿음!");

        if (ItemController.Instance != null)
            ItemController.Instance.AddItemToNearby(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (itemData != null)
            Debug.Log($"{itemData.itemName}에서 멀어짐!");
        else
            Debug.Log($"{gameObject.name}에서 멀어짐!");

        isPlayerNearby = false;

        if (ItemController.Instance != null)
            ItemController.Instance.RemoveItemFromNearby(this);
    }

    public virtual void OnPickedUp()
    {
        if (itemData != null)
            Debug.Log($"{itemData.itemName} ({instanceID}번)을 획득!");
        else
            Debug.Log($"{gameObject.name} ({instanceID}번)을 획득!");

        Destroy(gameObject);
    }

    public virtual void OnDroppedByPlayer()
    {
        if (itemData != null)
            Debug.Log($"{itemData.itemName} ({instanceID})가 플레이어에 의해 버려짐.");
        else
            Debug.Log($"{gameObject.name} ({instanceID})가 플레이어에 의해 버려짐.");
    }
}
