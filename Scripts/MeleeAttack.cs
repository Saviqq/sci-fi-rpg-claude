using System.Collections;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private float activeFrameDelay;
    [SerializeField] private LayerMask hitLayers;

    [Header("Values")]
    [SerializeField] private float range;
    [SerializeField] private int damage;
    [SerializeField] private float cooldown;

    private bool isOnCooldown = false;
    private Vector2 lastDirection = Vector2.down; // Cached for gizmo

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 hitCenter = (Vector2)transform.position + lastDirection * 0.8f;
        Gizmos.DrawWireSphere(hitCenter, 0.4f);
    }

    public void Attack(Vector2 direction)
    {
        if (isOnCooldown) return;
        lastDirection = direction;
        StartCoroutine(AttackCoroutine(direction));
    }

    private IEnumerator AttackCoroutine(Vector2 direction)
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(activeFrameDelay);
        PerformHit(direction);
        yield return new WaitForSeconds(cooldown - activeFrameDelay);
        isOnCooldown = false;
    }

    private void PerformHit(Vector2 direction)
    {
        Vector2 hitCenter = (Vector2)transform.position + direction * range;
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, 0.4f, hitLayers);
        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<Health>()?.TakeDamage(damage);
        }
    }
}
