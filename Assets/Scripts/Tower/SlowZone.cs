using UnityEngine;
using System.Collections;

public class SlowZone : MonoBehaviour
{
	[Header("Slow Zone")]
	[SerializeField] private float radius = 4f;
	[SerializeField] private float duration = 4f;
	[SerializeField] private float slowRate = 0.4f;

	[Header("Detect")]
	[SerializeField] private LayerMask enemyMask;
	[SerializeField] private float tickInterval = 0.2f;

	[Header("Sound")]
	[SerializeField] private AudioClip loopSound;
	[SerializeField] private float loopSoundVolume = 0.7f;

	[SerializeField] private GameObject hitEffect;

	private AudioSource loopAudioSource;
	private GameObject currentEffect;

	private void Start()
	{
		PlayLoopSound();

		if (hitEffect != null)
		{
			currentEffect = Instantiate(
				hitEffect,
				transform.position + Vector3.up,
				Quaternion.identity
			);
		}

		StartCoroutine(SlowRoutine());
		StartCoroutine(LifeRoutine());
	}

	private void OnDestroy()
	{
		if (currentEffect != null)
		{
			Destroy(currentEffect);
		}

		StopLoopSound();
	}

	public void Init(LayerMask newEnemyMask)
	{
		enemyMask = newEnemyMask;
	}

	private IEnumerator LifeRoutine()
	{
		yield return new WaitForSeconds(duration);

		Destroy(gameObject);
	}

	private IEnumerator SlowRoutine()
	{
		while (true)
		{
			if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
			{
				yield break;
			}

			ApplySlowToEnemies();
			yield return new WaitForSeconds(tickInterval);
		}
	}

	private void ApplySlowToEnemies()
	{
		Collider[] hits = Physics.OverlapSphere(
			transform.position,
			radius,
			enemyMask
		);

		for (int i = 0; i < hits.Length; i++)
		{
			Unit unit = hits[i].GetComponent<Unit>();

			if (unit == null)
			{
				continue;
			}

			unit.ApplySlow(slowRate, tickInterval + 0.1f);
		}
	}

	private void PlayLoopSound()
	{
		if (AudioManager.Instance == null)
		{
			return;
		}

		loopAudioSource = AudioManager.Instance.PlayLoopSfx(
			loopSound,
			loopSoundVolume
		);
	}

	private void StopLoopSound()
	{
		if (loopAudioSource == null)
		{
			return;
		}

		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.StopLoopSfx(loopAudioSource);
		}
		else
		{
			Destroy(loopAudioSource.gameObject);
		}

		loopAudioSource = null;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}