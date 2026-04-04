using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

[
    RequireComponent(typeof(Rigidbody2D)),
    RequireComponent(typeof(SpriteRenderer)),
    RequireComponent(typeof(Health)),
    RequireComponent(typeof(MeleeAttack))
]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;


    private Rigidbody2D rb;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 hitCenter = (Vector2)transform.position + facingDirection * 0.8f;
        Gizmos.DrawWireSphere(hitCenter, 0.4f);
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
        if (moveInput.sqrMagnitude > 0.01f)
        {
            facingDirection = moveInput.normalized;
        }
        spriteRenderer.flipX = facingDirection.x < 0;
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
