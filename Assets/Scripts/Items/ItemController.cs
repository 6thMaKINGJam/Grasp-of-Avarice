using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemController : MonoBehaviour
{
    public static ItemController Instance;
    public List<Item> nearbyItems = new List<Item>();
    private PlayerInventory playerInventory;

    private void Awake()
    {
        // if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        playerInventory = PlayerInventory.Instance;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        nearbyItems.Clear();
        playerInventory = PlayerInventory.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            CleanupNearbyList();

            if (nearbyItems.Count > 0) PickUpPriorityItem();
            else Debug.Log("남은 아이템이 없음!");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            CleanupNearbyList();

            Debug.Log($"주변 아이템 개수: {nearbyItems.Count}");
            foreach (var item in nearbyItems)
            {
                string itemName = (item != null && item.itemData != null) ? item.itemData.itemName : "NULL_ITEM_OR_DATA";
                int id = (item != null) ? item.instanceID : -1;
                Debug.Log($"{itemName} ({id})");
            }
        }
    }

    public void AddItemToNearby(Item item)
    {
        if (item == null) return;
        if (!nearbyItems.Contains(item)) nearbyItems.Add(item);
    }

    public void RemoveItemFromNearby(Item item)
    {
        if (item == null) return;
        if (nearbyItems.Contains(item)) nearbyItems.Remove(item);
    }

    private void CleanupNearbyList()
    {
        // 씬 이동/파괴로 인해 리스트에 null이 남는 경우 제거
        nearbyItems.RemoveAll(x => x == null || x.itemData == null);
    }

    private void PickUpPriorityItem()
    {
        CleanupNearbyList();
        if (nearbyItems.Count == 0) return;

        // 인벤토리 참조 보장
        if (playerInventory == null)
            playerInventory = PlayerInventory.Instance;

        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory.Instance가 null입니다. 인벤토리 오브젝트가 씬에 존재하는지/싱글톤 설정이 되었는지 확인하세요.");
            return;
        }

        // 우선순위: itemType 오름차순 -> instanceID 오름차순
        Item target = nearbyItems
            .OrderBy(x => (int)x.itemData.itemType)
            .ThenBy(x => x.instanceID)
            .FirstOrDefault();

        if (target == null || target.itemData == null) return;

        bool isAdded = playerInventory.TryAdd(target.itemData);
        if (isAdded)
        {
            Debug.Log($"{target.itemData.itemName} ({target.instanceID})를 인벤토리에 넣음!");
            target.OnPickedUp();
            nearbyItems.Remove(target);
        }
        else
        {
            Debug.Log($"인벤토리가 꽉 차서 {target.itemData.itemName} ({target.instanceID})를 못 넣음!");
        }
    }
}
