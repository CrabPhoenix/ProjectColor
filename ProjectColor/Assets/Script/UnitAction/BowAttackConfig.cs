using UnityEngine;

/// <summary>
/// 配置 Bow 攻击行动的伤害属性。
/// </summary>
[CreateAssetMenu(fileName = "BowAttackConfig", menuName = "ProjectColor/Config/Bow Attack Config")]
public class BowAttackConfig : ScriptableObject
{
    [SerializeField] private int damage = 50;

    public int Damage => Mathf.Max(0, damage);

    /// <summary>
    /// 在 Inspector 修改时限制伤害不小于零。
    /// </summary>
    private void OnValidate()
    {
        damage = Mathf.Max(0, damage);
    }
}
