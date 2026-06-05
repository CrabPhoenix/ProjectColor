using UnityEngine;

/// <summary>
/// GridManager 的公开访问接口，负责把外部系统需要的格子查询统一暴露出来。
/// </summary>
public partial class GridManager
{
    /// <summary>
    /// 检查当前网格数据和坐标转换器是否已经初始化完成。
    /// </summary>
    public bool IsGridReady()
    {
        return grid != null && gridRenderer != null;
    }

    /// <summary>
    /// 根据世界坐标和世界方向获得相邻格子。
    /// </summary>
    public GridCell GetNeighborCell(Vector3 current_position, Vector2 direction)
    {
        GridCell current_cell = gridRenderer.GetCellFromWorldPosition(current_position);
        Direction cell_direction = gridRenderer.GetCellDirectionFromWorld(direction);

        return grid.GetNeighborCellPosition(current_cell, cell_direction);
    }

    /// <summary>
    /// 检查玩家是否能移动到指定方向的邻格。
    /// </summary>
    public bool IsPlayerNeighborCellWalkable(Vector3 current_position, Vector2 move_direction)
    {
        GridCell neighbor_cell = GetNeighborCell(current_position, move_direction);
        return IsCellWalkable(neighbor_cell);
    }

    /// <summary>
    /// 检查 NPC 是否能移动到指定方向的邻格。
    /// </summary>
    public bool IsNPCNeighborCellWalkable(Vector3 current_position, Vector2 move_direction)
    {
        GridCell neighbor_cell = GetNeighborCell(current_position, move_direction);
        return IsCellWalkable(neighbor_cell);
    }

    /// <summary>
    /// 根据世界方向获得邻格中心点的世界坐标。
    /// </summary>
    public Vector3 GetNeighborCellPositionFromWorldDirection(Vector3 current_position, Vector2 direction)
    {
        GridCell neighbor_cell = GetNeighborCell(current_position, direction);
        return gridRenderer.GetCellCenter(neighbor_cell);
    }

    /// <summary>
    /// 获得当前世界坐标所在格子的中心点世界坐标。
    /// </summary>
    public Vector3 GetCurrentCellWorldPosition(Vector3 position)
    {
        GridCell gridCell = gridRenderer.GetCellFromWorldPosition(position);
        return gridRenderer.GetCellCenter(gridCell);
    }

    /// <summary>
    /// 检查物体是否已经沿指定方向经过格子中心点。
    /// </summary>
    public bool HasMoveGridCenterInDirection(Vector2 direction, Vector3 current_position)
    {
        return gridRenderer.HasReachedGridCenterInDirection(direction, current_position);
    }

    /// <summary>
    /// 获得格子 (0, 0) 的世界坐标。
    /// </summary>
    public Vector2 GetStartPosition()
    {
        return gridRenderer.GetWorldPositionFromCell(new GridCell(0, 0));
    }

    /// <summary>
    /// 获得所有格子对象。
    /// </summary>
    public GridObject[,] GetGridObject()
    {
        if(!IsGridReady()) return null;
        return grid.GetGridObjects();
    }

    /// <summary>
    /// 通过格子坐标获得格子中心点的世界坐标。
    /// </summary>
    public Vector3 GetWorldInGrid(GridCell gridCell)
    {
        return gridRenderer.GetCellCenter(gridCell);
    }

    /// <summary>
    /// 将世界坐标转换为当前网格中的格子坐标。
    /// </summary>
    public GridCell GetCellFromWorldPosition(Vector3 position)
    {
        return gridRenderer.GetCellFromWorldPosition(position);
    }

    /// <summary>
    /// 检测格子坐标是否在当前网格范围内。
    /// </summary>
    public bool IsValidCell(GridCell gridCell)
    {
        return grid.IsValidGrid(gridCell);
    }

    /// <summary>
    /// 检测指定格子是否属于单位可以站立的有效格子。
    /// </summary>
    public bool IsCellWalkable(GridCell gridCell)
    {
        if(!IsValidCell(gridCell)) return false;

        GridObject gridObject = grid.GetGridObjects()[gridCell.X, gridCell.Y];
        return gridObject.terrainType == TerrainType.Plate || gridObject.terrainType == TerrainType.Slope;
    }

    /// <summary>
    /// 获得指定格子在指定方向上的邻格，不会自动修正越界结果。
    /// </summary>
    public GridCell GetNeighborCell(GridCell currentCell, Direction direction)
    {
        return grid.GetNeighborCellPositionWithoutClamp(currentCell, direction);
    }

    /// <summary>
    /// 获得指定格子的格子对象。
    /// </summary>
    public GridObject GetGridObject(GridCell gridCell)
    {
        if(!IsValidCell(gridCell)) return null;
        return grid.GetGridObjects()[gridCell.X, gridCell.Y];
    }

    /// <summary>
    /// 获得指定格子当前记录的地形类型。
    /// </summary>
    public TerrainType GetTerrainType(GridCell gridCell)
    {
        GridObject gridObject = GetGridObject(gridCell);
        return gridObject != null ? gridObject.terrainType : TerrainType.None;
    }
}
