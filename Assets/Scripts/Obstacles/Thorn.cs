using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D triggerZone; // 플레이어 감지용(Trigger)

    [Header("Settings")]
    [SerializeField] private float fallGravity = 6f;
    [SerializeField] private bool destroyOnGround = false;

    private Vector3 startPos;
    private bool activated;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        // 시작은 고정(안 떨어지게)
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // triggerZone이 따로 있으면: triggerZone에 이 스크립트를 붙이거나,
        // 여기서 other가 플레이어인지 확인해도 됨.
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        ActivateFall();
    }

    private void ActivateFall()
    {
        activated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravity;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!activated) return;

        // 땅에 닿았을 때 처리
        if (col.collider.CompareTag("Ground"))
        {
            if (destroyOnGround) Destroy(gameObject);
            else
            {
                // 멈추기
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    public void ResetSpike()
    {
        activated = false;
        transform.position = startPos;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }
}
