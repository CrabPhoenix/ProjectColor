using UnityEngine;

/// <summary>
/// 提供游戏运行时需要读取的单位属性和行动属性配置。
/// </summary>
public class GameConfigProvider : MonoBehaviour
{
    private const int DefaultMaxHealth = 100;
    private const int DefaultMoveRange = 2;
    private const int DefaultSwordDamage = 50;
    private const int DefaultSwordRange = 1;
    private const int DefaultBowDamage = 50;
    private const int DefaultBowMinRange = 2;
    private const int DefaultBowMaxRange = 3;

    private static GameConfigProvider instance;

    [SerializeField] private UnitStatsConfig unitStatsConfig;
    [SerializeField] private SwordAttackConfig swordAttackConfig;
    [SerializeField] private BowAttackConfig bowAttackConfig;

    /// <summary>
    /// 建立配置提供者单例并补齐默认配置引用。
    /// </summary>
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ResolveDefaultConfigs();
    }

    /// <summary>
    /// 在 Inspector 修改时自动补齐默认配置引用。
    /// </summary>
    private void OnValidate()
    {
        ResolveDefaultConfigs();
    }

    /// <summary>
    /// 获取指定单位的最大生命值。
    /// </summary>
    public static int GetMaxHealth(Unit unit, int fallback = DefaultMaxHealth)
    {
        UnitStatsConfig config = GetUnitStatsConfig();
        return config != null ? config.GetMaxHealth(unit, fallback) : fallback;
    }

    /// <summary>
    /// 获取指定单位的可移动格子数。
    /// </summary>
    public static int GetMoveRange(Unit unit, int fallback = DefaultMoveRange)
    {
        UnitStatsConfig config = GetUnitStatsConfig();
        return config != null ? config.GetMoveRange(unit, fallback) : fallback;
    }

    /// <summary>
    /// 获取 Sword 攻击伤害。
    /// </summary>
    public static int GetSwordDamage()
    {
        SwordAttackConfig config = GetSwordAttackConfig();
        return config != null ? config.Damage : DefaultSwordDamage;
    }

    /// <summary>
    /// 获取 Sword 攻击最大范围。
    /// </summary>
    public static int GetSwordRange()
    {
        SwordAttackConfig config = GetSwordAttackConfig();
        return config != null ? config.Range : DefaultSwordRange;
    }

    /// <summary>
    /// 获取 Bow 攻击伤害。
    /// </summary>
    public static int GetBowDamage()
    {
        BowAttackConfig config = GetBowAttackConfig();
        return config != null ? config.Damage : DefaultBowDamage;
    }

    /// <summary>
    /// 获取 Bow 攻击最小范围。
    /// </summary>
    public static int GetBowMinRange()
    {
        BowAttackConfig config = GetBowAttackConfig();
        return config != null ? config.MinRange : DefaultBowMinRange;
    }

    /// <summary>
    /// 获取 Bow 攻击最大范围。
    /// </summary>
    public static int GetBowMaxRange()
    {
        BowAttackConfig config = GetBowAttackConfig();
        return config != null ? config.MaxRange : DefaultBowMaxRange;
    }

    /// <summary>
    /// 获取单位属性配置。
    /// </summary>
    private static UnitStatsConfig GetUnitStatsConfig()
    {
        GameConfigProvider provider = ResolveProvider();
        return provider != null ? provider.unitStatsConfig : null;
    }

    /// <summary>
    /// 获取 Sword 攻击配置。
    /// </summary>
    private static SwordAttackConfig GetSwordAttackConfig()
    {
        GameConfigProvider provider = ResolveProvider();
        return provider != null ? provider.swordAttackConfig : null;
    }

    /// <summary>
    /// 获取 Bow 攻击配置。
    /// </summary>
    private static BowAttackConfig GetBowAttackConfig()
    {
        GameConfigProvider provider = ResolveProvider();
        return provider != null ? provider.bowAttackConfig : null;
    }

    /// <summary>
    /// 查找或创建配置提供者。
    /// </summary>
    private static GameConfigProvider ResolveProvider()
    {
        if(instance != null) return instance;

        instance = FindFirstObjectByType<GameConfigProvider>();
        if(instance != null)
        {
            instance.ResolveDefaultConfigs();
            return instance;
        }

        GameObject providerObject = new GameObject("GameConfigProvider");
        instance = providerObject.AddComponent<GameConfigProvider>();
        instance.ResolveDefaultConfigs();
        return instance;
    }

    /// <summary>
    /// 在编辑器中按默认路径查找配置资产。
    /// </summary>
    private void ResolveDefaultConfigs()
    {
#if UNITY_EDITOR
        if(unitStatsConfig == null)
        {
            unitStatsConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitStatsConfig>("Assets/Config/DefaultUnitStatsConfig.asset");
        }

        if(swordAttackConfig == null)
        {
            swordAttackConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<SwordAttackConfig>("Assets/Config/Attack/SwordAttackConfig.asset");
        }

        if(bowAttackConfig == null)
        {
            bowAttackConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<BowAttackConfig>("Assets/Config/Attack/BowAttackConfig.asset");
        }
#endif
    }
}
