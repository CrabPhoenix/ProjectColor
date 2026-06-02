using System;
using UnityEngine;

/// <summary>
/// 表示场景中所有战斗单位的抽象基类，提供存活状态、移动组件和阵营接口。
/// </summary>
[RequireComponent(typeof(UnitMover))]
public abstract class Unit : MonoBehaviour
{
    [SerializeField] private bool isAlive = true;

    private UnitMover unitMover;

    public abstract UnitTeam Team { get; }
    public virtual bool CanPlayerControl => false;
    public virtual bool UsesRandomAI => true;
    public bool IsAlive => isAlive;
    public UnitMover UnitMover => unitMover;
    public GridCell CurrentCell => unitMover != null ? unitMover.CurrentCell : default;
    public event Action<Unit> OnAliveStateChanged;

    /// <summary>
    /// 缓存单位移动组件。
    /// </summary>
    private void Awake()
    {
        unitMover = GetComponent<UnitMover>();
    }

    /// <summary>
    /// 设置单位是否存活，并在死亡时释放格子占用。
    /// </summary>
    public void SetAlive(bool alive)
    {
        if(isAlive == alive) return;

        isAlive = alive;
        if(!isAlive)
        {
            UnitGridOccupancy.UnregisterUnit(this);
        }

        OnAliveStateChanged?.Invoke(this);
    }
}
