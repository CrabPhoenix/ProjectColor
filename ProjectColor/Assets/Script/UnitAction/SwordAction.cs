using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近战剑攻击行动，只能攻击四方向相邻一格内的合法目标。
/// </summary>
public class SwordAction : AttackActionBase
{
    public override string ActionName => "Sword";
    public override UnitAttackSkillType SkillType => UnitAttackSkillType.Sword;
    public override int Damage => GameConfigProvider.GetSwordDamage();
    private int Range => GameConfigProvider.GetSwordRange();

    /// <summary>
    /// 判断指定目标是否可以被剑攻击。
    /// </summary>
    public override bool CanExecute(Unit actor, Unit target)
    {
        if(!CanExecute(actor)) return false;
        if(!IsValidTarget(actor, target)) return false;

        actor.UnitMover.RefreshCurrentCell();
        target.UnitMover.RefreshCurrentCell();
        return IsInRange(actor.CurrentCell, target.CurrentCell);
    }

    /// <summary>
    /// 获得当前剑攻击可以覆盖的有效相邻格子。
    /// </summary>
    public override List<GridCell> GetTargetCells(Unit actor)
    {
        List<GridCell> targetCells = new List<GridCell>();
        if(actor == null || GridManager.Instance == null) return targetCells;

        actor.UnitMover.RefreshCurrentCell();
        GridCell actorCell = actor.CurrentCell;
        int range = Range;
        for(int x = -range; x <= range; x++)
        {
            for(int y = -range; y <= range; y++)
            {
                GridCell targetCell = new GridCell(actorCell.X + x, actorCell.Y + y);
                if(!IsInRange(actorCell, targetCell)) continue;
                if(!GridManager.Instance.IsCellWalkable(targetCell)) continue;

                targetCells.Add(targetCell);
            }
        }

        return targetCells;
    }

    /// <summary>
    /// 获得当前可以攻击的相邻目标。
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
    /// 判断两个格子是否四方向相邻。
    /// </summary>
    public bool IsAdjacent(GridCell firstCell, GridCell secondCell)
    {
        int distance = Mathf.Abs(firstCell.X - secondCell.X) + Mathf.Abs(firstCell.Y - secondCell.Y);
        return distance == 1;
    }

    /// <summary>
    /// 判断从指定格子能否攻击目标。
    /// </summary>
    public bool CanAttackFromCell(Unit actor, GridCell actorCell, Unit target)
    {
        if(!IsValidTarget(actor, target)) return false;
        target.UnitMover.RefreshCurrentCell();
        return IsInRange(actorCell, target.CurrentCell);
    }

    /// <summary>
    /// 判断目标格是否位于剑攻击最大范围内。
    /// </summary>
    private bool IsInRange(GridCell firstCell, GridCell secondCell)
    {
        int distance = Mathf.Abs(firstCell.X - secondCell.X) + Mathf.Abs(firstCell.Y - secondCell.Y);
        return distance >= 1 && distance <= Range;
    }
}
