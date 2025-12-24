using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class CharacterMovement : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private SpriteRenderer _sprite;
    private Collider2D _currentLadder;
    private Animator _animator;

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
    
    [Tooltip("사다리 시작/종료 시 필요한 수직 입력 임계값")]
    [SerializeField, Range(0.1f, 0.9f)] private float climbInputThreshold = 0.3f;
    
    [Tooltip("사다리 벗어날 때 주는 수평 속도")]
    [SerializeField] private float ladderExitHorizontalBoost = 3f;

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
    private bool _canClimb;  // 사다리 범위 안에 있는지

    // Box 통과 처리 관련
    private Collider2D _standingBoxCollider;
    private Coroutine _ignoreBoxCoroutine;

    public UICollider[] uiSensors;

    [Header("Landing SFX")]
    [Tooltip("착지 직전 y속도가 이 값 이하면 Land SFX 재생")]
    [SerializeField] private float hardLandingVelocity = -8f;

    private float _lastYVelocity;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _rigidBody.gravityScale = gravity;
    }

    private void FixedUpdate()
    {
        _lastYVelocity = _rigidBody.velocity.y;

        if (IsAnySensorBlocked())
        {
            _rigidBody.velocity = Vector2.zero;
            return;
        }

        UpdateJumpState();
        _isGround = CheckIsGround();

        // 점프 처리
        if (jump && _isGround)
        {
            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, defaultJumpPower);
            jump = false;
        }
        else if (stopJump)
        {
            stopJump = false;
            if (_rigidBody.velocity.y > 0)
                _rigidBody.velocity *= new Vector2(1f, jumpDeceleration);
        }

        // 등반 모드
        if (_isClimbing)
        {
            HandleClimbingMovement();
            return;
        }

        // 일반 이동 모드
        HandleNormalMovement();
    }

    private void Update()
    {
        // 점프 입력
        if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
        {
            // 사다리 등반 중이면 점프로 사다리에서 벗어남
            if (_isClimbing)
            {
                ExitLadder(true);
            }
            else
            {
                jumpState = JumpState.PrepareToJump;
            }
        }

        if (Input.GetButtonUp("Jump"))
            stopJump = true;

        // 박스 관통
        if (_standingBoxCollider != null && Input.GetKeyDown(KeyCode.S))
        {
            if (_ignoreBoxCoroutine == null)
                _ignoreBoxCoroutine = StartCoroutine(IgnoreBoxCollision(_standingBoxCollider));
        }
    }

    /// <summary>
    /// 일반 이동 처리
    /// </summary>
    private void HandleNormalMovement()
    {
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

    /// <summary>
    /// 등반 이동 처리
    /// </summary>
    private void HandleClimbingMovement()
    {
        _rigidBody.gravityScale = 0f;

        // X축 자동 정렬
        float xVel = 0f;
        if (_currentLadder != null)
        {
            float centerX = _currentLadder.bounds.center.x;
            float offset = centerX - _rigidBody.position.x;
            float desiredXVel = Mathf.Clamp(offset * ladderAlignStrength, -ladderMaxAlignSpeed, ladderMaxAlignSpeed);
            xVel = Mathf.MoveTowards(_rigidBody.velocity.x, desiredXVel, ladderAlignAccel * Time.fixedDeltaTime);
        }

        // Y축 등반
        float yVel = _nextDirection.y * climbSpeed;
        _rigidBody.velocity = new Vector2(xVel, yVel);

        // 사다리에서 벗어나기 체크
        CheckLadderExit();
    }

    /// <summary>
    /// 사다리 벗어나기 체크
    /// </summary>
    private void CheckLadderExit()
    {
        // 1. 수평 방향으로 강하게 입력하면 사다리에서 벗어남
        if (Mathf.Abs(_nextDirection.x) > 0.7f)
        {
            ExitLadder(false);
            return;
        }

        // 2. 사다리 꼭대기에 도달했는지 체크
        if (_currentLadder != null && _nextDirection.y > 0)
        {
            float ladderTop = _currentLadder.bounds.max.y;
            float playerCenter = _rigidBody.position.y;
            
            // 플레이어가 사다리 꼭대기를 넘어가면 자동으로 종료
            if (playerCenter > ladderTop - 0.5f)
            {
                ExitLadder(false);
            }
        }

        // 3. 사다리 바닥에서 아래로 내려가면 종료
        if (_currentLadder != null && _nextDirection.y < -0.1f)
        {
            float ladderBottom = _currentLadder.bounds.min.y;
            float playerBottom = _collider.bounds.min.y;
            
            if (playerBottom < ladderBottom + 0.2f && _isGround)
            {
                ExitLadder(false);
            }
        }
    }

    /// <summary>
    /// 사다리 등반 시작 시도
    /// </summary>
    private void TryStartClimbing()
    {
        // 사다리 범위 안에 있고, 수직 입력이 있을 때만
        if (_canClimb && _currentLadder != null)
        {
            if (Mathf.Abs(_nextDirection.y) > climbInputThreshold)
            {
                StartClimbing();
            }
        }
    }

    /// <summary>
    /// 사다리 등반 시작
    /// </summary>
    private void StartClimbing()
    {
        _isClimbing = true;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.gravityScale = 0f;
        
        if (_animator != null)
            _animator.SetBool("IsClimbing", true);
        
        Debug.Log("사다리 등반 시작");
    }

    /// <summary>
    /// 사다리에서 벗어남
    /// </summary>
    /// <param name="withJump">점프로 벗어났는지</param>
    private void ExitLadder(bool withJump)
    {
        _isClimbing = false;
        _rigidBody.gravityScale = gravity;
        
        if (_animator != null)
            _animator.SetBool("IsClimbing", false);

        // 수평 방향으로 약간의 속도 부여 (더 자연스러운 이탈)
        if (!withJump && Mathf.Abs(_nextDirection.x) > 0.1f)
        {
            _rigidBody.velocity = new Vector2(
                _nextDirection.x * ladderExitHorizontalBoost,
                _rigidBody.velocity.y
            );
        }
        
        Debug.Log("사다리에서 벗어남");
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
                {
                    if (!_isClimbing && _lastYVelocity <= hardLandingVelocity)
                    {
                        AudioManager.Instance?.PlaySfx(SfxType.Land);
                    }
                    jumpState = JumpState.Landed;
                }
                break;

            case JumpState.Landed:
                jumpState = JumpState.Grounded;
                break;
        }
    }

    /// <summary>
    /// 외부에서 이동 입력 (PlayerController에서 호출)
    /// </summary>
    public void Move(Vector2 direction)
    {
        _nextDirection = direction;

        // 사다리 등반 시작 체크
        if (!_isClimbing)
        {
            TryStartClimbing();
        }
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

    public bool IsClimbing() => _isClimbing;

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
        return hit.collider != null;
    }

    // ==================== 사다리 트리거 이벤트 ====================
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            _currentLadder = other;
            _canClimb = true;
            Debug.Log("사다리 범위 진입 (등반 가능)");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder") && _currentLadder == other)
        {
            _currentLadder = null;
            _canClimb = false;
            
            // 사다리 트리거를 벗어나면 등반 종료
            if (_isClimbing)
            {
                ExitLadder(false);
            }
            
            Debug.Log("사다리 범위 벗어남");
        }
    }

    // ==================== 박스 충돌 처리 ====================

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Box")) return;

        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                _standingBoxCollider = collision.collider;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Box"))
        {
            if (collision.collider == _standingBoxCollider)
                _standingBoxCollider = null;
        }
    }

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

            if (playerBottom < boxBottom - 0.01f)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_collider != null && boxCollider != null)
            Physics2D.IgnoreCollision(_collider, boxCollider, false);

        _ignoreBoxCoroutine = null;
    }

    bool IsAnySensorBlocked()
    {
        if (uiSensors == null) return false;

        foreach (UICollider sensor in uiSensors)
        {
            if (sensor != null && sensor.isHittingWall)
            {
                return true;
            }
        }
        return false;
    }
}