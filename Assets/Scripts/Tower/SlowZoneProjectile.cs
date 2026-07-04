using UnityEngine;

public class SlowZoneProjectile : ProjectileBase
{
    [Header("Projectile Stats")]
    [SerializeField] private float moveSpeed = 25f;
    [SerializeField] private float rotateSpeed = 20f;
    [SerializeField] private float hitDistance = 0.3f;

    [Header("Slow Zone")]
    [SerializeField] private SlowZone slowZonePrefab;

    private Vector3 targetPosition;

    public override void Init(Unit newTarget, LayerMask newEnemyMask)
    {
        base.Init(newTarget, newEnemyMask);

        if (target != null)
        {
            targetPosition = target.transform.position;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            targetPosition = target.transform.position;
        }

        MoveToTargetPosition();

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= hitDistance)
        {
            CreateSlowZone();
        }
    }

    private void MoveToTargetPosition()
    {
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

    private void CreateSlowZone()
    {
        if (slowZonePrefab != null)
        {
            SlowZone slowZone = Instantiate(
                slowZonePrefab,
                targetPosition,
                Quaternion.identity
            );

            slowZone.Init(enemyMask);
        }

        Destroy(gameObject);
    }
}