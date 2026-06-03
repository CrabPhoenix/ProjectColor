using UnityEngine;

/// <summary>
/// 配置 Sword 攻击行动的伤害属性。
/// </summary>
[CreateAssetMenu(fileName = "SwordAttackConfig", menuName = "ProjectColor/Config/Sword Attack Config")]
public class SwordAttackConfig : ScriptableObject
{
    [SerializeField] private float damage = 50;
    [SerializeField] private float range = 1;

    public int Damage => GetPositiveInteger(damage);
    public int Range => GetPositiveInteger(range);

    /// <summary>
    /// 在 Inspector 修改时限制伤害不小于零。
    /// </summary>
    private void OnValidate()
    {
        damage = GetPositiveInteger(damage);
        range = GetPositiveInteger(range);
    }

    /// <summary>
    /// 将输入数值四舍五入并限制为正整数。
    /// </summary>
    private int GetPositiveInteger(float value)
    {
        return Mathf.Max(1, Mathf.RoundToInt(value));
    }
}
