# Plan 01 — Player Foundation

Systems: PlayerController · Camera · MeleeAttack · HealthSystem  
Engine: Unity 6.3 LTS · 2D Top-Down Pixel Art  
Goal: A player you can move, face, attack with, and take damage.

---

## Unity Project Setup

- **Template:** 2D (Core) — not URP unless you want shader-based lighting later; can upgrade
- **Packages to install first:**
  - Cinemachine 3.x (via Package Manager)
  - Input System (via Package Manager — enable new Input System backend, restart when prompted)
- **Physics settings:** Edit → Project Settings → Physics 2D
  - Set gravity to `(0, 0)` — top-down, no gravity
- **Layer setup (create these now):**
  - `Player`
  - `Enemy`
  - `Interactable`
  - `Wall`
- **Tag:** `Player`

---

## 1. HealthSystem

Start here — it's a pure data component with no dependencies. Both the player and enemies will use it.

### Component: `HealthSystem.cs`

Attach to: any GameObject that can take damage (Player, Feral, etc.)

```
Fields:
  [SerializeField] int maxHealth = 100
  int currentHealth  (private)

Properties:
  int CurrentHealth  (get only)
  bool IsAlive → currentHealth > 0

Events:
  Action<int, int> OnHealthChanged   // (current, max) — for UI
  Action OnDeath

Public methods:
  void TakeDamage(int amount)
  void Heal(int amount)
  void Initialize(int max)           // call on Start if you want runtime override
```

**TakeDamage logic:**
```
currentHealth = Mathf.Max(0, currentHealth - amount)
OnHealthChanged?.Invoke(currentHealth, maxHealth)
if (currentHealth == 0) OnDeath?.Invoke()
```

**Notes:**
- Keep this dumb — no death behavior here. Subscribe to `OnDeath` from other components (PlayerController disables input, enemy triggers death anim, etc.)
- `Heal` clamps to `maxHealth`
- No knockback, no invincibility frames here — those belong on the receiver

---

## 2. PlayerController

### GameObject setup

```
PlayerRoot (GameObject)
├── Rigidbody2D
│     ├── Body Type: Dynamic
│     ├── Collision Detection: Continuous
│     ├── Freeze Rotation Z: ✓
│     └── Gravity Scale: 0
├── CapsuleCollider2D  (or CircleCollider2D — circle is simpler for top-down)
├── SpriteRenderer
├── PlayerController.cs
├── MeleeAttack.cs      (added in section 3)
└── HealthSystem.cs
```

Use a `CircleCollider2D` to start — avoids corner-catching on tile edges.

### Input Action Asset

Create `PlayerInputActions.inputactions`:

```
ActionMap: Player
  Move      → Value, Vector2 → WASD + Left Stick
  Attack    → Button → Left Click + Gamepad West
```

Generate C# class from the asset (checkbox in inspector). This gives you `PlayerInputActions` you can `new` up directly.

### Component: `PlayerController.cs`

```
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]

Fields:
  [SerializeField] float moveSpeed = 5f

Private:
  Rigidbody2D rb
  Vector2 moveInput
  Vector2 facingDirection   ← normalized, defaults to (1,0)
  PlayerInputActions input
  bool canMove = true       ← set false on death / dialogue

Lifecycle:
  Awake  → rb = GetComponent, input = new PlayerInputActions()
  OnEnable  → input.Player.Enable()
  OnDisable → input.Player.Disable()
  Start  → GetComponent<HealthSystem>().OnDeath += OnDeath

Update:
  moveInput = input.Player.Move.ReadValue<Vector2>()
  UpdateFacing()

FixedUpdate:
  if (!canMove) return
  rb.linearVelocity = moveInput * moveSpeed

UpdateFacing():
  if (moveInput.sqrMagnitude > 0.01f)
    facingDirection = moveInput.normalized
  // Flip sprite based on facingDirection.x
  spriteRenderer.flipX = facingDirection.x < 0

OnDeath():
  canMove = false
  rb.linearVelocity = Vector2.zero
  // Trigger death animation or disable — expand later
```

**facingDirection** is the key shared value — `MeleeAttack` reads it to place the hitbox.

---

## 3. MeleeAttack

### What it does
On attack input: wait for the "active frame" (short delay simulating windup), spawn a hitbox in `facingDirection`, deal damage to anything with `HealthSystem` in range, enforce a cooldown.

No animation events yet — use a coroutine for timing. Swap in animation events later when you have sprites.

### Component: `MeleeAttack.cs`

Attach to: Player (same GameObject as PlayerController)

```
Fields:
  [SerializeField] float attackRange = 0.8f
  [SerializeField] int attackDamage = 20
  [SerializeField] float attackCooldown = 0.5f
  [SerializeField] float activeFrameDelay = 0.1f    // windup before hit lands
  [SerializeField] LayerMask hitLayers              // set to Enemy layer

Private:
  PlayerController playerController
  PlayerInputActions input
  bool isOnCooldown = false

Awake:
  playerController = GetComponent<PlayerController>()
  input = new PlayerInputActions()

OnEnable / OnDisable:
  input.Player.Attack.performed += OnAttackInput  /  -= 

OnAttackInput(ctx):
  if (isOnCooldown) return
  StartCoroutine(AttackCoroutine())

AttackCoroutine():
  isOnCooldown = true
  yield return new WaitForSeconds(activeFrameDelay)
  PerformHit()
  yield return new WaitForSeconds(attackCooldown - activeFrameDelay)
  isOnCooldown = false

PerformHit():
  Vector2 hitCenter = (Vector2)transform.position + playerController.FacingDirection * attackRange
  Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, 0.4f, hitLayers)
  foreach hit in hits:
    hit.GetComponent<HealthSystem>()?.TakeDamage(attackDamage)
```

**Notes:**
- `FacingDirection` needs to be `public` or `internal` on `PlayerController`
- `Physics2D.OverlapCircleAll` — the 0.4f radius is the hitbox size; tune to feel
- `hitLayers` in the Inspector: assign the Enemy layer only (avoids self-hit)
- You can visualize the hitbox in `OnDrawGizmosSelected` with `Gizmos.DrawWireSphere`

### Gizmo (add to MeleeAttack for editor debugging)

```csharp
void OnDrawGizmosSelected()
{
    if (playerController == null) return;
    Gizmos.color = Color.red;
    Vector2 hitCenter = (Vector2)transform.position + playerController.FacingDirection * attackRange;
    Gizmos.DrawWireSphere(hitCenter, 0.4f);
}
```

---

## 4. Camera

Unity 6.3 uses Cinemachine 3.x — the API changed from 2.x. Use the new component names.

### Setup

1. Main Camera: leave as-is, remove AudioListener if you add a separate one
2. Add a **CinemachineCamera** GameObject (GameObject → Cinemachine → Cinemachine Camera)
3. On the CinemachineCamera:
   - **Follow:** assign `PlayerRoot`
   - **Look At:** assign `PlayerRoot`
   - Add component: `CinemachinePositionComposer`
     - Damping X / Y: `0.15` — subtle lag, not floaty
     - Dead Zone Width / Height: `0` for now
   - **Lens → Orthographic Size:** start at `5`, tune to taste (smaller = more zoomed in)

4. **Pixel-perfect (optional but recommended for pixel art):**
   - Add `PixelPerfectCamera` to Main Camera
   - Set `Assets Pixels Per Unit` to match your sprites (e.g. 16 or 32)
   - CinemachineCamera works with it automatically

### Camera bounds (for later)
When you have a room built, add `CinemachineConfiner2D` and assign a `PolygonCollider2D` on the room boundary. Skip for now.

---

## Scene Structure

```
Scene: MedicalSector_CryoRoom (first room)

Hierarchy:
  --- Environment ---
  Tilemap_Ground
  Tilemap_Walls
  Tilemap_Details

  --- Entities ---
  Player
    (all player components here)

  --- Camera ---
  CM_Camera (CinemachineCamera)
  Main Camera

  --- Managers ---
  GameManager (empty for now, placeholder)
```

---

## Implementation Order

1. Create project, install packages, set up layers
2. `HealthSystem.cs` — write and unit-test in isolation (create a test scene, button calls TakeDamage)
3. `PlayerController.cs` — get movement working, verify `FacingDirection` updates
4. Placeholder room — 3×3 tiles with wall colliders so the player has something to bump into
5. `MeleeAttack.cs` — add gizmo, verify hitbox placement looks right in editor
6. Wire `OnDeath` on the player — just disable the PlayerController for now
7. Camera — add CinemachineCamera, tune damping

---

## What's Explicitly Not Here

- Animator / sprite sheets — use a colored square placeholder
- Sound — no AudioSource yet
- Enemy — that's Plan 02
- UI health bar — that's Plan 03 (UIManager)
- Dodge roll / stamina — out of scope for MVP

---

## Checklist Before Moving to Plan 02

- [ ] Player moves in 8 directions at consistent speed
- [ ] `FacingDirection` correctly reflects last movement direction
- [ ] Sprite flips horizontally when moving left
- [ ] Attack hitbox gizmo appears in the correct position facing direction
- [ ] `HealthSystem.TakeDamage` fires `OnHealthChanged` and `OnDeath` correctly
- [ ] `OnDeath` stops player movement
- [ ] Camera follows player with slight lag
- [ ] Player collides with wall tiles (does not pass through)
