using System;
using UnityEngine;

/// <summary>
/// 管理单位生命值、受伤和死亡逻辑。
/// </summary>
public class UnitHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    private Unit unit;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    public event Action<UnitHealth> OnHealthChanged;
    public event Action<UnitHealth> OnDeath;

    /// <summary>
    /// 初始化生命值并缓存单位组件。
    /// </summary>
    private void Awake()
    {
        unit = GetComponent<Unit>();
        maxHealth = GameConfigProvider.GetMaxHealth(unit, maxHealth);
        maxHealth = Mathf.Max(1, maxHealth);
        if(currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// 受到指定伤害并在生命值耗尽时死亡。
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if(unit == null || !unit.IsAlive) return;
        if(damageInfo.Damage <= 0) return;

        int finalDamage = CalculateDamage(damageInfo);
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        OnHealthChanged?.Invoke(this);

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 根据攻击来源相对当前单位朝向的位置修正最终伤害。
    /// </summary>
    /// <summary>
    /// 根据当前朝向倍率预览最终会受到的伤害。
    /// </summary>
    public int CalculateDamage(DamageInfo damageInfo)
    {
        if(damageInfo.Damage <= 0) return 0;

        return GetFacingModifiedDamage(damageInfo);
    }

    private int GetFacingModifiedDamage(DamageInfo damageInfo)
    {
        if(unit == null || unit.Facing == null || damageInfo.Attacker == null) return damageInfo.Damage;

        UnitFacingSide incomingSide = unit.Facing.GetIncomingSide(damageInfo.Attacker);
        float multiplier = GameConfigProvider.GetFacingDamageMultiplier(incomingSide);
        return Mathf.Max(1, Mathf.RoundToInt(damageInfo.Damage * multiplier));
    }

    /// <summary>
    /// 恢复到满血状态。
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(this);
    }

    /// <summary>
    /// 设置当前生命值，用于替换单位时继承旧单位血量。
    /// </summary>
    public void SetCurrentHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(this);

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 处理死亡、释放占格并隐藏单位。
    /// </summary>
    private void Die()
    {
        if(unit == null || !unit.IsAlive) return;

        unit.SetAlive(false);
        OnDeath?.Invoke(this);
        gameObject.SetActive(false);
    }
}
