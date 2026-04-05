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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 hitCenter = (Vector2)transform.position + (Vector2)transform.up * 0.8f;
        Gizmos.DrawWireSphere(hitCenter, 0.4f);
    }

    public void Attack()
    {
        if (isOnCooldown) return;
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(activeFrameDelay);
        PerformHit();
        yield return new WaitForSeconds(cooldown - activeFrameDelay);
        isOnCooldown = false;
    }

    private void PerformHit()
    {
        Vector2 hitCenter = (Vector2)transform.position + (Vector2)transform.up * range;
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, 0.4f, hitLayers);
        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<Health>()?.TakeDamage(damage);
        }
    }
}
