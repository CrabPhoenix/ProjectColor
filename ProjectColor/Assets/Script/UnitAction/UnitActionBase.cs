using System.Collections.Generic;

/// <summary>
/// 所有单位行动的抽象基类，统一行动名称、目标校验和执行入口。
/// </summary>
public abstract class UnitActionBase
{
    public abstract string ActionName { get; }
    public virtual bool ConsumesAction => true;

    /// <summary>
    /// 判断指定单位是否可以执行该行动。
    /// </summary>
    public virtual bool CanExecute(Unit actor)
    {
        if(actor == null || !actor.IsAlive) return false;

        UnitActionState actionState = actor.GetComponent<UnitActionState>();
        return actionState == null || actionState.CanAct;
    }

    /// <summary>
    /// 判断指定单位是否可以对目标执行该行动。
    /// </summary>
    public abstract bool CanExecute(Unit actor, Unit target);

    /// <summary>
    /// 执行该行动。
    /// </summary>
    public abstract bool Execute(Unit actor, Unit target);

    /// <summary>
    /// 获得当前可作为目标的格子。
    /// </summary>
    public virtual List<GridCell> GetTargetCells(Unit actor)
    {
        return new List<GridCell>();
    }

    /// <summary>
    /// 行动成功后记录本回合已行动。
    /// </summary>
    protected void MarkActionConsumed(Unit actor)
    {
        if(!ConsumesAction || actor == null) return;

        UnitActionState actionState = actor.GetComponent<UnitActionState>();
        if(actionState != null)
        {
            actionState.MarkActed();
        }
    }
}
