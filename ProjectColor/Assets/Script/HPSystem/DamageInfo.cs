/// <summary>
/// 记录一次伤害结算需要的来源、目标、数值和行动名称。
/// </summary>
public readonly struct DamageInfo
{
    public readonly Unit Attacker;
    public readonly Unit Target;
    public readonly int Damage;
    public readonly string ActionName;

    /// <summary>
    /// 创建一次伤害信息。
    /// </summary>
    public DamageInfo(Unit attacker, Unit target, int damage, string actionName)
    {
        Attacker = attacker;
        Target = target;
        Damage = damage;
        ActionName = actionName;
    }
}
