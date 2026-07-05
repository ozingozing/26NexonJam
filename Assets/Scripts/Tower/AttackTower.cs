using UnityEngine;

public class AttackTower : MonoBehaviour
{
    [Header("Attack Stats")]
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float attackInterval = 1f;

	[Header("Sound")]
	[SerializeField] private AudioClip fireSound;
	[SerializeField] private float fireSoundVolume = 1f;

	[Header("Projectile")]
    [SerializeField] private ProjectileBase projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Target")]
    [SerializeField] private LayerMask enemyMask;

    [Header("Rotation")]
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private float rotateSpeed = 10f;

    private Unit currentTarget;
    private float attackTimer;

    private void Update()
    {
        FindTarget();

        if (currentTarget == null)
        {
            return;
        }

        RotateToTarget();

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            Attack();
        }
    }

    private void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange,
            enemyMask
        );

        if (hits.Length == 0)
        {
            currentTarget = null;
            return;
        }

        float closestDistance = float.MaxValue;
        Unit closestUnit = null;

        for (int i = 0; i < hits.Length; i++)
        {
            Unit unit = hits[i].GetComponent<Unit>();

            if (unit == null)
            {
                continue;
            }

            float distance = Vector3.Distance(
                transform.position,
                unit.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestUnit = unit;
            }
        }

        currentTarget = closestUnit;
    }

    private void RotateToTarget()
    {
        if (rotatingPart == null)
        {
            return;
        }

        Vector3 direction = currentTarget.transform.position - rotatingPart.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        rotatingPart.rotation = Quaternion.Slerp(
            rotatingPart.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    private void Attack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name}에 projectilePrefab이 없습니다.");
            return;
        }

        if (currentTarget == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position;

        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        ProjectileBase projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        projectile.Init(currentTarget, enemyMask);

		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySfx(
				fireSound,
				fireSoundVolume
			);
		}
	}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}