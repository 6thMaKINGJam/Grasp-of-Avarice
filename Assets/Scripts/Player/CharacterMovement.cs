using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class CharacterMovement : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private SpriteRenderer _sprite;
    private Collider2D _currentLadder;

    [Header("Move")]
    [SerializeField, Range(0.0f, 20.0f)] private float speed = 6f;
    public float Speed => speed;

    [Tooltip("Air Acceleration")]
    [SerializeField, Range(0.0f, 100.0f)] private float acceleration = 40f;

    [Tooltip("Air Deceleration")]
    [SerializeField, Range(0.0f, 100.0f)] private float deceleration = 30f;

    [Header("Jump")]
    [SerializeField, Range(0.0f, 30.0f)] private float defaultJumpPower = 8f;

    [Header("Gravity")]
    [SerializeField, Range(0.0f, 10.0f)] private float gravity = 1f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField, Range(0.01f, 1.0f)] private float groundCheckDistance = 0.25f;

    [Header("Climb")]
    [SerializeField, Range(0.0f, 20.0f)] private float climbSpeed = 4f;
    [SerializeField] private float ladderAlignStrength = 8f;  
    [SerializeField] private float ladderMaxAlignSpeed = 3f; 
    [SerializeField] private float ladderAlignAccel = 30f;

    private Vector2 _nextDirection;     // 인풋 방향
    private Vector2 _currentVelocity;   // 이동 속도

    private bool _isGround;
    private bool _isClimbing;       


    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _sprite = GetComponent<SpriteRenderer>();

        _rigidBody.gravityScale = gravity;
    }

    private void FixedUpdate()
    {
        _isGround = CheckIsGround();

        if (_isClimbing)
        {
            _rigidBody.gravityScale = 0f;

            float xVel = 0f;

            if (_currentLadder != null)
            {
                float centerX = _currentLadder.bounds.center.x;
                float offset = centerX - _rigidBody.position.x;
                float desiredXVel = Mathf.Clamp(offset * ladderAlignStrength, -ladderMaxAlignSpeed, ladderMaxAlignSpeed);
                xVel = Mathf.MoveTowards(_rigidBody.velocity.x, desiredXVel, ladderAlignAccel * Time.fixedDeltaTime);
            }
            float yVel = _nextDirection.y * climbSpeed;
            _rigidBody.velocity = new Vector2(xVel, yVel);
            return;
        }
        _rigidBody.gravityScale = gravity;
        float targetX = _nextDirection.x * speed;
        float newX;

        if (_isGround)
        {
            newX = targetX;
        }
        else
        {
            if (Mathf.Abs(_nextDirection.x) > 0.01f)
                newX = Mathf.MoveTowards(_rigidBody.velocity.x, targetX, acceleration * Time.fixedDeltaTime);
            else
                newX = Mathf.MoveTowards(_rigidBody.velocity.x, 0f, deceleration * Time.fixedDeltaTime);
        }
        _rigidBody.velocity = new Vector2(newX, _rigidBody.velocity.y);
    }

    public void Move(Vector2 direction)
    {
        _nextDirection = direction;
    }

    public void Jump()
    {
        Jump(defaultJumpPower);
    }

    public void Jump(float jumpPower)
    {
        if (_isClimbing) return;

        if (!CheckIsGround()) return;

        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, 0f);
        _rigidBody.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    public void ClimbState()
    {
        _isClimbing = true;
        _nextDirection = Vector2.zero;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.gravityScale = 0f;
    }

    public void EndClimbState()
    {
        _isClimbing = false;
        _rigidBody.gravityScale = gravity;
    }

    public bool IsClimbing() => _isClimbing;

    /// <summary>플립용: 스프라이트가 바라보는 x방향</summary>
    public Vector2 GetCharacterSpriteDirection()
    {
        return new Vector2(_sprite.flipX ? -1 : 1, 0);
    }

    public bool CheckIsGround()
    {
        Bounds b = _collider.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + 0.02f);
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.06f);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDistance, groundMask);
        Debug.DrawLine(origin, origin + Vector2.down * groundCheckDistance, Color.red);

        return hit.collider != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            _currentLadder = other;
            ClimbState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            if (_currentLadder == other) _currentLadder = null;
            EndClimbState();
        }
    }
}
