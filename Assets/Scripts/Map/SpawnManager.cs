using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Current Spawn")]
    [SerializeField] private Transform currentSpawnPoint; // 지금 리스폰 위치

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetDefaultSpawn(Transform spawnPoint)
    {
        // 맵 시작 스폰(디폴트). 아직 스폰이 없을 때만 세팅
        if (currentSpawnPoint == null)
            currentSpawnPoint = spawnPoint;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentSpawnPoint = checkpoint;
    }

    public Vector3 GetSpawnPosition()
    {
        return currentSpawnPoint != null ? currentSpawnPoint.position : Vector3.zero;
    }

    public void Respawn(PlayerLife player)
    {
        if (player == null) return;

        Collider2D pc = player.GetComponent<Collider2D>();
        if (pc) pc.enabled = false;

        // 1) 플레이어 먼저 스폰으로 이동
        player.transform.position = GetSpawnPosition();
        Rigidbody2D prb = player.GetComponent<Rigidbody2D>();
        if (prb)
        {
            prb.velocity = Vector2.zero;
            prb.angularVelocity = 0f;
        }
        player.ResetLifeToOne();

        // 2) 그 다음 함정/가시 리셋
        var monos = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var m in monos)
            if (m is IResettable r) r.ResetState();

        // 3) 콜라이더 다시 켜기
        if (pc) pc.enabled = true;
    }
}
