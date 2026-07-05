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
	private float currentSpeed;
    private Coroutine slowCoroutine;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 10f;

    [Header("Path Target")]
    public Transform target;

    private int currentHp;

    private Vector3[] path;
    private int targetIndex;
    private Coroutine followPathCoroutine;

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

        RequestPath();
    }

    public void RecalculatePath()
    {
        if (target == null)
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

        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (!pathSuccessful || newPath == null || newPath.Length == 0)
        {
            if (followPathCoroutine != null)
            {
                StopCoroutine(followPathCoroutine);
                followPathCoroutine = null;
            }

            path = null;
            return;
        }

        path = newPath;
        targetIndex = 0;

        if (followPathCoroutine != null)
        {
            StopCoroutine(followPathCoroutine);
        }

        followPathCoroutine = StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];

        while (true)
        {
            if (Vector3.Distance(transform.position, currentWaypoint) < 0.05f)
            {
                targetIndex++;

                if (targetIndex >= path.Length)
                {
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
        Quaternion offsetRotation = Quaternion.Euler(0f, -90f, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation * offsetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    private void OnReachGoal()
    {
		BaseHealth baseHealth = target.GetComponent<BaseHealth>();

		if (baseHealth != null)
		{
			baseHealth.TakeDamage(baseDamage);
		}

		RemoveUnit();
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        RemoveUnit();
    }

    private void RemoveUnit()
    {
        if (removed)
        {
            return;
        }

        removed = true;

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