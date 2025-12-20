using System.Collections;
using UnityEngine;

public class Thorn : MonoBehaviour, IResettable
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D triggerZone; 

    [Header("Settings")]
    [SerializeField] private float fallGravity = 6f;

    [Header("Detect")]
    [SerializeField] private LayerMask playerLayer; 

    private Vector3 startPos;
    private Quaternion startRot;

    private bool activated;
    private bool armed; 

    private readonly Collider2D[] _overlapBuf = new Collider2D[2];

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (triggerZone == null) triggerZone = GetComponent<Collider2D>();

        startPos = transform.position;
        startRot = transform.rotation;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;

        StartCoroutine(ArmWhenClear());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed) return;
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

    public void ResetSpike()
    {
        activated = false;

        transform.position = startPos;
        transform.rotation = startRot;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

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

    public void ResetState() => ResetSpike();
}
