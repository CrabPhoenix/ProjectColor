using UnityEngine;

/// <summary>
/// 配置 Bow 攻击行动的伤害属性。
/// </summary>
[CreateAssetMenu(fileName = "BowAttackConfig", menuName = "ProjectColor/Config/Bow Attack Config")]
public class BowAttackConfig : ScriptableObject
{
    [SerializeField] private float damage = 50;
    [SerializeField] private float minRange = 2;
    [SerializeField] private float maxRange = 3;

    public int Damage => GetPositiveInteger(damage);
    public int MinRange => GetPositiveInteger(minRange);
    public int MaxRange => Mathf.Max(MinRange, GetPositiveInteger(maxRange));

    /// <summary>
    /// 在 Inspector 修改时限制伤害不小于零。
    /// </summary>
    private void OnValidate()
    {
        damage = GetPositiveInteger(damage);
        minRange = GetPositiveInteger(minRange);
        maxRange = Mathf.Max(GetPositiveInteger(minRange), GetPositiveInteger(maxRange));
    }

    /// <summary>
    /// 将输入数值四舍五入并限制为正整数。
    /// </summary>
    private int GetPositiveInteger(float value)
    {
        return Mathf.Max(1, Mathf.RoundToInt(value));
    }
}
