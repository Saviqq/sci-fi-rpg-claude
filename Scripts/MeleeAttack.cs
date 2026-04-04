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

    public void Attack(Vector2 direction)
    {
        if (isOnCooldown) return;
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
