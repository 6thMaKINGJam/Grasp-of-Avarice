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

    private void Awake(){
        Instance = this;
    }

    // Update is called once per frame
    void Update(){
        if(Input.GetKeyDown(KeyCode.E)){
            if(nearbyItems.Count > 0)    PickUpPriorityItem();
            else    Debug.Log("남은 아이템이 없음!");
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
            target.OnPickedUp();
            nearbyItems.Remove(target);
        }
    }
}
