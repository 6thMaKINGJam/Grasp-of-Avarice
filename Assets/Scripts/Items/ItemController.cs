using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public static ItemController Instance;

    public List<Item> nearbyItems = new List<Item>();

    [SerializeField] private PlayerInventory playerInventory;

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
            Debug.Log($"주변 아이템 개수: {nearbyItems.Count}");
            foreach(var item in nearbyItems){
                Debug.Log($"{item.itemData.itemName} ({item.instanceID})를 버림");
            }
        }
    }

    public void AddItemToNearby(Item item){
        if(!nearbyItems.Contains(item)) nearbyItems.Add(item);
    }
    
    public void RemoveItemFromNearby(Item item){
        if(nearbyItems.Contains(item))  nearbyItems.Remove(item);
    }

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
}
