using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemController : MonoBehaviour
{
    public static ItemController Instance { get; private set; }

    [Header("Nearby Items")]
    public List<Item> nearbyItems = new List<Item>();

    [Header("Refs")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform playertransform;

    [Header("Ending Gate")]
    [SerializeField] private bool dropHat = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BindRefsImmediate();
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
        StartCoroutine(RebindNextFrame());
    }

    private IEnumerator RebindNextFrame()
    {
        yield return null;
        BindRefsImmediate();
    }

    private void BindRefsImmediate()
    {
        playerInventory = PlayerInventory.Instance;
        playertransform = PlayerSingleton.Tr;
        if (playertransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playertransform = p.transform;
        }
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
            DropLastItem();
        }
    }

    public void AddItemToNearby(Item item)
    {
        if (item == null) return;
        if (item.itemData == null) return;

        if (!nearbyItems.Contains(item))
            nearbyItems.Add(item);
    }

    public void RemoveItemFromNearby(Item item)
    {
        if (item == null) return;

        if (nearbyItems.Contains(item))
            nearbyItems.Remove(item);
    }

    private void CleanupNearbyList()
    {
        nearbyItems.RemoveAll(x => x == null || x.itemData == null);
    }

    private void PickUpPriorityItem()
    {
        CleanupNearbyList();
        if (nearbyItems.Count == 0) return;

        if (playerInventory == null)
            playerInventory = PlayerInventory.Instance;

        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory.Instance가 null입니다. 인벤토리 오브젝트가 존재하는지/싱글톤 설정 확인!");
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
            AudioManager.Instance?.PlaySfx(SfxType.ItemPickup);
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
        int lastSlotIndex = playerInventory.GetLastFilledIndex();

        if(lastSlotIndex == -1){
            Debug.Log("버릴 아이템이 없음!");
            return;
        }
        
        ItemData dataToCheck = playerInventory.GetItem(lastSlotIndex);

        if (dataToCheck != null)
        {
            if (!dataToCheck.canDrop && !dropHat)
            {
                AudioManager.Instance?.PlaySfx(SfxType.No);
                Debug.Log($"{dataToCheck.itemName}은(는) 버릴 수 없는 아이템입니다!");
                return;
            }
        }


         // 인벤토리 참조 보장
        if(playerInventory.TryRemoveLastFilled(out ItemData droppedItemData)){
            AudioManager.Instance?.PlaySfx(SfxType.ItemDrop);
            if(droppedItemData != null){
                Vector3 spawnPos = playertransform.position;
                spawnPos.y -= 0.1f;
                spawnPos.z = 0f;

                Debug.Log($"{droppedItemData.itemName}를 버림!");
                GameObject droppedItem = Instantiate(droppedItemData.worldPrefab, spawnPos, Quaternion.identity);
                Item itemScript = droppedItem.GetComponent<Item>();
                if(itemScript != null){
                    itemScript.itemData = droppedItemData;
                    itemScript.IsDroppedByPlayer = true;
                    itemScript.OnDroppedByPlayer();
                }
            }
        }
    }

    public void SetEndingDropPermission(bool allow)
    {
        dropHat = allow;
    }

}