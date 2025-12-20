using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;

        if (SpawnManager.Instance != null)
            SpawnManager.Instance.SetCheckpoint(transform);

        // 필요하면 여기서 체크포인트 이펙트/사운드 호출
        // Debug.Log("Checkpoint Activated");
    }
}
