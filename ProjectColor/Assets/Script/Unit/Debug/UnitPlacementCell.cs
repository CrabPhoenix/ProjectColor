using System;
using UnityEngine;

/// <summary>
/// 记录一个有效格子上的单位摆放信息。
/// </summary>
[Serializable]
public class UnitPlacementCell
{
    [SerializeField, HideInInspector] private Vector2Int cellPosition;
    [SerializeField, HideInInspector] private UnitTeam unitTeam = UnitTeam.None;
    [SerializeField] private Unit unitPrefab;

    public Vector2Int CellPosition => cellPosition;
    public bool HasUnit => unitPrefab != null;
    public UnitTeam UnitTeam => ResolveUnitTeam();
    public Unit UnitPrefab => unitPrefab;

    public UnitPlacementCell(Vector2Int cellPosition)
    {
        this.cellPosition = cellPosition;
        SyncUnitTeamFromPrefab();
    }

    /// <summary>
    /// 设置当前格子的单位摆放信息。
    /// </summary>
    public void SetUnit(Unit prefab)
    {
        unitPrefab = prefab;
        SyncUnitTeamFromPrefab();
    }

    /// <summary>
    /// 清除当前格子上的单位摆放信息。
    /// </summary>
    public void ClearUnit()
    {
        unitPrefab = null;
        SyncUnitTeamFromPrefab();
    }

    /// <summary>
    /// 根据当前单位 Prefab 同步只读阵营字段。
    /// </summary>
    public void SyncUnitTeamFromPrefab()
    {
        unitTeam = ResolveUnitTeam();
    }

    /// <summary>
    /// 根据当前单位 Prefab 获得对应阵营，没有单位时返回 None。
    /// </summary>
    private UnitTeam ResolveUnitTeam()
    {
        if(unitPrefab == null) return UnitTeam.None;
        return unitPrefab.Team;
    }
}
