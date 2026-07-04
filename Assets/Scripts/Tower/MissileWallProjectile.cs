using UnityEngine;

public class MissileWallProjectile : ProjectileBase
{
    [Header("Projectile")]
    [SerializeField] private float moveSpeed = 25f;
    [SerializeField] private float rotateSpeed = 20f;
    [SerializeField] private float hitDistance = 0.4f;

    [Header("Splash Damage")]
    [SerializeField] private int damage = 3;
    [SerializeField] private float splashRadius = 3f;


    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;

    private Vector3 lastTargetPosition;
    private bool hasExploded;

    public override void Init(Unit newTarget, LayerMask newEnemyMask)
    {
        base.Init(newTarget, newEnemyMask);

        if (target != null)
        {
            lastTargetPosition = target.transform.position;
        }
    }

    private void Update()
    {
        if (hasExploded)
        {
            return;
        }

        if (target != null)
        {
            lastTargetPosition = target.transform.position;
        }

        MoveTo(lastTargetPosition);

        float distance = GetXZDistance(transform.position, lastTargetPosition);

        if (distance <= hitDistance)
        {
            Explode(lastTargetPosition + Vector3.up);
        }
    }

    private void MoveTo(Vector3 targetPosition)
    {
        // 탑뷰 미사일이므로 높이는 현재 미사일 높이 유지
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private float GetXZDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    private void Explode(Vector3 explodePosition)
    {
        if (hasExploded)
        {
            return;
        }

        hasExploded = true;

        ApplySplashDamage(explodePosition);

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, explodePosition, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void ApplySplashDamage(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(
            center,
            splashRadius,
            enemyMask
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Unit unit = hits[i].GetComponent<Unit>();

            if (unit == null)
            {
                continue;
            }

            unit.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}