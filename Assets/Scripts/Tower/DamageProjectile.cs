using UnityEngine;

public class DamageProjectile : ProjectileBase
{
    [Header("Projectile Stats")]
    [SerializeField] private float moveSpeed = 30f;
    [SerializeField] private float rotateSpeed = 20f;
    [SerializeField] private float hitDistance = 0.3f;
    [SerializeField] private int damage = 1;

    [Header("Optional")]
    [SerializeField] private GameObject hitEffect;


	[Header("Sound")]
	[SerializeField] private AudioClip hitSound;
	[SerializeField] private float hitSoundVolume = 1f;

	private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position;
        targetPosition.y += 0.5f;

        MoveToTarget(targetPosition);

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= hitDistance)
        {
            HitTarget();
        }
    }

    private void MoveToTarget(Vector3 targetPosition)
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

    private void HitTarget()
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.PlaySfx(
				hitSound,
				hitSoundVolume
			);
		}

		Destroy(gameObject);
    }
}