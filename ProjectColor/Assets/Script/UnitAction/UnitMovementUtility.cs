using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 提供所有单位共用的移动方向、移动范围和格子校验方法。
/// </summary>
public static class UnitMovementUtility
{
    public static readonly Direction[] FourDirections =
    {
        Direction.Up,
        Direction.Down,
        Direction.Left,
        Direction.Right
    };

    /// <summary>
    /// 判断目标格子是否可以被单位移动进入。
    /// </summary>
    public static bool CanUnitMoveToCell(Unit unit, GridCell targetCell)
    {
        if(unit == null || GridManager.Instance == null) return false;
        if(!GridManager.Instance.IsCellWalkable(targetCell)) return false;
        if(UnitGridOccupancy.IsCellOccupied(targetCell, unit)) return false;

        return true;
    }

    /// <summary>
    /// 获得单位一格内可以移动到的格子。
    /// </summary>
    public static List<GridCell> GetMovableCells(Unit unit, GridCell currentCell)
    {
        return GetMovableCells(unit, currentCell, 1);
    }

    /// <summary>
    /// 获得单位在指定步数范围内可以移动到的格子。
    /// </summary>
    public static List<GridCell> GetMovableCells(Unit unit, GridCell currentCell, int maxMoveDistance)
    {
        List<GridCell> movableCells = new List<GridCell>();
        if(unit == null || GridManager.Instance == null) return movableCells;
        if(maxMoveDistance <= 0) return movableCells;

        Queue<(GridCell cell, int distance)> cellsToCheck = new Queue<(GridCell cell, int distance)>();
        HashSet<GridCell> checkedCells = new HashSet<GridCell>();
        cellsToCheck.Enqueue((currentCell, 0));
        checkedCells.Add(currentCell);

        while(cellsToCheck.Count > 0)
        {
            (GridCell cell, int distance) current = cellsToCheck.Dequeue();
            if(current.distance >= maxMoveDistance) continue;

            foreach(Direction direction in FourDirections)
            {
                GridCell targetCell = GridManager.Instance.GetNeighborCell(current.cell, direction);
                if(checkedCells.Contains(targetCell)) continue;
                checkedCells.Add(targetCell);

                if(!CanUnitMoveToCell(unit, targetCell)) continue;

                movableCells.Add(targetCell);
                cellsToCheck.Enqueue((targetCell, current.distance + 1));
            }
        }

        return movableCells;
    }

    /// <summary>
    /// 检查目标格子是否在单位指定步数的可移动范围内。
    /// </summary>
    public static bool IsCellInMoveRange(Unit unit, GridCell currentCell, GridCell targetCell, int maxMoveDistance)
    {
        return TryGetMovePath(unit, currentCell, targetCell, maxMoveDistance, out _);
    }

    /// <summary>
    /// 使用四方向 BFS 寻找从当前格到目标格的移动路径，路径不包含起点。
    /// </summary>
    public static bool TryGetMovePath(Unit unit, GridCell currentCell, GridCell targetCell, int maxMoveDistance, out List<GridCell> path)
    {
        path = new List<GridCell>();
        if(unit == null || GridManager.Instance == null) return false;
        if(maxMoveDistance <= 0) return false;
        if(currentCell == targetCell) return false;

        Queue<(GridCell cell, int distance)> cellsToCheck = new Queue<(GridCell cell, int distance)>();
        Dictionary<GridCell, GridCell> previousCells = new Dictionary<GridCell, GridCell>();
        HashSet<GridCell> checkedCells = new HashSet<GridCell>();
        cellsToCheck.Enqueue((currentCell, 0));
        checkedCells.Add(currentCell);

        while(cellsToCheck.Count > 0)
        {
            (GridCell cell, int distance) current = cellsToCheck.Dequeue();
            if(current.distance >= maxMoveDistance) continue;

            foreach(Direction direction in FourDirections)
            {
                GridCell neighborCell = GridManager.Instance.GetNeighborCell(current.cell, direction);
                if(checkedCells.Contains(neighborCell)) continue;
                checkedCells.Add(neighborCell);

                if(!CanUnitMoveToCell(unit, neighborCell)) continue;

                previousCells[neighborCell] = current.cell;
                if(neighborCell == targetCell)
                {
                    path = BuildPath(currentCell, targetCell, previousCells);
                    return true;
                }

                cellsToCheck.Enqueue((neighborCell, current.distance + 1));
            }
        }

        return false;
    }

    /// <summary>
    /// 根据 BFS 的前置格记录还原移动路径。
    /// </summary>
    private static List<GridCell> BuildPath(GridCell startCell, GridCell targetCell, Dictionary<GridCell, GridCell> previousCells)
    {
        List<GridCell> path = new List<GridCell>();
        GridCell currentCell = targetCell;

        while(currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = previousCells[currentCell];
        }

        path.Reverse();
        return path;
    }
}
