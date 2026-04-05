# Plan 02 — Field of View System (Raycast Mesh + Dark Overlay)

Engine: Unity 6.3 LTS · URP · 2D Top-Down  
Goal: A directional visibility cone that rotates with the player. Outside the cone = dark. Walls occlude vision.

---

## How It Works

```
Every frame:
  Cast N rays in a cone arc centered on transform.up
      ↓
  Each ray hits a wall (stop) or reaches max range
      ↓
  Ray endpoints → triangle fan mesh (local space)
      ↓
  Mesh writes stencil = 1 to the GPU stencil buffer
      ↓
  Dark overlay quad renders everywhere stencil ≠ 1
      ↓
  Result: scene visible inside cone, dark outside
```

No custom lighting. No URP 2D lights. Works on any render pipeline.

---

## Files to Create

```
Assets/
├── Shaders/
│   ├── FOVMask.shader        ← marks visible area in stencil buffer (invisible itself)
│   └── DarkOverlay.shader    ← renders darkness everywhere outside stencil mark
├── Materials/
│   ├── FOVMask.mat
│   └── DarkOverlay.mat
└── Scripts/
    └── FieldOfViewSystem.cs  ← raycasting + mesh generation
```

---

## 1. FOVMask.shader

Writes `1` to the stencil buffer where the FOV mesh covers. Draws no color — completely invisible. Renders first (Queue 3000).

```hlsl
Shader "Custom/FOVMask"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            ZWrite Off
            ColorMask 0

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target { return half4(0, 0, 0, 0); }
            ENDHLSL
        }
    }
}
```

---

## 2. DarkOverlay.shader

Covers the entire screen with a dark color — but skips any pixel where stencil = 1 (the visible cone). Renders after FOVMask (Queue 3001).

```hlsl
Shader "Custom/DarkOverlay"
{
    Properties
    {
        _Color ("Overlay Color", Color) = (0, 0, 0, 0.95)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            Stencil
            {
                Ref 1
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target { return _Color; }
            ENDHLSL
        }
    }
}
```

---

## 3. FieldOfViewSystem.cs

Attach to: Player GameObject (same object as PlayerController).

Builds the FOV mesh in local space every frame — it rotates automatically with the player.

```csharp
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewSystem : MonoBehaviour
{
    [Header("Cone Settings")]
    [SerializeField] private float viewRadius = 7f;
    [Range(10f, 360f)]
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private int rayCount = 60;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Atmosphere")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float flickerSpeed = 2f;
    [SerializeField] private float flickerAmount = 0.03f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh fovMesh;

    // Cached arrays — avoids GC allocation every frame
    private Vector3[] vertices;
    private int[] triangles;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        fovMesh = new Mesh { name = "FOV Mesh" };
        meshFilter.mesh = fovMesh;

        // +1 for origin vertex, +1 so final ray closes the arc
        vertices  = new Vector3[rayCount + 2];
        triangles = new int[rayCount * 3];
    }

    void LateUpdate()
    {
        BuildMesh();

        if (enableFlicker)
            ApplyFlicker();
    }

    private void BuildMesh()
    {
        float halfAngle = viewAngle * 0.5f;
        float angleStep = viewAngle / rayCount;

        vertices[0] = Vector3.zero; // cone origin — player center in local space

        for (int i = 0; i <= rayCount; i++)
        {
            // Rotate around Z from -half to +half, centered on transform.up
            float angle = -halfAngle + angleStep * i;
            Vector2 worldDir = Quaternion.Euler(0f, 0f, angle) * transform.up;

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, worldDir, viewRadius, obstacleMask);

            Vector3 worldPoint = hit.collider != null
                ? (Vector3)hit.point
                : transform.position + (Vector3)(worldDir * viewRadius);

            // Store in local space so mesh rotates with the player
            vertices[i + 1] = transform.InverseTransformPoint(worldPoint);
        }

        // Triangle fan: (origin, v_i, v_i+1)
        for (int i = 0; i < rayCount; i++)
        {
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        fovMesh.Clear();
        fovMesh.vertices  = vertices;
        fovMesh.triangles = triangles;
        // No normals needed — unlit shader
    }

    private void ApplyFlicker()
    {
        // Subtle intensity variation via material color alpha
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        Color c = meshRenderer.material.color;
        c.a = 1f + (noise - 0.5f) * flickerAmount;
        meshRenderer.material.color = c;
    }

    // Called by FeralSenseSystem (Plan 04)
    public void Pulse(float radiusBoost, float duration)
    {
        StartCoroutine(PulseCoroutine(radiusBoost, duration));
    }

    private System.Collections.IEnumerator PulseCoroutine(float boost, float duration)
    {
        float original = viewRadius;
        viewRadius = original + boost;
        yield return new WaitForSeconds(duration);
        viewRadius = original;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        float half = viewAngle * 0.5f;
        Vector2 leftEdge  = Quaternion.Euler(0, 0,  half) * transform.up;
        Vector2 rightEdge = Quaternion.Euler(0, 0, -half) * transform.up;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(leftEdge  * viewRadius));
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(rightEdge * viewRadius));
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}
```

---

## 4. Scene Setup

### Create Materials

1. Project → Create → Material → name `FOVMask`
   - Shader: `Custom/FOVMask`
2. Project → Create → Material → name `DarkOverlay`
   - Shader: `Custom/DarkOverlay`
   - `_Color`: `(0, 0, 0, 0.95)` — tune alpha for darkness intensity

### Player — FOV Mesh Renderer

The `FieldOfViewSystem` script needs a `MeshFilter` and `MeshRenderer` to live on the Player. Add both components to the Player GameObject, then:
- MeshRenderer → Material: `FOVMask`

> The script is on the same GameObject as PlayerController. MeshFilter/MeshRenderer are also on the Player. The mesh is in local space so it auto-rotates.

### Dark Overlay Quad

1. Hierarchy → right-click → 3D Object → **Quad** → name `DarkOverlay`
2. Parent it to **Main Camera**
3. Transform:
   - Position: `(0, 0, 5)` — in front of camera on Z (camera looks at -Z, so positive Z is "toward" the scene from camera's view)
   - Rotation: `(0, 0, 0)`
   - Scale: `(60, 60, 1)` — large enough to cover any view
4. Remove the `Mesh Collider` component (unnecessary)
5. MeshRenderer → Material: `DarkOverlay`

### Obstacle Layer for Raycasts

- `obstacleMask` in the Inspector → assign the `Wall` layer
- This tells the FOV raycasts to stop at wall tiles only

---

## Inspector Values (starting point)

| Field | Value |
|-------|-------|
| View Radius | 7 |
| View Angle | 90 |
| Ray Count | 60 |
| Obstacle Mask | Wall |
| Enable Flicker | true |
| Flicker Speed | 2 |
| Flicker Amount | 0.03 |

Tune `View Angle` for how wide the cone feels. 90° is flashlight-tight. 120° is more generous. Never go above 180° without increasing `Ray Count`.

---

## Checklist

- [ ] Dark overlay covers the whole screen (scene is nearly black without the cone)
- [ ] Cone of visibility follows player, rotates with mouse aim
- [ ] Walking a wall into the cone edge: vision is cut off at the wall surface
- [ ] No mesh flickering or gaps between rays (increase Ray Count if visible)
- [ ] Flicker is subtle — barely noticeable, just alive
- [ ] `Pulse()` method exists and compiles (wired to Feral Sense in Plan 04)

---

## If Stencil Doesn't Work (Troubleshooting)

If the dark overlay covers everything including the cone:
- The stencil write may be cleared between render passes in the URP 2D renderer
- Fix: In the URP Renderer asset (Project Settings → Graphics → your URP asset → Renderer), enable **Stencil** support

If the dark overlay is invisible entirely:
- The `DarkOverlay` quad may be behind the camera — change Z position to `-5` (in front of camera when camera looks at -Z)

---

## What's Not Here

- Enemy visibility culling (enemies outside FOV are already darkened by the overlay — no extra code needed)
- Feral Sense pulse (Plan 04 — calls `FieldOfViewSystem.Pulse()`)
- Edge-precision rays at wall corners (optional polish pass later — current uniform distribution is fine for MVP)
