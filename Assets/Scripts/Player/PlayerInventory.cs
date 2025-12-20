using System;
using Unity.VisualScripting;
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
    [SerializeField] private string restartSceneName = "Main";

    private ItemData[] _slots;

    public event Action OnChanged;
    public event Action OnGameOver;

    public int Capacity => Mathf.Max(1, capacity);

    public static PlayerInventory Instance;
    public ItemData startingHat;
    public PlayerLife playerLife;

    private void Awake()
    {
        playerLife = GetComponent<PlayerLife>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureInit();
        if(startingHat != null){
            _slots[0] = startingHat;
            Debug.Log($"시작할 때 {startingHat.itemName}을(를) 착용했습니다.");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInit();

        int filled = 0;
        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i] != null) filled++;

        Debug.Log($"[Inventory] filled={filled}/{_slots.Length}");
        // 필요하면 여기서 OnChanged를 한 번 강제 호출해도 됨:
        // OnChanged?.Invoke();
    }

    private void Start()
    {
        OnChanged?.Invoke();
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
                print(slotIndex + "번 슬롯에 " + item.itemName + " 아이템 추가됨");
                if (k == fillOrder.Length - 1)
                {
                    Debug.Log("인벤토리가 가득 찼습니다!");
                    playerLife.TakeDamage(1);
                }
                return true;
            }
        }

        //playerLife.TakeDamage(1);
        TriggerGameOver();
        return false;
    }

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

    public ItemData upgradedHat;
    public void UpgradeHat()
    {
        EnsureInit();
        if (upgradedHat != null)
        {
            _slots[0] = upgradedHat;
            OnChanged?.Invoke();
            Debug.Log($"모자가 업그레이드되었습니다!");
        }
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
