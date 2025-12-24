using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(CharacterMovement))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private CharacterMovement _movement;
    private SpriteRenderer _sprite;
    
    public Vector2 MoveInput { get; private set; }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _movement = GetComponent<CharacterMovement>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    // --------------------------- Move ---------------------------
    public void OnMove(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();

        // 스프라이트 플립 (수평 이동 시)
        if (_sprite != null && Mathf.Abs(MoveInput.x) > 0.01f)
        {
            _sprite.flipX = MoveInput.x < 0;
        }

        // CharacterMovement로 전달
        // 등반 중이든 아니든 상관없이 모든 입력을 전달
        _movement.Move(MoveInput);
    }

    // --------------------------- Jump ---------------------------
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // 등반 중이면 CharacterMovement가 알아서 사다리에서 벗어남
        AudioManager.Instance?.PlaySfx(SfxType.Jump);
        _movement.Jump();
    }

    // --------------------------- Pick ---------------------------
    public void OnPick(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // TODO: 소지품 줍기 (E버튼)
        Debug.Log("[Input] Pick");
    }

    // --------------------------- Drop ---------------------------
    public void OnDrop(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // TODO: 소지품 버리기 (Q버튼)
        Debug.Log("[Input] Drop");
    }
}