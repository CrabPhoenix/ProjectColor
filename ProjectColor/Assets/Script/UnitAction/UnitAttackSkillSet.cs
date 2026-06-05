using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 配置单个单位 Prefab 在玩家攻击菜单中拥有的攻击技能。
/// </summary>
public class UnitAttackSkillSet : MonoBehaviour
{
    [SerializeField] private List<UnitAttackSkillType> attackSkills = new List<UnitAttackSkillType> { UnitAttackSkillType.Sword };

    public IReadOnlyList<UnitAttackSkillType> AttackSkills => attackSkills;

    /// <summary>
    /// 判断指定单位是否拥有某个攻击技能。
    /// </summary>
    public static bool HasSkill(Unit unit, UnitAttackSkillType skillType)
    {
        UnitAttackSkillSet skillSet = unit != null ? unit.GetComponent<UnitAttackSkillSet>() : null;
        if(skillSet == null || skillSet.attackSkills == null || skillSet.attackSkills.Count == 0)
        {
            return skillType == UnitAttackSkillType.Sword;
        }

        return skillSet.attackSkills.Contains(skillType);
    }

    /// <summary>
    /// 获得单位默认用于回击的攻击技能。
    /// </summary>
    public static UnitAttackSkillType GetDefaultSkill(Unit unit)
    {
        UnitAttackSkillSet skillSet = unit != null ? unit.GetComponent<UnitAttackSkillSet>() : null;
        if(skillSet == null || skillSet.attackSkills == null || skillSet.attackSkills.Count == 0)
        {
            return UnitAttackSkillType.Sword;
        }

        return skillSet.attackSkills[0];
    }

    /// <summary>
    /// 获得单位当前配置的全部攻击技能。
    /// </summary>
    public static List<UnitAttackSkillType> GetSkills(Unit unit)
    {
        UnitAttackSkillSet skillSet = unit != null ? unit.GetComponent<UnitAttackSkillSet>() : null;
        if(skillSet == null || skillSet.attackSkills == null || skillSet.attackSkills.Count == 0)
        {
            return new List<UnitAttackSkillType> { UnitAttackSkillType.Sword };
        }

        return new List<UnitAttackSkillType>(skillSet.attackSkills);
    }

    /// <summary>
    /// 在 Inspector 修改时移除重复技能。
    /// </summary>
    private void OnValidate()
    {
        if(attackSkills == null)
        {
            attackSkills = new List<UnitAttackSkillType>();
            return;
        }

        HashSet<UnitAttackSkillType> seenSkills = new HashSet<UnitAttackSkillType>();
        for(int i = attackSkills.Count - 1; i >= 0; i--)
        {
            if(!seenSkills.Add(attackSkills[i]))
            {
                attackSkills.RemoveAt(i);
            }
        }
    }
}
