using UnityEngine;

/// <summary>
/// 记录单位最近一次主动攻击使用的攻击技能，用于后续回击选择。
/// </summary>
public class UnitAttackMemory : MonoBehaviour
{
    private bool hasLastAttackSkill;
    private UnitAttackSkillType lastAttackSkill;

    /// <summary>
    /// 记录单位主动攻击使用的技能。
    /// </summary>
    public void RecordAttackSkill(UnitAttackSkillType attackSkill)
    {
        lastAttackSkill = attackSkill;
        hasLastAttackSkill = true;
    }

    /// <summary>
    /// 获得当前单位用于回击的技能。
    /// </summary>
    public UnitAttackSkillType GetCounterSkillForUnit(Unit unit)
    {
        return hasLastAttackSkill ? lastAttackSkill : UnitAttackSkillSet.GetDefaultSkill(unit);
    }

    /// <summary>
    /// 记录指定单位主动攻击使用的技能。
    /// </summary>
    public static void Record(Unit unit, UnitAttackSkillType attackSkill)
    {
        if(unit == null) return;

        UnitAttackMemory memory = unit.GetComponent<UnitAttackMemory>();
        if(memory == null)
        {
            memory = unit.gameObject.AddComponent<UnitAttackMemory>();
        }

        memory.RecordAttackSkill(attackSkill);
    }

    /// <summary>
    /// 获得指定单位当前用于回击的技能。
    /// </summary>
    public static UnitAttackSkillType GetCounterSkill(Unit unit)
    {
        UnitAttackMemory memory = unit != null ? unit.GetComponent<UnitAttackMemory>() : null;
        return memory != null ? memory.GetCounterSkillForUnit(unit) : UnitAttackSkillSet.GetDefaultSkill(unit);
    }
}
