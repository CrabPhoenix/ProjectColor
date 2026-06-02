/// <summary>
/// 所有攻击类行动的抽象父类，统一阵营目标校验、伤害结算和行动消耗。
/// </summary>
public abstract class AttackActionBase : UnitActionBase
{
    public abstract int Damage { get; }

    /// <summary>
    /// 对目标执行攻击并造成伤害。
    /// </summary>
    public override bool Execute(Unit actor, Unit target)
    {
        if(!CanExecute(actor, target)) return false;

        UnitHealth targetHealth = target.GetComponent<UnitHealth>();
        if(targetHealth == null) return false;

        targetHealth.TakeDamage(new DamageInfo(actor, target, Damage, ActionName));
        MarkActionConsumed(actor);
        return true;
    }

    /// <summary>
    /// 判断目标是否符合攻击阵营规则。
    /// </summary>
    public bool IsValidTarget(Unit actor, Unit target)
    {
        if(actor == null || target == null) return false;
        if(!actor.IsAlive || !target.IsAlive) return false;
        if(actor == target) return false;

        if(actor.Team == UnitTeam.Player || actor.Team == UnitTeam.Ally)
        {
            return target.Team == UnitTeam.Enemy || target.Team == UnitTeam.Neutral;
        }

        if(actor.Team == UnitTeam.Enemy || actor.Team == UnitTeam.Neutral)
        {
            return target.Team != actor.Team;
        }

        return false;
    }
}
