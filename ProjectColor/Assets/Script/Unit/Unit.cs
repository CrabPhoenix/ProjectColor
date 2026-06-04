using System;
using UnityEngine;

/// <summary>
/// 表示场景中所有战斗单位的抽象基类，提供存活状态、移动组件、生命值组件和运行时阵营接口。
/// </summary>
[RequireComponent(typeof(UnitMover))]
public abstract class Unit : MonoBehaviour
{
    [SerializeField] private bool isAlive = true;
    [SerializeField] private UnitTeam runtimeTeamOverride = UnitTeam.None;

    private UnitMover unitMover;
    private UnitHealth unitHealth;
    private UnitActionState unitActionState;
    private UnitFacing unitFacing;

    protected abstract UnitTeam NativeTeam { get; }

    public UnitTeam Team => runtimeTeamOverride == UnitTeam.None ? NativeTeam : runtimeTeamOverride;
    public virtual bool CanPlayerControl => Team == UnitTeam.Player;
    public virtual bool UsesRandomAI => Team != UnitTeam.Player && Team != UnitTeam.None;
    public bool IsAlive => isAlive;
    public UnitMover UnitMover => unitMover;
    public UnitHealth Health => unitHealth;
    public UnitActionState ActionState => unitActionState;
    public UnitFacing Facing => unitFacing;
    public GridCell CurrentCell => unitMover != null ? unitMover.CurrentCell : default;
    public event Action<Unit> OnAliveStateChanged;
    public event Action<Unit> OnTeamChanged;

    /// <summary>
    /// 缓存并补齐单位运行所需组件。
    /// </summary>
    private void Awake()
    {
        unitMover = GetComponent<UnitMover>();
        unitFacing = GetComponent<UnitFacing>();
        if(unitFacing == null)
        {
            unitFacing = gameObject.AddComponent<UnitFacing>();
        }
        unitFacing.FaceTeamDefault(Team);

        unitHealth = GetComponent<UnitHealth>();
        if(unitHealth == null)
        {
            unitHealth = gameObject.AddComponent<UnitHealth>();
        }

        if(GetComponent<UnitHealthBar>() == null)
        {
            gameObject.AddComponent<UnitHealthBar>();
        }

        unitActionState = GetComponent<UnitActionState>();
        if(unitActionState == null)
        {
            unitActionState = gameObject.AddComponent<UnitActionState>();
        }

        UnitFacingHoverController.EnsureExists();
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

    /// <summary>
    /// 设置运行时阵营覆盖，用于转化等临时阵营变化。
    /// </summary>
    public void SetTeamOverride(UnitTeam team)
    {
        if(runtimeTeamOverride == team) return;

        runtimeTeamOverride = team;
        OnTeamChanged?.Invoke(this);
    }

    /// <summary>
    /// 清除运行时阵营覆盖，恢复单位原生阵营。
    /// </summary>
    public void ClearTeamOverride()
    {
        SetTeamOverride(UnitTeam.None);
    }
}
