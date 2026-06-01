using System;
using UnityEngine;

/// <summary>
/// 负责格子坐标轴与世界坐标轴之间的转化。坐标，尺寸，方向等
/// </summary>
public class GridRenderer 
{
    private Vector3 origin;
    private float gridSize = 1.0f;
    private GameGrid grid;

    public float GridSize => gridSize;

    public GridRenderer(Vector3 origin, GameGrid grid)
    {
        this.origin = origin;
        this.grid = grid;
    }

    //将格子坐标转化为其在世界的位置
    public Vector3 GetWorldPositionFromCell(GridCell cell)
    {
        return new Vector3(cell.X, cell.Y, 0) + origin;
    }

    //将玩家输入的方向改为格子坐标的四向移动
    public Direction GetCellDirectionFromWorld(Vector2 direction)
    {
        
        if(direction == Vector2.up) {return Direction.Up;}
        if(direction == Vector2.down) {return Direction.Down;}
        if(direction == Vector2.left) {return Direction.Left;}
        if(direction == Vector2.right) {return Direction.Right;}

        return Direction.Invalid;
    }

    //将世界坐标消除原点的偏移
    public Vector3 GetWorldPositionInGridCoordinate(Vector3 world_position)
    {
        return world_position - origin;
    }

    //将世界坐标转化为格子坐标
    public GridCell GetCellFromWorldPosition(Vector3 position)
    {
        Vector3 originReverted_position = position - origin;
        var handled_cell = new GridCell(Mathf.FloorToInt(originReverted_position.x), Mathf.FloorToInt(originReverted_position.y));

        return grid.ConvertToValidGrid(handled_cell);
    }

    //通过格子坐标获得其格子的中心点的世界坐标
    public Vector3 GetCellCenter(GridCell cell)
    {
        return GetWorldPositionFromCell(cell) + new Vector3(gridSize/2, gridSize/2);
    }

    //检测是否移动到了目标格子的中心点
    public bool HasReachedGridCenterInDirection(Vector2 direction, Vector3 current_position)
    {
        Direction dir = GetCellDirectionFromWorld(direction);
        Vector3 postion_inGridCoordinate = GetWorldPositionInGridCoordinate(current_position);
        float distance_from_center_x = postion_inGridCoordinate.x - MathF.Floor(postion_inGridCoordinate.x);
        float distance_from_center_y = postion_inGridCoordinate.y - MathF.Floor(postion_inGridCoordinate.y);

        switch (dir)
        {
            case Direction.Up: return distance_from_center_y >=  gridSize / 2;
            case Direction.Down: return distance_from_center_y <=  gridSize / 2;
            case Direction.Left: return distance_from_center_x <=  gridSize / 2;
            case Direction.Right: return distance_from_center_x >=  gridSize / 2;
                    
        }

        return false; 
    }
}
