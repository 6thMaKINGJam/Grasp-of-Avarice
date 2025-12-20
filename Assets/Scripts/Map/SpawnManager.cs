using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Current Spawn")]
    [SerializeField] private Transform currentSpawnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        Debug.Log($"[SpawnManager] Awake instanceID={GetInstanceID()}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"[SpawnManager] ActiveSceneChanged -> {oldScene.name} => {newScene.name}");
    }

    /// <summary>
    /// ✅ 문으로 다음 씬 넘어가기 직전에 호출하면 “이전 씬 체크포인트 잔상” 방지
    /// </summary>
    public void PrepareForNewScene()
    {
        currentSpawnPoint = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SpawnManager] OnSceneLoaded fired -> scene={scene.name}, mode={mode}");

        // 새 씬 시작은 기본 스폰이 기준
        currentSpawnPoint = null;
        StartCoroutine(CoResolveSpawnThenTeleport(scene.name));
    }

    private IEnumerator CoResolveSpawnThenTeleport(string sceneName)
    {
        yield return null;
        yield return null;

        // SpawnPoint 확정
        var sp = Object.FindFirstObjectByType<SpawnPoint>(FindObjectsInactive.Include);
        Debug.Log($"[SpawnManager] SpawnPoint found? {(sp != null)} in scene={sceneName}");
        if (sp != null) currentSpawnPoint = sp.transform;

        // ✅ PlayerSingleton.Tr 준비될 때까지 최대 30프레임 대기
        int wait = 0;
        while (PlayerSingleton.Tr == null && wait < 30)
        {
            wait++;
            yield return null;
        }

        // ✅ 그래도 없으면 태그로 직접 찾기 (플레이어 태그 꼭 "Player")
        if (PlayerSingleton.Tr == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) PlayerSingleton.Tr = go.transform;
        }

        if (PlayerSingleton.Tr == null)
        {
            Debug.LogWarning($"[SpawnManager] Player still NULL after wait+find. scene={sceneName}");
            yield break;
        }

        Vector3 target = GetSpawnPosition();

        var rb = PlayerSingleton.Tr.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = target;
        }
        else
        {
            PlayerSingleton.Tr.position = target;
        }

        Debug.Log($"[SpawnManager] Teleport DONE -> {target}, spawn={(currentSpawnPoint ? currentSpawnPoint.name : "NULL")}, scene={sceneName}");
    }


    public void SetDefaultSpawn(Transform spawnPoint)
    {
        if (currentSpawnPoint == null)
            currentSpawnPoint = spawnPoint;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentSpawnPoint = checkpoint;
    }

    public Vector3 GetSpawnPosition()
    {
        // SpawnPoint 못 찾으면 “현재 위치 유지”로 두는 게 안전 (0,0 점프 방지)
        if (currentSpawnPoint != null) return currentSpawnPoint.position;
        if (PlayerSingleton.Tr != null) return PlayerSingleton.Tr.position;
        return Vector3.zero;
    }

    /// <summary>
    /// ✅ PlayerLife.cs가 호출하는 Respawn 시그니처와 맞춤
    /// </summary>
    public void Respawn(PlayerLife player)
    {
        if (player == null) return;

        // 플레이어 콜라이더 잠깐 꺼서 즉시 재충돌 방지
        Collider2D pc = player.GetComponent<Collider2D>();
        if (pc) pc.enabled = false;

        // 스폰 위치로 이동
        Vector3 target = GetSpawnPosition();

        Rigidbody2D prb = player.GetComponent<Rigidbody2D>();
        if (prb != null)
        {
            prb.velocity = Vector2.zero;
            prb.angularVelocity = 0f;
            prb.position = target;
        }
        else
        {
            player.transform.position = target;
        }

        // 체력 리셋
        player.ResetLifeToOne();

        // 콜라이더 다시 켜기
        if (pc) pc.enabled = true;
    }
}
