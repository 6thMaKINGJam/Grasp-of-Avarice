using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var spawn = Object.FindFirstObjectByType<SpawnPoint>();
        if (spawn == null) return;

        // 위치 이동 (2D면 z값 유지하고 싶으면 아래처럼)
        Vector3 p = spawn.transform.position;
        p.z = player.transform.position.z;
        player.transform.position = p;
    }
}
