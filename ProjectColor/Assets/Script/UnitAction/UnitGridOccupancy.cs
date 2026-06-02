using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 记录所有单位当前占据的格子，供移动校验和 Debug 显示使用。
/// </summary>
public static class UnitGridOccupancy
{
    private static readonly Dictionary<GridCell, Unit> occupiedUnits = new Dictionary<GridCell, Unit>();
    private static readonly List<Unit> units = new List<Unit>();

    /// <summary>
    /// 注册单位并记录其当前所在格子。
    /// </summary>
    public static void RegisterUnit(Unit unit, GridCell cell)
    {
        if(unit == null) return;

        CleanupNullUnits();
        RemoveUnitFromCells(unit);

        if(!units.Contains(unit))
        {
            units.Add(unit);
        }

        if(occupiedUnits.TryGetValue(cell, out Unit otherUnit) && otherUnit != null && otherUnit != unit)
        {
            Debug.LogWarning($"{unit.name} 尝试注册到已被 {otherUnit.name} 占据的格子 {cell}");
            return;
        }

        occupiedUnits[cell] = unit;
    }

    /// <summary>
    /// 取消单位的格子占用记录。
    /// </summary>
    public static void UnregisterUnit(Unit unit)
    {
        if(unit == null) return;

        RemoveUnitFromCells(unit);
        units.Remove(unit);
    }

    /// <summary>
    /// 更新单位从旧格子移动到新格子的占用记录。
    /// </summary>
    public static bool MoveUnit(Unit unit, GridCell fromCell, GridCell toCell)
    {
        if(unit == null) return false;
        if(IsCellOccupied(toCell, unit)) return false;

        if(occupiedUnits.TryGetValue(fromCell, out Unit currentUnit) && currentUnit == unit)
        {
            occupiedUnits.Remove(fromCell);
        }

        occupiedUnits[toCell] = unit;
        if(!units.Contains(unit))
        {
            units.Add(unit);
        }

        return true;
    }

    /// <summary>
    /// 检查指定格子是否已经被其他单位占据。
    /// </summary>
    public static bool IsCellOccupied(GridCell cell, Unit ignoredUnit = null)
    {
        CleanupNullUnits();

        if(!occupiedUnits.TryGetValue(cell, out Unit unit)) return false;
        if(unit == null || !unit.IsAlive)
        {
            occupiedUnits.Remove(cell);
            return false;
        }

        return unit != ignoredUnit;
    }

    /// <summary>
    /// 尝试获得指定格子上的单位。
    /// </summary>
    public static bool TryGetUnit(GridCell cell, out Unit unit)
    {
        CleanupNullUnits();
        return occupiedUnits.TryGetValue(cell, out unit) && unit != null && unit.IsAlive;
    }

    /// <summary>
    /// 获得指定阵营中仍然存活的单位。
    /// </summary>
    public static List<Unit> GetAliveUnits(UnitTeam team)
    {
        CleanupNullUnits();

        List<Unit> teamUnits = new List<Unit>();
        foreach(Unit unit in units)
        {
            if(unit != null && unit.IsAlive && unit.Team == team)
            {
                teamUnits.Add(unit);
            }
        }

        return teamUnits;
    }

    /// <summary>
    /// 重新扫描场景内单位并刷新占用表。
    /// </summary>
    public static void RebuildFromScene()
    {
        RebuildFromScene(GridManager.Instance);
    }

    /// <summary>
    /// 使用指定 GridManager 重新扫描场景内单位并刷新占用表。
    /// </summary>
    public static void RebuildFromScene(GridManager gridManager)
    {
        occupiedUnits.Clear();
        units.Clear();

        if(gridManager == null || !gridManager.IsGridReady()) return;

        Unit[] sceneUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach(Unit unit in sceneUnits)
        {
            if(unit == null || !unit.IsAlive) continue;

            GridCell cell = gridManager.GetCellFromWorldPosition(unit.transform.position);
            RegisterUnit(unit, cell);
        }
    }

    /// <summary>
    /// 清理已经被销毁或死亡的单位记录。
    /// </summary>
    private static void CleanupNullUnits()
    {
        units.RemoveAll(unit => unit == null || !unit.IsAlive);

        List<GridCell> cellsToRemove = new List<GridCell>();
        foreach(KeyValuePair<GridCell, Unit> pair in occupiedUnits)
        {
            if(pair.Value == null || !pair.Value.IsAlive)
            {
                cellsToRemove.Add(pair.Key);
            }
        }

        foreach(GridCell cell in cellsToRemove)
        {
            occupiedUnits.Remove(cell);
        }
    }

    /// <summary>
    /// 移除指定单位已有的全部占用记录。
    /// </summary>
    private static void RemoveUnitFromCells(Unit unit)
    {
        List<GridCell> cellsToRemove = new List<GridCell>();
        foreach(KeyValuePair<GridCell, Unit> pair in occupiedUnits)
        {
            if(pair.Value == unit)
            {
                cellsToRemove.Add(pair.Key);
            }
        }

        foreach(GridCell cell in cellsToRemove)
        {
            occupiedUnits.Remove(cell);
        }
    }
}
