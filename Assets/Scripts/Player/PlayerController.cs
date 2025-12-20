using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(CharacterMovement))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private CharacterMovement _movement;
    private SpriteRenderer _sprite;
    public Vector2 MoveInput { get; private set; }  
    public Vector2 ClimbInput { get; private set; } 

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

        if (_sprite != null && Mathf.Abs(MoveInput.x) > 0.01f)
            _sprite.flipX = MoveInput.x < 0;

        if (_movement.IsClimbing())
        {
            return;
        }

        _movement.Move(new Vector2(MoveInput.x, 0f));
    }

    // --------------------------- Climb ---------------------------
    public void OnClimb(InputAction.CallbackContext ctx)
    {
        ClimbInput = ctx.ReadValue<Vector2>();

        if (_movement.IsClimbing())
        {
            _movement.Move(new Vector2(0f, ClimbInput.y));
        }
    }

    // --------------------------- Jump ---------------------------
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        _movement.Jump();
    }

    // --------------------------- Pick ---------------------------
    public void OnPick(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // TODO: 소지품 줍기 (E버튼과 연결해둠)
        Debug.Log("[Input] Pick");
    }

    // --------------------------- Drop ---------------------------
    public void OnDrop(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // TODO: 소지품 버리기 (Q버튼과 연결해둠)
        Debug.Log("[Input] Drop");
    }
}
