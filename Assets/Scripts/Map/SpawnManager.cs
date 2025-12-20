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

    public void PrepareForNewScene()
    {
        currentSpawnPoint = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SpawnManager] OnSceneLoaded fired -> scene={scene.name}, mode={mode}");

        currentSpawnPoint = null;
        StartCoroutine(CoResolveSpawnThenTeleport(scene.name));
    }

    private IEnumerator CoResolveSpawnThenTeleport(string sceneName)
    {
        yield return null;
        yield return null;

        var sp = Object.FindFirstObjectByType<SpawnPoint>(FindObjectsInactive.Include);
        Debug.Log($"[SpawnManager] SpawnPoint found? {(sp != null)} in scene={sceneName}");
        if (sp != null) currentSpawnPoint = sp.transform;

        int wait = 0;
        while (PlayerSingleton.Tr == null && wait < 30)
        {
            wait++;
            yield return null;
        }

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
        if (currentSpawnPoint != null) return currentSpawnPoint.position;
        if (PlayerSingleton.Tr != null) return PlayerSingleton.Tr.position;
        return Vector3.zero;
    }

    public void Respawn(PlayerLife player)
    {
        if (player == null) return;
        StartCoroutine(CoRespawn(player));
    }

    private IEnumerator CoRespawn(PlayerLife player)
    {
        // 0) 콜라이더 잠깐 꺼서 "리스폰 즉시 재충돌/즉사" 방지
        Collider2D pc = player.GetComponent<Collider2D>();
        if (pc) pc.enabled = false;

        // 1) 스폰 위치로 이동 + 속도 초기화
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

        // 2) 체력 리셋
        player.ResetLifeToOne();

        // 3) 물리 한 프레임 정리
        yield return new WaitForFixedUpdate();

        // 4) 콜라이더 켜기 (이제 함정들이 "플레이어 겹침"을 감지할 수 있음)
        if (pc) pc.enabled = true;

        // 5) 물리 한 프레임 더 진행 후 함정 리셋 (ArmWhenClear 안정화)
        yield return new WaitForFixedUpdate();

        // 6) 모든 장애물/함정 Reset
        var monos = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var m in monos)
        {
            if (m is IResettable r)
                r.ResetState();
        }
    }
}
