using UnityEngine;

public enum ItemType {Coin = 0, Bomb = 1, Box = 2}
[CreateAssetMenu(menuName = "Game/Item Data", fileName = "ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public Sprite icon;

    // 나중에 드롭할 월드 프리팹 필요하면 사용
    public GameObject worldPrefab;
}
