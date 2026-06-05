/// <summary>
/// 记录一次战斗预览中主动攻击与回击的伤害结果。
/// </summary>
public struct CombatPreviewData
{
    public bool IsValid;
    public Unit Attacker;
    public Unit Defender;
    public UnitAttackSkillType AttackSkill;
    public int AttackDamage;
    public bool CanCounter;
    public UnitAttackSkillType CounterSkill;
    public int CounterDamage;

    /// <summary>
    /// 创建一次有效的战斗预览数据。
    /// </summary>
    public CombatPreviewData(Unit attacker, Unit defender, UnitAttackSkillType attackSkill, int attackDamage, bool canCounter, UnitAttackSkillType counterSkill, int counterDamage)
    {
        IsValid = true;
        Attacker = attacker;
        Defender = defender;
        AttackSkill = attackSkill;
        AttackDamage = attackDamage;
        CanCounter = canCounter;
        CounterSkill = counterSkill;
        CounterDamage = counterDamage;
    }
}
