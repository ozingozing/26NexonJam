using UnityEngine;
using System.Collections;

public class BuildableWall : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private float recalculateRadius = 2f;

    [Header("Lifetime")]
    [SerializeField] private bool autoDestroy = true;
    [SerializeField] private float lifeTime = 3f;

    private Grid grid;
    private bool removed;

    public void Init(Grid newGrid)
    {
        grid = newGrid;

        RegisterWall();

        if (autoDestroy && lifeTime > 0f)
        {
            StartCoroutine(LifeRoutine());
        }
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        RemoveWall();
    }

    private void RegisterWall()
    {
        if (grid == null)
        {
            grid = FindObjectOfType<Grid>();
        }

        if (grid == null)
        {
            Debug.LogWarning("Grid를 찾을 수 없습니다.");
            return;
        }

        Physics.SyncTransforms();

        grid.RecalculateWalkableArea(transform.position, recalculateRadius);
    }

    public void RemoveWall()
    {
        if (removed)
        {
            return;
        }

        removed = true;

        DisableColliders();

        Physics.SyncTransforms();

        if (grid != null)
        {
            grid.RecalculateWalkableArea(transform.position, recalculateRadius);
        }

        Destroy(gameObject);
    }

    private void DisableColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (removed)
        {
            return;
        }

        removed = true;

        DisableColliders();

        Physics.SyncTransforms();

        if (grid != null)
        {
            grid.RecalculateWalkableArea(transform.position, recalculateRadius);
        }
    }
}