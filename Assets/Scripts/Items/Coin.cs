using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private bool isPlayerNearby = false;
    // Update is called once per frame
    void Update()
    {
        if(isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            //아이템 먹기
            Debug.Log("먹음");
            Destroy(gameObject); //아직 인벤 저장 구현이 안 됨
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player"){
            //현재 플레이어와 닿은 상태
            isPlayerNearby = true;
            Debug.Log("닿음!");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            isPlayerNearby = false;
            Debug.Log("멀어짐");
        }
    }
}
