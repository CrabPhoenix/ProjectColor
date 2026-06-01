using System;
using UnityEngine;

/// <summary>
/// 格子系统，将游戏区域分成离散的格子。负责处理格子的数量，格子的位置，系统中的方向等
/// </summary>
public class GameGrid
{
    private int width;
    private int height;
    private GridObject[,] gridObjects;

    public int Width  => width;
    public int Height => height;
    public GridObject[,] GetGridObjects() => gridObjects;
    

    public GameGrid(int width, int height)
    {
        this.width = width;
        this.height = height;

        gridObjects = new GridObject[width, height];
        InitializeGrid(); 
    }


    private void InitializeGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                GridCell cell_position = new GridCell(x, y);
                gridObjects[x, y] = new GridObject(cell_position); 
            }
        }
    }

    // 检测格子是否有效
    public bool IsValidGrid(GridCell grid_position)
    {
        if(grid_position.X < 0 || grid_position.Y < 0) {return false;}
        if(grid_position.X >= width || grid_position.Y >= height) {return false;}
        return true;
    }

    // 将无效的各自变为最近的有效的格子
    public GridCell ConvertToValidGrid(GridCell grid_position)
    {
        if(IsValidGrid(grid_position)) return grid_position;

        int x = grid_position.X;
        int y = grid_position.Y;

        if(x < 0) x = 0;
        if(x >= Width) x = Width - 1;
        if(y < 0) y = 0;
        if(y >= Width) y = Width - 1;

        return new GridCell(x, y);
    }

    //通过转化过的格子方向确定移动的格子位置
    public GridCell GetNeighborCellPosition(GridCell current_cell, Direction cell_direction)
    {
        GridCell neighbor_cell;
        
        switch (cell_direction)
        {
            case Direction.Up:
                neighbor_cell = new GridCell(current_cell.X, current_cell.Y + 1);
                break;
            case Direction.Down:
                neighbor_cell = new GridCell(current_cell.X, current_cell.Y - 1);
                break;
            case Direction.Left:
                neighbor_cell = new GridCell(current_cell.X - 1, current_cell.Y );
                break;
            case Direction.Right:
                neighbor_cell = new GridCell(current_cell.X + 1, current_cell.Y );
                break;
            default:
                throw new Exception("Invalid neighbor cell position");
        }

        return ConvertToValidGrid(neighbor_cell);
    }

}

/// <summary>
/// 记录格子坐标,类型为Vector2Int
/// </summary>
public readonly struct GridCell
{
    public int X {get; }
    public int Y {get; }

    public GridCell(int x, int y)
    {
        X = x;
        Y = y;
    }
}

/// <summary>
/// grid系统中的方向的枚举 
/// </summary>
public enum Direction { Up, Down, Left, Right, Invalid }