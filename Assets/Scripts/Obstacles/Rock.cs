using System.Collections;
using UnityEngine;

public class Rock : MonoBehaviour, IResettable
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D triggerZone; // Trigger(플레이어 감지용)

    [Header("Waiting (Before Start)")]
    [SerializeField] private float waitingGravity = 0f;

    [Header("Rolling (After Start)")]
    [SerializeField] private float runGravity = 2f;

    [Header("Detect")]
    [SerializeField] private LayerMask playerLayer; // Player 레이어 지정 추천

    [Header("Option")]
    [SerializeField] private bool resetOnResetZone = true; 

    private bool started;

    private Vector3 startPos;
    private float startRotZ;

    private bool armed; 
    private readonly Collider2D[] _overlapBuf = new Collider2D[2];

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (triggerZone == null) triggerZone = GetComponent<Collider2D>();

        startPos = transform.position;
        startRotZ = transform.eulerAngles.z;

        // 대기 상태
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = waitingGravity;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 시작 무장
        StartCoroutine(ArmWhenClear());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed) return;
        if (started) return;
        if (!other.CompareTag("Player")) return;

        if (triggerZone != null && !triggerZone.IsTouching(other)) return;

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

        armed = false;
        StopAllCoroutines();
        StartCoroutine(ArmWhenClear());
    }

    private bool IsPlayerOverlapping()
    {
        if (triggerZone == null) return false;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = playerLayer;
        filter.useTriggers = true;

        int count = triggerZone.OverlapCollider(filter, _overlapBuf);
        return count > 0;
    }

    private IEnumerator ArmWhenClear()
    {
        yield return new WaitForFixedUpdate();

        while (IsPlayerOverlapping())
            yield return new WaitForFixedUpdate();

        armed = true;
    }

    public void ResetState() => ResetBoulder();
}
