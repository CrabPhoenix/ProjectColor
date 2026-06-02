using System;

/// <summary>
/// 格子系统，将游戏区域分成离散的格子，负责处理格子数量、格子位置和格子方向。
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

    /// <summary>
    /// 初始化所有格子对象。
    /// </summary>
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

    /// <summary>
    /// 检测格子坐标是否在网格范围内。
    /// </summary>
    public bool IsValidGrid(GridCell grid_position)
    {
        if(grid_position.X < 0 || grid_position.Y < 0) return false;
        if(grid_position.X >= width || grid_position.Y >= height) return false;
        return true;
    }

    /// <summary>
    /// 将越界格子修正为最近的有效格子。
    /// </summary>
    public GridCell ConvertToValidGrid(GridCell grid_position)
    {
        if(IsValidGrid(grid_position)) return grid_position;

        int x = grid_position.X;
        int y = grid_position.Y;

        if(x < 0) x = 0;
        if(x >= Width) x = Width - 1;
        if(y < 0) y = 0;
        if(y >= Height) y = Height - 1;

        return new GridCell(x, y);
    }

    /// <summary>
    /// 根据方向获得邻格，并把越界结果修正到有效范围内。
    /// </summary>
    public GridCell GetNeighborCellPosition(GridCell current_cell, Direction cell_direction)
    {
        GridCell neighbor_cell = GetNeighborCellPositionWithoutClamp(current_cell, cell_direction);
        return ConvertToValidGrid(neighbor_cell);
    }

    /// <summary>
    /// 根据方向获得邻格，不会把越界结果修正到有效范围内。
    /// </summary>
    public GridCell GetNeighborCellPositionWithoutClamp(GridCell current_cell, Direction cell_direction)
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
                neighbor_cell = new GridCell(current_cell.X - 1, current_cell.Y);
                break;
            case Direction.Right:
                neighbor_cell = new GridCell(current_cell.X + 1, current_cell.Y);
                break;
            default:
                throw new Exception("Invalid neighbor cell position");
        }

        return neighbor_cell;
    }
}

/// <summary>
/// 记录格子坐标，类型类似 Vector2Int。
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

    /// <summary>
    /// 判断两个格子坐标是否相同。
    /// </summary>
    public override bool Equals(object obj)
    {
        if(!(obj is GridCell other)) return false;
        return X == other.X && Y == other.Y;
    }

    /// <summary>
    /// 获得格子坐标的哈希值。
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    /// <summary>
    /// 将格子坐标转为可读文本。
    /// </summary>
    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static bool operator ==(GridCell left, GridCell right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridCell left, GridCell right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Grid 系统中的四方向枚举。
/// </summary>
public enum Direction { Up, Down, Left, Right, Invalid }
