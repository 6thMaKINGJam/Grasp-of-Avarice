using UnityEngine;

public class Rock : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D triggerZone;

    [Header("Waiting (Before Start)")]
    [SerializeField] private float waitingGravity = 0f;

    [Header("Rolling (After Start)")]
    [SerializeField] private float runGravity = 2f;

    [Header("Option")]
    [SerializeField] private bool resetOnResetZone = true;

    private bool started;
    private Vector3 startPos;
    private float startRotZ;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        startPos = transform.position;
        startRotZ = transform.eulerAngles.z;

        // 대기 상태: 고정
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = waitingGravity;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (started) return;
        if (!other.CompareTag("Player")) return;

        StartRolling();
    }

    public void StartRolling()
    {
        if (started) return;
        started = true;

        rb.bodyType = RigidbodyType2D.Dynamic;   
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.position += Vector2.down * 0.05f;   
        rb.gravityScale = runGravity;
    }

    public void ResetBoulder()
    {
        started = false;

        transform.position = startPos;
        transform.rotation = Quaternion.Euler(0, 0, startRotZ);

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = waitingGravity;
    }
}
