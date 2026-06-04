using UnityEngine;

/// <summary>
/// 配置单位受到来自正面、侧面和背面攻击时的伤害倍率。
/// </summary>
[CreateAssetMenu(fileName = "UnitFacingDamageConfig", menuName = "ProjectColor/Config/Unit Facing Damage Config")]
public class UnitFacingDamageConfig : ScriptableObject
{
    [SerializeField] private float frontDamageMultiplier = 1f;
    [SerializeField] private float sideDamageMultiplier = 1.5f;
    [SerializeField] private float backDamageMultiplier = 2f;

    public float FrontDamageMultiplier => frontDamageMultiplier;
    public float SideDamageMultiplier => sideDamageMultiplier;
    public float BackDamageMultiplier => backDamageMultiplier;

    /// <summary>
    /// 在 Inspector 修改时保证倍率不为负数。
    /// </summary>
    private void OnValidate()
    {
        frontDamageMultiplier = Mathf.Max(0f, frontDamageMultiplier);
        sideDamageMultiplier = Mathf.Max(0f, sideDamageMultiplier);
        backDamageMultiplier = Mathf.Max(0f, backDamageMultiplier);
    }
}
