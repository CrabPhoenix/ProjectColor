using UnityEngine;

/// <summary>
/// 所有攻击类行动的抽象父类，统一阵营目标校验、伤害结算、朝向更新和行动消耗。
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

        FaceAttackTarget(actor, target);
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

    /// <summary>
    /// 攻击成功时让攻击者朝向受击者所在方向。
    /// </summary>
    private void FaceAttackTarget(Unit actor, Unit target)
    {
        if(actor == null || target == null || actor.Facing == null) return;

        Vector2 attackDirection = target.transform.position - actor.transform.position;
        if(Mathf.Abs(attackDirection.x) == Mathf.Abs(attackDirection.y)) attackDirection.x += 0.01f; // 避免斜向时无法正确判断朝向
        Direction closestDirection = UnitFacing.GetClosestDirection(attackDirection);
        if(closestDirection == Direction.Invalid) return;

        actor.Facing.Face(closestDirection);
    }
}
