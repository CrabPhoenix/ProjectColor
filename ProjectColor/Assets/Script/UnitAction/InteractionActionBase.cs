using UnityEngine;

/// <summary>
/// 所有互动类行动的抽象父类，统一互动距离判断和行动消耗入口。
/// </summary>
public abstract class InteractionActionBase : UnitActionBase
{
    public virtual int InteractionRange => 1;

    /// <summary>
    /// 判断两个单位是否在互动范围内。
    /// </summary>
    protected bool IsInInteractionRange(Unit actor, Unit target)
    {
        if(actor == null || target == null) return false;

        actor.UnitMover.RefreshCurrentCell();
        target.UnitMover.RefreshCurrentCell();
        int distance = Mathf.Abs(actor.CurrentCell.X - target.CurrentCell.X) + Mathf.Abs(actor.CurrentCell.Y - target.CurrentCell.Y);
        return distance <= InteractionRange;
    }
}
