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

    public enum JumpState
    {
        Grounded,
        PrepareToJump,
        Jumping,
        InFlight,
        Landed
    }

    [SerializeField] private float jumpDeceleration = 0.5f;
    private JumpState jumpState = JumpState.Grounded;
    private bool stopJump;
    private bool jump;


    private Vector2 _nextDirection;     // 인풋 방향
    private Vector2 _currentVelocity;   // 이동 속도

    private bool _isGround;
    private bool _isClimbing;

    // Box 통과 처리 관련
    private Collider2D _standingBoxCollider;   // 지금 플레이어 바로 아래에 서 있는 박스의 collider
    private Coroutine _ignoreBoxCoroutine;


    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _sprite = GetComponent<SpriteRenderer>();

        _rigidBody.gravityScale = gravity;
    }

    private void FixedUpdate()
    {
        UpdateJumpState();
        _isGround = CheckIsGround();

        if (jump && _isGround)
        {
            _rigidBody.velocity = new Vector2(
                _rigidBody.velocity.x,
                defaultJumpPower
            );
            jump = false;
        }
        else if (stopJump)
        {
            stopJump = false;
            if (_rigidBody.velocity.y > 0)
                _rigidBody.velocity *= new Vector2(1f, jumpDeceleration);
        }


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

    private void Update()
    {
        _nextDirection.x = Input.GetAxis("Horizontal");

        if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
            jumpState = JumpState.PrepareToJump;

        if (Input.GetButtonUp("Jump"))
            stopJump = true;

        // 박스 위에 서 있을 때 D 누르면 박스와의 충돌을 일시 해제하여 아래로 내려가도록 처리
        if (_standingBoxCollider != null && Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S키 눌림, 박스 충돌 무시 시작");
            if (_ignoreBoxCoroutine == null)
                _ignoreBoxCoroutine = StartCoroutine(IgnoreBoxCollision(_standingBoxCollider));
        }
    }

    private void UpdateJumpState()
    {
        jump = false;

        switch (jumpState)
        {
            case JumpState.PrepareToJump:
                jumpState = JumpState.Jumping;
                jump = true;
                stopJump = false;
                break;

            case JumpState.Jumping:
                if (!_isGround)
                    jumpState = JumpState.InFlight;
                break;

            case JumpState.InFlight:
                if (_isGround)
                    jumpState = JumpState.Landed;
                break;

            case JumpState.Landed:
                jumpState = JumpState.Grounded;
                break;
        }
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

    // 박스 위에 서 있는지 판단: collision의 contact normal을 사용
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Box")) return;

        Debug.Log("Box 위에 서 있음");
        foreach (var contact in collision.contacts)
        {
            // contact.normal 는 충돌 쪽에서 바라본 법선, 플레이어가 위에 있으면 normal.y > 0
            if (contact.normal.y > 0.5f)
            {
                _standingBoxCollider = collision.collider;
                Debug.Log("standingBoxCollider 설정됨");
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Box"))
        {
            Debug.Log("Box에서 벗어남");
            if (collision.collider == _standingBoxCollider)
                _standingBoxCollider = null;
        }
    }

    // 플레이어 콜라이더와 boxCollider 간 충돌을 일시 무시하고,
    // 플레이어가 완전히 박스 아래로 내려가면 충돌을 복구
    private IEnumerator IgnoreBoxCollision(Collider2D boxCollider)
    {
        if (_collider == null || boxCollider == null)
        {
            _ignoreBoxCoroutine = null;
            yield break;
        }

        Physics2D.IgnoreCollision(_collider, boxCollider, true);

        float timeout = 1.5f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (boxCollider == null) break;

            float playerBottom = _collider.bounds.min.y;
            float boxBottom = boxCollider.bounds.min.y;

            // 플레이어가 박스보다 아래로 내려가면 종료
            if (playerBottom < boxBottom - 0.01f)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_collider != null && boxCollider != null)
            Physics2D.IgnoreCollision(_collider, boxCollider, false);

        _ignoreBoxCoroutine = null;
    }
}
