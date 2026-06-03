using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 保存部署阶段中玩家可摆放单位的仓库配置。
/// </summary>
[CreateAssetMenu(fileName = "UnitDeployConfig", menuName = "Unit Deploy/Unit Deploy Config")]
public class UnitDeployConfig : ScriptableObject
{
    [SerializeField] private List<UnitDeployEntry> units = new List<UnitDeployEntry>();

    public IReadOnlyList<UnitDeployEntry> Units => units;

    /// <summary>
    /// 获得所有有效的玩家单位部署条目。
    /// </summary>
    public List<UnitDeployEntry> GetValidPlayerEntries()
    {
        List<UnitDeployEntry> validEntries = new List<UnitDeployEntry>();
        foreach(UnitDeployEntry entry in units)
        {
            if(entry == null || entry.UnitPrefab == null) continue;
            if(entry.UnitPrefab.Team != UnitTeam.Player) continue;
            if(entry.Count <= 0) continue;

            validEntries.Add(entry);
        }

        return validEntries;
    }
}
