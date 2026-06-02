/// <summary>
/// 待命行动，只消耗当前单位本回合行动次数。
/// </summary>
public class WaitAction : UnitActionBase
{
    public override string ActionName => "待命";

    /// <summary>
    /// 待命不需要目标。
    /// </summary>
    public override bool CanExecute(Unit actor, Unit target)
    {
        return target == null && CanExecute(actor);
    }

    /// <summary>
    /// 执行待命并标记已行动。
    /// </summary>
    public override bool Execute(Unit actor, Unit target)
    {
        if(!CanExecute(actor, target)) return false;

        MarkActionConsumed(actor);
        return true;
    }
}
