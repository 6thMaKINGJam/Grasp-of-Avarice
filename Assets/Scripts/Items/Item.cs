using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public int priority = 0; // 아이템 우선순위

    // 아이템이 주워졌을 때 호출되는 함수
    public virtual void OnPicked(GameObject picker)
    {
        Destroy(gameObject);
    }
}