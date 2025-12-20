using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Awake()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.SetDefaultSpawn(transform);
        }
    }

}
