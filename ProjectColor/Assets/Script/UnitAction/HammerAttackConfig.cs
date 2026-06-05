using UnityEngine;

/// <summary>
/// 配置 Hammer 攻击行动的伤害、范围和推动距离。
/// </summary>
[CreateAssetMenu(fileName = "HammerAttackConfig", menuName = "ProjectColor/Config/Hammer Attack Config")]
public class HammerAttackConfig : ScriptableObject
{
    [SerializeField] private float damage = 30;
    [SerializeField] private float range = 1;
    [SerializeField] private float pushDistance = 1;

    public int Damage => GetPositiveInteger(damage);
    public int Range => GetPositiveInteger(range);
    public int PushDistance => GetPositiveInteger(pushDistance);

    /// <summary>
    /// 在 Inspector 修改时将所有数值修正为正整数。
    /// </summary>
    private void OnValidate()
    {
        damage = GetPositiveInteger(damage);
        range = GetPositiveInteger(range);
        pushDistance = GetPositiveInteger(pushDistance);
    }

    /// <summary>
    /// 将输入数值四舍五入并限制为正整数。
    /// </summary>
    private int GetPositiveInteger(float value)
    {
        return Mathf.Max(1, Mathf.RoundToInt(value));
    }
}
