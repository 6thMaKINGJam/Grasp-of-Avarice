using UnityEngine;

public abstract class Item : MonoBehaviour
{
    /// <summary>
    /// 아이템이 플레이어에게 줍힐 때 호출됩니다.
    /// 기본 동작은 해당 게임오브젝트를 파괴하는 것 입니다.
    /// 필요하면 서브클래스에서 오버라이드하세요.
    /// </summary>

    // 아이템이 주워졌을 때 호출되는 함수
    public virtual void OnPicked(GameObject picker)
    {
        Destroy(gameObject);
    }
}