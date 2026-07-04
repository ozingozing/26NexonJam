using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    protected Unit target;
    protected LayerMask enemyMask;

    public virtual void Init(Unit newTarget, LayerMask newEnemyMask)
    {
        target = newTarget;
        enemyMask = newEnemyMask;
    }
}