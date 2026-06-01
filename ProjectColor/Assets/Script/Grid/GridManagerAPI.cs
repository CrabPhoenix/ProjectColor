using UnityEngine;

public partial class GridManager
{
    //该方法先找到当前所处的位置的格子坐标，将玩家输入方向改为格子方向，最后获得邻格坐标
    public GridCell GetNeighborCell(Vector3 current_position, Vector2 direction)
    {
        GridCell current_cell = gridRenderer.GetCellFromWorldPosition(current_position);
        Direction cell_direction = gridRenderer.GetCellDirectionFromWorld(direction);

        return grid.GetNeighborCellPosition(current_cell, cell_direction);
    }

    //检查玩家是否能移动到邻格
    public bool IsPlayerNeighborCellWalkable(Vector3 current_position, Vector2 move_direction)
    {
        
        GridCell neighbor_cell = GetNeighborCell(current_position, move_direction);
        GridObject gridObject = grid.GetGridObjects()[neighbor_cell.X, neighbor_cell.Y];
        return gridObject.type == GridObjectType.Path;
    }

    //检查NPC是否能移动到邻格
    public bool IsNPCNeighborCellWalkable(Vector3 current_position, Vector2 move_direction)
    {
        
        GridCell neighbor_cell = GetNeighborCell(current_position, move_direction);
        GridObject gridObject = grid.GetGridObjects()[neighbor_cell.X, neighbor_cell.Y];
        return gridObject.type == GridObjectType.Path || gridObject.type == GridObjectType.Chamber;
    }

    //获得邻格坐标并返回到格子中心位置的世界坐标
    public Vector3 GetNeighborCellPositionFromWorldDirection(Vector3 current_position, Vector2 direction)
    {
        
        GridCell neighbor_cell = GetNeighborCell(current_position, direction);
        return gridRenderer.GetCellCenter(neighbor_cell);
    }

    //通过当前物体的世界坐标获得所处格子的中心的世界坐标
    public Vector3 GetCurrentCellWorldPosition(Vector3 position)
    {
        GridCell gridCell = gridRenderer.GetCellFromWorldPosition(position);
        return gridRenderer.GetCellCenter(gridCell);
    }


    public bool HasMoveGridCenterInDirection(Vector2 direction, Vector3 current_position)
    {
        return gridRenderer.HasReachedGridCenterInDirection(direction, current_position);
    }

    //获得格子(0, 0)在世界坐标的位置
    public Vector2 GetStartPosition()
    {
        return gridRenderer.GetWorldPositionFromCell(new GridCell(0 ,0));
    }

    public GridObject[,] GetGridObject()
    {
        return grid.GetGridObjects();
    }

    //通过Cell获得world坐标
    public Vector3 GetWorldInGrid(GridCell gridCell)
    {
        return gridRenderer.GetCellCenter(gridCell);
    }
}
