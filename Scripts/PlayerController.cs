using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public enum FacingDirection
{
    DOWN = 0,
    UP = 1,
    SIDE = 2
}

[
    RequireComponent(typeof(Rigidbody2D)),
    RequireComponent(typeof(SpriteRenderer)),
    RequireComponent(typeof(Animator)),
    RequireComponent(typeof(Health)),
    RequireComponent(typeof(MeleeAttack))
]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Health health;
    private MeleeAttack meleeAttack;
    private InputSystem inputSystem;

    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.down;
    private bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<Health>();
        meleeAttack = GetComponent<MeleeAttack>();
        inputSystem = new InputSystem();
    }

    void OnEnable()
    {
        inputSystem.Enable();
        inputSystem.Player.Attack.performed += OnAttack;
        health.OnDeath += OnDeath;
    }

    void OnDisable()
    {
        inputSystem.Disable();
        inputSystem.Player.Attack.performed -= OnAttack;
        health.OnDeath -= OnDeath;
    }

    void Update()
    {
        moveInput = inputSystem.Player.Move.ReadValue<Vector2>();
        UpdateFacing();
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateFacing()
    {
        if (moveInput.sqrMagnitude < 0.01f) return;

        Vector2 snapped = Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y) ?
        new Vector2(Mathf.Sign(moveInput.x), 0)
        : new Vector2(0, Mathf.Sign(moveInput.y));

        if (snapped == facingDirection) return;

        facingDirection = snapped;
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        int animationDirection = facingDirection switch
        {
            var d when d == Vector2.down => (int)FacingDirection.DOWN,
            var d when d == Vector2.up => (int)FacingDirection.UP,
            _ => (int)FacingDirection.SIDE
        };
        spriteRenderer.flipX = facingDirection.x < 0;
        animator.SetInteger("direction", animationDirection);
    }

    private void OnAttack(CallbackContext action)
    {
        meleeAttack.Attack(facingDirection);
    }

    private void OnDeath()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
    }

}
