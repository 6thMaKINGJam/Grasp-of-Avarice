using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Feather : MonoBehaviour
{
    public bool isPlayerNearby = false;
    public ItemData itemData;
    void Update()
    {
        if(FeatherUI.Instance != null){
            if (Input.GetKeyDown(KeyCode.E) && isPlayerNearby){
                Debug.Log("깃털 획득!");
                FeatherUI.Instance.UpdateFeatherCount();
                OnPickedUp();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.gameObject.tag == "Player"){
            //현재 플레이어와 닿은 상태
            isPlayerNearby = true;
            Debug.Log("깃털에 닿음!");
        }
    }
    private void OnTriggerExit2D(Collider2D collision){
        if(collision.gameObject.tag == "Player"){
            //현재 플레이어와 떨어진 상태
            isPlayerNearby = false;
            Debug.Log("깃털에서 멀어짐!");
        }
    }
    public virtual void OnPickedUp()
    {
        Debug.Log($"{FeatherUI.Instance.currentFeatherCount}번째 깃털을 획득!");
        Destroy(gameObject);
    }
}
