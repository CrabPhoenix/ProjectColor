using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弓箭攻击行动，可攻击距离二到三格内的合法目标。
/// </summary>
public class BowAction : AttackActionBase
{
    public override string ActionName => "Bow";
    public override int Damage => GameConfigProvider.GetBowDamage();
    private int MinRange => GameConfigProvider.GetBowMinRange();
    private int MaxRange => GameConfigProvider.GetBowMaxRange();

    /// <summary>
    /// 判断指定目标是否可以被弓箭攻击。
    /// </summary>
    public override bool CanExecute(Unit actor, Unit target)
    {
        if(!CanExecute(actor)) return false;
        if(!IsValidTarget(actor, target)) return false;

        actor.UnitMover.RefreshCurrentCell();
        target.UnitMover.RefreshCurrentCell();
        return IsInBowRange(actor.CurrentCell, target.CurrentCell);
    }

    /// <summary>
    /// 获取弓箭当前可覆盖的有效攻击格子。
    /// </summary>
    public override List<GridCell> GetTargetCells(Unit actor)
    {
        List<GridCell> targetCells = new List<GridCell>();
        if(actor == null || GridManager.Instance == null) return targetCells;

        actor.UnitMover.RefreshCurrentCell();
        GridCell actorCell = actor.CurrentCell;
        for(int x = -MaxRange; x <= MaxRange; x++)
        {
            for(int y = -MaxRange; y <= MaxRange; y++)
            {
                GridCell targetCell = new GridCell(actorCell.X + x, actorCell.Y + y);
                if(!IsInBowRange(actorCell, targetCell)) continue;
                if(!GridManager.Instance.IsCellWalkable(targetCell)) continue;

                targetCells.Add(targetCell);
            }
        }

        return targetCells;
    }

    /// <summary>
    /// 获取当前可以被弓箭攻击的目标。
    /// </summary>
    public List<Unit> GetAttackableTargets(Unit actor)
    {
        List<Unit> targets = new List<Unit>();
        if(actor == null || GridManager.Instance == null) return targets;

        foreach(GridCell targetCell in GetTargetCells(actor))
        {
            if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit target)) continue;
            if(!CanExecute(actor, target)) continue;

            targets.Add(target);
        }

        return targets;
    }

    /// <summary>
    /// 判断从指定格子是否可以攻击目标。
    /// </summary>
    public bool CanAttackFromCell(Unit actor, GridCell actorCell, Unit target)
    {
        if(!IsValidTarget(actor, target)) return false;
        target.UnitMover.RefreshCurrentCell();
        return IsInBowRange(actorCell, target.CurrentCell);
    }

    /// <summary>
    /// 判断目标格是否位于弓箭攻击距离二到三格内。
    /// </summary>
    private bool IsInBowRange(GridCell actorCell, GridCell targetCell)
    {
        int distance = Mathf.Abs(actorCell.X - targetCell.X) + Mathf.Abs(actorCell.Y - targetCell.Y);
        return distance >= MinRange && distance <= MaxRange;
    }
}
