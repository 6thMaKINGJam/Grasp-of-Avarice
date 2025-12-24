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

    [Header("Platform")]
    [SerializeField] private LayerMask platformMask; // Platform 레이어 설정
    [SerializeField, Range(0.01f, 0.5f)] private float platformCheckDistance = 0.15f;
    private Coroutine _ignorePlatformCoroutine;

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

    private Vector2 _nextDirection;
    private Vector2 _currentVelocity;

    private bool _isGround;
    private bool _isClimbing;
    private bool _canClimb;

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

        // ========== 플랫폼 관통 ==========
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S키 눌림!");
            bool onPlatform = IsOnPlatform();
            Debug.Log($"플랫폼 위 여부: {onPlatform}, 코루틴 실행중: {_ignorePlatformCoroutine != null}");
            
            if (onPlatform && _ignorePlatformCoroutine == null)
            {
                Debug.Log("플랫폼 통과 코루틴 시작!");
                _ignorePlatformCoroutine = StartCoroutine(IgnorePlatformCollision());
            }
        }
    }

    /// <summary>
    /// 현재 플랫폼 위에 있는지 체크
    /// </summary>
    private bool IsOnPlatform()
    {
        Bounds b = _collider.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y);
        Vector2 size = new Vector2(b.size.x * 0.8f, 0.1f);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, platformCheckDistance, platformMask);
        
        Debug.Log($"IsOnPlatform 체크: hit={hit.collider != null}, layer={platformMask.value}");
        if (hit.collider != null)
        {
            Debug.Log($"감지된 오브젝트: {hit.collider.gameObject.name}, 레이어: {hit.collider.gameObject.layer}");
        }
        
        return hit.collider != null;
    }

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

    private void HandleClimbingMovement()
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

        CheckLadderExit();
    }

    private void CheckLadderExit()
    {
        if (Mathf.Abs(_nextDirection.x) > 0.7f)
        {
            ExitLadder(false);
            return;
        }

        if (_currentLadder != null && _nextDirection.y > 0)
        {
            float ladderTop = _currentLadder.bounds.max.y;
            float playerBottom = _collider.bounds.min.y; // 발 위치 기준
            
            if (playerBottom > ladderTop) 
            {
                ExitLadder(false);
            }
        }

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

    private void TryStartClimbing()
    {
        if (_canClimb && _currentLadder != null)
        {
            if (Mathf.Abs(_nextDirection.y) > climbInputThreshold)
            {
                StartClimbing();
            }
        }
    }

    private void StartClimbing()
    {
        _isClimbing = true;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.gravityScale = 0f;
        
        if (_animator != null)
            _animator.SetBool("IsClimbing", true);
        
        Debug.Log("사다리 등반 시작");
    }

    private void ExitLadder(bool withJump)
    {
        _isClimbing = false;
        _rigidBody.gravityScale = gravity;
        
        if (_animator != null)
            _animator.SetBool("IsClimbing", false);

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

    public void Move(Vector2 direction)
    {
        _nextDirection = direction;

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

        // Ground와 Platform 모두 체크
        LayerMask combinedMask = groundMask | platformMask;
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDistance, combinedMask);
        
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

    // ==================== 플랫폼 충돌 처리 ====================

    private IEnumerator IgnorePlatformCollision()
    {
        if (_collider == null)
        {
            _ignorePlatformCoroutine = null;
            yield break;
        }

        // 1. 발 아래 플랫폼 감지 (범위를 살짝 더 넓게 잡음)
        Bounds b = _collider.bounds;
        Vector2 checkPos = new Vector2(b.center.x, b.min.y - 0.1f);
        Vector2 checkSize = new Vector2(b.size.x * 0.9f, 0.2f);
        
        Collider2D[] platforms = Physics2D.OverlapBoxAll(checkPos, checkSize, 0f, platformMask);
        
        if (platforms.Length == 0)
        {
            Debug.Log("플랫폼을 찾을 수 없음");
            _ignorePlatformCoroutine = null;
            yield break;
        }

        // 2. 모든 감지된 플랫폼과 충돌 무시
        foreach (var platform in platforms)
        {
            if (platform != null)
                Physics2D.IgnoreCollision(_collider, platform, true);
        }

        // 3. 핵심 수정: 충돌 무시 직후 캐릭터를 아주 살짝 아래로 밀어주거나 잠시 대기
        // 이렇게 해야 발이 플랫폼에 걸려서 바로 '통과 완료'라고 판단하는 것을 방지합니다.
        yield return new WaitForSeconds(0.5f); 

        // 4. 플레이어가 플랫폼 아래로 통과할 때까지 대기
        float timeout = 1.0f;
        float elapsed = 0.2f; // 위에서 대기한 시간 포함

        while (elapsed < timeout)
        {
            bool stillOverlapping = false;
            
            foreach (var platform in platforms)
            {
                if (platform == null) continue;

                float playerBottom = _collider.bounds.min.y;
                float platformTop = platform.bounds.max.y;

                // 캐릭터의 발이 플랫폼의 윗부분보다 아래에 있지 않다면 아직 통과 중
                if (playerBottom > platformTop - 0.2f) 
                {
                    stillOverlapping = true;
                    break;
                }
            }

            if (!stillOverlapping)
            {
                Debug.Log("플랫폼 통과 완료");
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5. 충돌 복구
        foreach (var platform in platforms)
        {
            if (_collider != null && platform != null)
            {
                Physics2D.IgnoreCollision(_collider, platform, false);
            }
        }

        Debug.Log("플랫폼 충돌 복구 완료");
        _ignorePlatformCoroutine = null;
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