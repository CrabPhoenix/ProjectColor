using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 按单位 Prefab 和阵营分组配置单位生命值和可移动格子数。
/// </summary>
[CreateAssetMenu(fileName = "DefaultUnitStatsConfig", menuName = "ProjectColor/Config/Unit Stats Config")]
public class UnitStatsConfig : ScriptableObject
{
    [SerializeField] private List<UnitStatsEntry> playerUnitStats = new List<UnitStatsEntry>();
    [SerializeField] private List<UnitStatsEntry> enemyUnitStats = new List<UnitStatsEntry>();
    [SerializeField] private List<UnitStatsEntry> neutralUnitStats = new List<UnitStatsEntry>();
    [SerializeField] private List<UnitStatsEntry> allyUnitStats = new List<UnitStatsEntry>();

    public IReadOnlyList<UnitStatsEntry> PlayerUnitStats => playerUnitStats;
    public IReadOnlyList<UnitStatsEntry> EnemyUnitStats => enemyUnitStats;
    public IReadOnlyList<UnitStatsEntry> NeutralUnitStats => neutralUnitStats;
    public IReadOnlyList<UnitStatsEntry> AllyUnitStats => allyUnitStats;

    /// <summary>
    /// 获取指定单位对应的属性配置。
    /// </summary>
    public bool TryGetStats(Unit unit, out UnitStatsEntry stats)
    {
        stats = null;
        if(unit == null) return false;

        string unitName = NormalizeUnitName(unit.name);
        foreach(UnitStatsEntry entry in GetAllEntries())
        {
            if(entry == null || entry.UnitPrefab == null) continue;
            if(entry.UnitPrefab.name == unitName)
            {
                stats = entry;
                return true;
            }
        }

        Type unitType = unit.GetType();
        foreach(UnitStatsEntry entry in GetAllEntries())
        {
            if(entry == null || entry.UnitPrefab == null) continue;
            if(entry.UnitPrefab.GetType() == unitType)
            {
                stats = entry;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取指定单位配置的最大生命值。
    /// </summary>
    public int GetMaxHealth(Unit unit, int fallback)
    {
        return TryGetStats(unit, out UnitStatsEntry stats) ? stats.MaxHealth : fallback;
    }

    /// <summary>
    /// 获取指定单位配置的可移动格子数。
    /// </summary>
    public int GetMoveRange(Unit unit, int fallback)
    {
        return TryGetStats(unit, out UnitStatsEntry stats) ? stats.MoveRange : fallback;
    }

    /// <summary>
    /// 在 Inspector 修改时限制配置值并把单位自动归类到正确阵营。
    /// </summary>
    private void OnValidate()
    {
        ClampAllEntries();
        SortEntriesByTeam();
    }

    /// <summary>
    /// 遍历所有阵营中的单位属性配置。
    /// </summary>
    private IEnumerable<UnitStatsEntry> GetAllEntries()
    {
        foreach(UnitStatsEntry entry in playerUnitStats) yield return entry;
        foreach(UnitStatsEntry entry in enemyUnitStats) yield return entry;
        foreach(UnitStatsEntry entry in neutralUnitStats) yield return entry;
        foreach(UnitStatsEntry entry in allyUnitStats) yield return entry;
    }

    /// <summary>
    /// 限制所有配置值范围。
    /// </summary>
    private void ClampAllEntries()
    {
        foreach(UnitStatsEntry entry in GetAllEntries())
        {
            entry?.ClampValues();
        }
    }

    /// <summary>
    /// 根据 Prefab 单位阵营把配置条目移动到对应列表。
    /// </summary>
    private void SortEntriesByTeam()
    {
        List<UnitStatsEntry> unsortedEntries = new List<UnitStatsEntry>();
        CollectEntries(playerUnitStats, unsortedEntries);
        CollectEntries(enemyUnitStats, unsortedEntries);
        CollectEntries(neutralUnitStats, unsortedEntries);
        CollectEntries(allyUnitStats, unsortedEntries);

        playerUnitStats.Clear();
        enemyUnitStats.Clear();
        neutralUnitStats.Clear();
        allyUnitStats.Clear();

        foreach(UnitStatsEntry entry in unsortedEntries)
        {
            AddEntryToTeamList(entry);
        }
    }

    /// <summary>
    /// 收集列表中的全部条目。
    /// </summary>
    private void CollectEntries(List<UnitStatsEntry> source, List<UnitStatsEntry> target)
    {
        foreach(UnitStatsEntry entry in source)
        {
            if(entry != null)
            {
                target.Add(entry);
            }
        }
    }

    /// <summary>
    /// 把单个配置条目添加到其 Prefab 阵营对应的列表。
    /// </summary>
    private void AddEntryToTeamList(UnitStatsEntry entry)
    {
        if(entry.UnitPrefab == null)
        {
            AddUnique(playerUnitStats, entry);
            return;
        }

        switch(entry.UnitPrefab.Team)
        {
            case UnitTeam.Player:
                AddUnique(playerUnitStats, entry);
                break;
            case UnitTeam.Enemy:
                AddUnique(enemyUnitStats, entry);
                break;
            case UnitTeam.Neutral:
                AddUnique(neutralUnitStats, entry);
                break;
            case UnitTeam.Ally:
                AddUnique(allyUnitStats, entry);
                break;
            default:
                AddUnique(playerUnitStats, entry);
                break;
        }
    }

    /// <summary>
    /// 避免同一个 Prefab 配置重复加入列表。
    /// </summary>
    private void AddUnique(List<UnitStatsEntry> target, UnitStatsEntry entry)
    {
        foreach(UnitStatsEntry existingEntry in target)
        {
            if(existingEntry == null || entry == null) continue;
            if(existingEntry.UnitPrefab != null && existingEntry.UnitPrefab == entry.UnitPrefab)
            {
                return;
            }
        }

        target.Add(entry);
    }

    /// <summary>
    /// 去除运行时实例名中的 Clone 后缀，便于匹配 Prefab 名称。
    /// </summary>
    private string NormalizeUnitName(string unitName)
    {
        return unitName.Replace("(Clone)", string.Empty).Trim();
    }
}

/// <summary>
/// 单个单位 Prefab 的属性配置条目。
/// </summary>
[Serializable]
public class UnitStatsEntry
{
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int moveRange = 2;

    public Unit UnitPrefab => unitPrefab;
    public int MaxHealth => Mathf.Max(1, maxHealth);
    public int MoveRange => Mathf.Max(1, moveRange);

    /// <summary>
    /// 限制配置值不小于一。
    /// </summary>
    public void ClampValues()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        moveRange = Mathf.Max(1, moveRange);
    }
}
