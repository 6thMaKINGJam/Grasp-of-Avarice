using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemController : MonoBehaviour
{
    public static ItemController Instance;
    public List<Item> nearbyItems = new List<Item>();

    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform playertransform;

    private void Awake(){
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
        if(Input.GetKeyDown(KeyCode.Q)){
            DropLastItem();
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

    // 인벤토리에서 마지막 아이템을 제거하고 월드에 드롭
    private void DropLastItem(){
        if(playerInventory.TryRemoveLastFilled(out ItemData droppedItemData)){
            if(droppedItemData != null){
                Vector3 spawnPos = playertransform.position;
                spawnPos.y -= 0.1f;
                spawnPos.z = 0f;

                GameObject droppedItem = Instantiate(droppedItemData.worldPrefab, spawnPos, Quaternion.identity);

                Item itemScript = droppedItem.GetComponent<Item>();
                if(itemScript != null){
                    itemScript.itemData = droppedItemData;
                }
                Debug.Log($"{droppedItemData.itemName}를 버림!");
            }
            else{
                Debug.Log("버릴 아이템이 없음!");
            }
        }
    }
}
