using UnityEngine;

/// <summary>
/// 统一计算并执行主动攻击与回击结算。
/// </summary>
public static class CombatResolver
{
    private static readonly SwordAction swordAction = new SwordAction();
    private static readonly BowAction bowAction = new BowAction();

    /// <summary>
    /// 尝试生成战斗预览数据。
    /// </summary>
    public static bool TryBuildPreview(Unit attacker, Unit defender, UnitAttackSkillType attackSkill, out CombatPreviewData previewData)
    {
        previewData = default;
        AttackActionBase attackAction = GetAttackAction(attackSkill);
        if(attackAction == null || !attackAction.CanExecute(attacker, defender)) return false;

        int attackDamage = attackAction.PreviewDamage(attacker, defender);
        bool canCounter = false;
        int counterDamage = 0;
        UnitAttackSkillType counterSkill = UnitAttackMemory.GetCounterSkill(defender);

        UnitHealth defenderHealth = defender != null ? defender.GetComponent<UnitHealth>() : null;
        bool defenderSurvives = defenderHealth != null && defenderHealth.CurrentHealth > attackDamage;
        if(defenderSurvives && CanCounter(defender, attacker, counterSkill))
        {
            AttackActionBase counterAction = GetAttackAction(counterSkill);
            counterDamage = counterAction != null ? counterAction.PreviewDamage(defender, attacker) : 0;
            canCounter = counterDamage > 0;
        }

        previewData = new CombatPreviewData(attacker, defender, attackSkill, attackDamage, canCounter, counterSkill, counterDamage);
        return true;
    }

    /// <summary>
    /// 根据预览数据重新校验并执行战斗。
    /// </summary>
    public static bool ExecuteCombat(CombatPreviewData previewData)
    {
        if(!previewData.IsValid) return false;

        return ExecuteCombat(previewData.Attacker, previewData.Defender, previewData.AttackSkill);
    }

    /// <summary>
    /// 执行一次主动攻击，并在条件满足时立即执行回击。
    /// </summary>
    public static bool ExecuteCombat(Unit attacker, Unit defender, UnitAttackSkillType attackSkill)
    {
        if(!TryBuildPreview(attacker, defender, attackSkill, out CombatPreviewData previewData)) return false;

        AttackActionBase attackAction = GetAttackAction(attackSkill);
        if(attackAction == null || !attackAction.DealDamage(attacker, defender, true)) return false;

        UnitAttackMemory.Record(attacker, attackSkill);

        if(defender == null || !defender.IsAlive || !previewData.CanCounter) return true;

        AttackActionBase counterAction = GetAttackAction(previewData.CounterSkill);
        if(counterAction == null) return true;

        counterAction.DealDamage(defender, attacker, false);
        return true;
    }

    /// <summary>
    /// 获得指定技能对应的攻击行动实例。
    /// </summary>
    public static AttackActionBase GetAttackAction(UnitAttackSkillType attackSkill)
    {
        switch(attackSkill)
        {
            case UnitAttackSkillType.Sword:
                return swordAction;
            case UnitAttackSkillType.Bow:
                return bowAction;
            default:
                return swordAction;
        }
    }

    /// <summary>
    /// 判断防守方是否可以使用指定技能回击主动方。
    /// </summary>
    private static bool CanCounter(Unit defender, Unit attacker, UnitAttackSkillType counterSkill)
    {
        if(defender == null || attacker == null) return false;
        if(!defender.IsAlive || !attacker.IsAlive) return false;
        if(!UnitAttackSkillSet.HasSkill(defender, counterSkill)) return false;

        defender.UnitMover.RefreshCurrentCell();
        attacker.UnitMover.RefreshCurrentCell();

        if(counterSkill == UnitAttackSkillType.Sword)
        {
            return swordAction.CanAttackFromCell(defender, defender.CurrentCell, attacker);
        }

        if(counterSkill == UnitAttackSkillType.Bow)
        {
            return bowAction.CanAttackFromCell(defender, defender.CurrentCell, attacker);
        }

        return false;
    }
}
