using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    public ItemData itemData;
    public int instanceID;

    public bool isPlayerNearby = false;

    [HideInInspector]
    public bool IsDroppedByPlayer = false;

    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.gameObject.tag == "Player"){
            //현재 플레이어와 닿은 상태
            isPlayerNearby = true;
            Debug.Log($"{itemData.itemName}에 닿음!");

            ItemController.Instance.AddItemToNearby(this);
        }
    }
    private void OnTriggerExit2D(Collider2D collision){
        if(collision.gameObject.tag == "Player"){
            //현재 플레이어와 떨어진 상태
            isPlayerNearby = false;
            Debug.Log($"{itemData.itemName}에서 멀어짐!");

            ItemController.Instance.RemoveItemFromNearby(this);
        }
    }
    public virtual void OnPickedUp()
    {
        Debug.Log($"{itemData.itemName} ({instanceID}번)을 획득!");
        Destroy(gameObject);
    }

    public virtual void OnDroppedByPlayer()
    {
        Debug.Log($"{itemData.itemName} ({instanceID})가 플레이어에 의해 버려짐.");
    }
}
