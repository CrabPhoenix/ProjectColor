using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 锤击攻击行动，对近距离目标造成伤害并将其沿攻击方向推开。
/// </summary>
public class HammerAction : AttackActionBase
{
    public override string ActionName => "Hammer";
    public override UnitAttackSkillType SkillType => UnitAttackSkillType.Hammer;
    public override int Damage => GameConfigProvider.GetHammerDamage();
    private int Range => GameConfigProvider.GetHammerRange();
    private int PushDistance => GameConfigProvider.GetHammerPushDistance();

    /// <summary>
    /// 判断指定目标是否可以被 Hammer 攻击。
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
    /// 获得当前 Hammer 可覆盖的攻击格子。
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
                if(!GridManager.Instance.IsValidCell(targetCell)) continue;

                targetCells.Add(targetCell);
            }
        }

        return targetCells;
    }

    /// <summary>
    /// 获得当前可以被 Hammer 攻击的目标。
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
    /// 判断从指定格子是否可以 Hammer 攻击目标。
    /// </summary>
    public bool CanAttackFromCell(Unit actor, GridCell actorCell, Unit target)
    {
        if(!IsValidTarget(actor, target)) return false;
        target.UnitMover.RefreshCurrentCell();
        return IsInRange(actorCell, target.CurrentCell);
    }

    /// <summary>
    /// 对目标造成 Hammer 伤害，并在目标存活时尝试推动目标。
    /// </summary>
    public override bool DealDamage(Unit actor, Unit target, bool consumeAction)
    {
        if(actor != null && actor.UnitMover != null) actor.UnitMover.RefreshCurrentCell();
        if(target != null && target.UnitMover != null) target.UnitMover.RefreshCurrentCell();

        GridCell actorCell = actor != null && actor.UnitMover != null ? actor.CurrentCell : default;
        GridCell targetCell = target != null && target.UnitMover != null ? target.CurrentCell : default;
        bool success = base.DealDamage(actor, target, consumeAction);
        if(success && target != null && target.IsAlive)
        {
            PushTarget(actor, target, actorCell, targetCell);
        }

        return success;
    }

    /// <summary>
    /// 预判 Hammer 推动是否会让目标进入水地形死亡。
    /// </summary>
    public override bool WillTargetDieFromPostDamageEffect(Unit actor, Unit target, int previewDamage)
    {
        if(actor == null || target == null || GridManager.Instance == null) return false;

        UnitHealth targetHealth = target.GetComponent<UnitHealth>();
        if(targetHealth == null || targetHealth.CurrentHealth <= previewDamage) return false;

        actor.UnitMover.RefreshCurrentCell();
        target.UnitMover.RefreshCurrentCell();

        GridCell destinationCell = GetPushDestination(actor.CurrentCell, target.CurrentCell, target);
        if(destinationCell == target.CurrentCell) return false;

        return GridManager.Instance.GetTerrainType(destinationCell) == TerrainType.Water;
    }

    /// <summary>
    /// 尝试将目标沿攻击方向推开。
    /// </summary>
    private void PushTarget(Unit actor, Unit target, GridCell actorCell, GridCell targetCell)
    {
        if(actor == null || target == null || target.UnitMover == null || GridManager.Instance == null) return;

        Direction pushDirection = GetDirectionFromAttack(actorCell, targetCell);
        if(pushDirection == Direction.Invalid) return;

        GridCell destinationCell = GetPushDestination(actorCell, targetCell, target);

        if(destinationCell == targetCell) return;

        target.UnitMover.TryForceMoveToCell(destinationCell);
    }

    /// <summary>
    /// 计算 Hammer 推动后的最终落点。
    /// </summary>
    private GridCell GetPushDestination(GridCell actorCell, GridCell targetCell, Unit target)
    {
        Direction pushDirection = GetDirectionFromAttack(actorCell, targetCell);
        if(pushDirection == Direction.Invalid) return targetCell;

        GridCell destinationCell = targetCell;
        for(int i = 0; i < PushDistance; i++)
        {
            GridCell nextCell = GridManager.Instance.GetNeighborCell(destinationCell, pushDirection);
            if(!CanPushToCell(nextCell, target)) break;

            destinationCell = nextCell;
        }

        return destinationCell;
    }

    /// <summary>
    /// 判断目标是否可以被推入指定格子。
    /// </summary>
    private bool CanPushToCell(GridCell cell, Unit target)
    {
        if(GridManager.Instance == null || !GridManager.Instance.IsValidCell(cell)) return false;
        if(UnitGridOccupancy.IsCellOccupied(cell, target)) return false;

        TerrainType terrainType = GridManager.Instance.GetTerrainType(cell);
        return terrainType == TerrainType.Plate || terrainType == TerrainType.Slope || terrainType == TerrainType.Water;
    }

    /// <summary>
    /// 根据攻击者与目标格子计算推动方向。
    /// </summary>
    private Direction GetDirectionFromAttack(GridCell actorCell, GridCell targetCell)
    {
        int deltaX = targetCell.X - actorCell.X;
        int deltaY = targetCell.Y - actorCell.Y;

        if(Mathf.Abs(deltaX) >= Mathf.Abs(deltaY))
        {
            if(deltaX > 0) return Direction.Right;
            if(deltaX < 0) return Direction.Left;
        }

        if(deltaY > 0) return Direction.Up;
        if(deltaY < 0) return Direction.Down;

        return Direction.Invalid;
    }

    /// <summary>
    /// 判断目标格是否在 Hammer 攻击范围内。
    /// </summary>
    private bool IsInRange(GridCell firstCell, GridCell secondCell)
    {
        int distance = Mathf.Abs(firstCell.X - secondCell.X) + Mathf.Abs(firstCell.Y - secondCell.Y);
        return distance >= 1 && distance <= Range;
    }
}
