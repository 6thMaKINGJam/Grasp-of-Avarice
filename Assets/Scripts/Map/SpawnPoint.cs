using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Start()
    {
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SetDefaultSpawn(transform);
    }
}
