using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)] 
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int capacity = 15;

    [Tooltip("인벤토리 채워지는 순서(슬롯 인덱스 순서). 예: [0,1,2,...]가 아니라 UI 번호 순서대로 넣기")]
    [SerializeField] private int[] fillOrder;

    [Header("Game Over")]
    [Tooltip("인벤토리가 꽉 찬 상태에서 아이템을 더 주우면 이 씬으로 이동")]
    [SerializeField] private string restartSceneName = "Main";

    private ItemData[] _slots;

    public event Action OnChanged;
    public event Action OnGameOver;

    public int Capacity => Mathf.Max(1, capacity);

    public static PlayerInventory Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureInit();
    }

    private void OnValidate()
    {
        if (capacity <= 0) capacity = 1;
        if (fillOrder == null || fillOrder.Length != capacity)
        {
            fillOrder = new int[capacity];
            for (int i = 0; i < capacity; i++) fillOrder[i] = i;
        }
    }

    private void EnsureInit()
    {
        if (_slots == null || _slots.Length != Capacity)
            _slots = new ItemData[Capacity];

        if (fillOrder == null || fillOrder.Length != Capacity)
        {
            fillOrder = new int[Capacity];
            for (int i = 0; i < Capacity; i++) fillOrder[i] = i;
        }
    }

    public ItemData GetItem(int index)
    {
        EnsureInit();

        if (index < 0 || index >= _slots.Length) return null;
        return _slots[index];
    }

    /// <summary>
    /// fillOrder 순서대로 빈칸을 찾아 채움
    /// </summary>
    public bool TryAdd(ItemData item)
    {
        EnsureInit();
        if (item == null) return false;

        for (int k = 0; k < fillOrder.Length; k++)
        {
            int slotIndex = fillOrder[k];
            if (slotIndex < 0 || slotIndex >= _slots.Length) continue;

            if (_slots[slotIndex] == null)
            {
                _slots[slotIndex] = item;
                OnChanged?.Invoke();
                return true;
            }
        }

        TriggerGameOver();
        return false;
    }

    /// <summary>
    /// 스택(LIFO): fillOrder 역순으로 훑어서 마지막으로 채워진 슬롯을 제거
    /// </summary>
    public bool TryRemoveLastFilled(out ItemData removedItem, out int removedSlotIndex)
    {
        removedItem = null;
        removedSlotIndex = -1;

        for (int k = fillOrder.Length - 1; k >= 0; k--)
        {
            int slotIndex = fillOrder[k];
            if (slotIndex < 0 || slotIndex >= _slots.Length) continue;

            if (_slots[slotIndex] != null)
            {
                removedItem = _slots[slotIndex];
                removedSlotIndex = slotIndex;
                _slots[slotIndex] = null;
                OnChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public bool TryRemoveLastFilled(out ItemData removedItem)
    {
        return TryRemoveLastFilled(out removedItem, out _);
    }

    /// <summary>
    /// (선택) UI 하이라이트용: 지금 "가장 최근 아이템"이 들어있는 슬롯 인덱스
    /// </summary>
    public int GetLastFilledIndex()
    {
        for (int k = fillOrder.Length - 1; k >= 0; k--)
        {
            int slotIndex = fillOrder[k];
            if (slotIndex < 0 || slotIndex >= _slots.Length) continue;
            if (_slots[slotIndex] != null) return slotIndex;
        }
        return -1;
    }

    private void TriggerGameOver()
    {
        OnGameOver?.Invoke();

        if (!string.IsNullOrEmpty(restartSceneName))
            SceneManager.LoadScene(restartSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
