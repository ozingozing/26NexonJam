using UnityEngine;
using System.Collections;
using TMPro;

public class SlowZone : MonoBehaviour
{
    [Header("Slow Zone")]
    [SerializeField] private float radius = 4f;
    [SerializeField] private float duration = 4f;
    [SerializeField] private float slowRate = 0.4f;

    [Header("Detect")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float tickInterval = 0.2f;

    [SerializeField] private GameObject hitEffect;
    private GameObject currentEffect;

    private void Start()
    {
        StartCoroutine(SlowRoutine());

        if (hitEffect != null)
        {
            currentEffect = Instantiate(hitEffect, transform.position + Vector3.up, Quaternion.identity);
        }
        Destroy(gameObject, duration);
    }

    private void OnDestroy()
    {
        if (currentEffect != null)
            Destroy(currentEffect);
    }

    public void Init(LayerMask newEnemyMask)
    {
        enemyMask = newEnemyMask;
    }

    private IEnumerator SlowRoutine()
    {
        while (true)
        {
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

            Debug.Log("asdasd");
            // 장판 안에 있는 동안 계속 슬로우를 갱신
            unit.ApplySlow(slowRate, tickInterval + 0.1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}