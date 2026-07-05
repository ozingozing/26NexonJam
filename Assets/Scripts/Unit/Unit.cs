using UnityEngine;
using System;
using System.Collections;

public class Unit : MonoBehaviour
{
	public event Action<Unit> OnUnitRemoved;

	[Header("Unit Type")]
	public UnitType unitType;

	[Header("Stats")]
	[SerializeField] private float speed = 20f;
	[SerializeField] private int maxHp = 10;
	[SerializeField] private int baseDamage = 1;

	[Header("Reward")]
	[SerializeField] private int goldReward = 10;

	[Header("Rotation")]
	[SerializeField] private float rotateSpeed = 10f;

	[Header("Path Target")]
	public Transform target;

	private int currentHp;
	private float currentSpeed;

	private Vector3[] path;
	private int targetIndex;

	private Coroutine followPathCoroutine;
	private Coroutine slowCoroutine;

	private bool initialized;
	private bool removed;

	private void Start()
	{
		currentHp = maxHp;
		currentSpeed = speed;

		// 씬에 미리 배치된 유닛용
		if (!initialized && target != null)
		{
			initialized = true;
			RequestPath();
		}
	}

	public void Init(Transform newTarget)
	{
		target = newTarget;

		currentHp = maxHp;
		currentSpeed = speed;

		initialized = true;
		removed = false;

		RequestPath();
	}

	public void RecalculatePath()
	{
		if (target == null)
		{
			return;
		}

		if (removed || !gameObject.activeInHierarchy)
		{
			return;
		}

		RequestPath();
	}

	private void RequestPath()
	{
		if (target == null)
		{
			Debug.LogWarning($"{name}의 target이 없습니다.");
			return;
		}

		if (removed || !gameObject.activeInHierarchy)
		{
			return;
		}

		Unit unit = this;

		PathRequestManager.RequestPath(
			transform.position,
			target.position,
			(newPath, pathSuccessful) =>
			{
				// 이 콜백은 경로 계산이 끝난 뒤 나중에 호출됩니다.
				// 그 사이 Unit이 Destroy되었을 수 있으므로 반드시 체크해야 합니다.
				if (unit == null)
				{
					return;
				}

				if (unit.removed || !unit.gameObject.activeInHierarchy)
				{
					Debug.LogWarning($"[{unit.name}] 경로 계산 완료 후 Unit이 제거되었거나 비활성화 상태입니다. 경로 결과를 무시합니다.");
					return;
				}

				unit.OnPathFound(newPath, pathSuccessful);
			}
		);
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
	{
		if (removed || !gameObject.activeInHierarchy)
		{
			return;
		}

		if (!pathSuccessful || newPath == null || newPath.Length == 0)
		{
			return;
		}

		path = newPath;
		targetIndex = 0;

		if (followPathCoroutine != null)
		{
			StopCoroutine(followPathCoroutine);
			followPathCoroutine = null;
		}

		followPathCoroutine = StartCoroutine(FollowPath());
	}

	private IEnumerator FollowPath()
	{
		if (path == null || path.Length == 0)
		{
			followPathCoroutine = null;
			yield break;
		}

		Vector3 currentWaypoint = path[0];

		while (true)
		{
			if (removed || !gameObject.activeInHierarchy)
			{
				followPathCoroutine = null;
				yield break;
			}

			if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
			{
				followPathCoroutine = null;
				yield break;
			}

			if (Vector3.Distance(transform.position, currentWaypoint) < 0.05f)
			{
				targetIndex++;

				if (targetIndex >= path.Length)
				{
					followPathCoroutine = null;
					OnReachGoal();
					yield break;
				}

				currentWaypoint = path[targetIndex];
			}

			LookAtWaypoint(currentWaypoint);

			transform.position = Vector3.MoveTowards(
				transform.position,
				currentWaypoint,
				currentSpeed * Time.deltaTime
			);

			yield return null;
		}
	}

	private void LookAtWaypoint(Vector3 waypoint)
	{
		Vector3 direction = waypoint - transform.position;
		direction.y = 0f;

		if (direction.sqrMagnitude < 0.001f)
		{
			return;
		}

		Quaternion targetRotation = Quaternion.LookRotation(direction);

		// 모델 앞 방향 보정값
		Quaternion offsetRotation = Quaternion.Euler(0f, -90f, 0f);

		transform.rotation = Quaternion.Slerp(
			transform.rotation,
			targetRotation * offsetRotation,
			rotateSpeed * Time.deltaTime
		);
	}

	private void OnReachGoal()
	{
		if (removed)
		{
			return;
		}

		if (target != null)
		{
			BaseHealth baseHealth = target.GetComponent<BaseHealth>();

			if (baseHealth != null)
			{
				baseHealth.TakeDamage(baseDamage);
			}
		}

		RemoveUnit();
	}

	public void TakeDamage(int damage)
	{
		if (removed)
		{
			return;
		}

		currentHp -= damage;

		if (currentHp <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		if (removed)
		{
			return;
		}

		if (PlayerGold.Instance != null)
		{
			PlayerGold.Instance.AddGold(goldReward);
		}

		RemoveUnit();
	}

	private void RemoveUnit()
	{
		if (removed)
		{
			return;
		}

		removed = true;

		if (followPathCoroutine != null)
		{
			StopCoroutine(followPathCoroutine);
			followPathCoroutine = null;
		}

		if (slowCoroutine != null)
		{
			StopCoroutine(slowCoroutine);
			slowCoroutine = null;
		}

		OnUnitRemoved?.Invoke(this);

		Destroy(gameObject);
	}

	private void OnDestroy()
	{
		if (removed)
		{
			return;
		}

		removed = true;

		OnUnitRemoved?.Invoke(this);
	}

	public void ApplySlow(float slowRate, float duration)
	{
		if (removed || !gameObject.activeInHierarchy)
		{
			return;
		}

		slowRate = Mathf.Clamp01(slowRate);

		if (slowCoroutine != null)
		{
			StopCoroutine(slowCoroutine);
		}

		slowCoroutine = StartCoroutine(SlowRoutine(slowRate, duration));
	}

	private IEnumerator SlowRoutine(float slowRate, float duration)
	{
		currentSpeed = speed * (1f - slowRate);

		yield return new WaitForSeconds(duration);

		currentSpeed = speed;
		slowCoroutine = null;
	}

	private void OnDrawGizmos()
	{
		if (path == null)
		{
			return;
		}

		for (int i = targetIndex; i < path.Length; i++)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawCube(path[i], Vector3.one);

			if (i == targetIndex)
			{
				Gizmos.DrawLine(transform.position, path[i]);
			}
			else
			{
				Gizmos.DrawLine(path[i - 1], path[i]);
			}
		}
	}
}