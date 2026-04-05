using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[
    RequireComponent(typeof(Rigidbody2D)),
    RequireComponent(typeof(Health)),
    RequireComponent(typeof(MeleeAttack))
]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    private Camera mainCamera;
    private Rigidbody2D rb;
    private Health health;
    private MeleeAttack meleeAttack;
    private InputSystem inputSystem;

    private Vector2 moveInput;
    private bool canMove = true;

    void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
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
        UpdateRotation();
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateRotation()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = mouseWorld - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnAttack(CallbackContext action)
    {
        meleeAttack.Attack();
    }

    private void OnDeath()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
    }

}
