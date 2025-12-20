using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public static ItemController Instance;

    public List<Item> nearbyItems = new List<Item>();

    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform playertransform;

    private void Awake(){
        Instance = this;
    }

    // Update is called once per frame
    void Update(){
        if(Input.GetKeyDown(KeyCode.E)){
            if(nearbyItems.Count > 0)    PickUpPriorityItem();
            else    Debug.Log("남은 아이템이 없음!");
        }
        if(Input.GetKeyDown(KeyCode.Q)){
            DropLastItem();
        }
    }

    // 근처 아이템을 획득 가능 리스트에 표시/제거하는 함수
    public void AddItemToNearby(Item item){
        if(!nearbyItems.Contains(item)) nearbyItems.Add(item);
    }
    
    public void RemoveItemFromNearby(Item item){
        if(nearbyItems.Contains(item))  nearbyItems.Remove(item);
    }

    // 아이템 우선순위에 따라 아이템을 획득
    private void PickUpPriorityItem(){
        Item target = nearbyItems
            .OrderBy(x => (int)x.itemData.itemType)
            .ThenBy(x => x.instanceID)
            .FirstOrDefault();
        
        if(target != null){
            bool isAdded = playerInventory.TryAdd(target.itemData);
            if(isAdded){
                Debug.Log($"{target.itemData.itemName} ({target.instanceID})를 인벤토리에 넣음!");
                target.OnPickedUp();
                nearbyItems.Remove(target);
            }
            else{
                Debug.Log($"인벤토리가 꽉 차서 {target.itemData.itemName} ({target.instanceID})를 못 넣음!");
            }
        }
    }

    // 인벤토리에서 마지막 아이템을 제거하고 월드에 드롭
    private void DropLastItem(){
        if(playerInventory.TryRemoveLastFilled(out ItemData droppedItemData)){
            if(droppedItemData != null){
                Vector3 spawnPos = playertransform.position;
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
