using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 保存部署阶段中允许摆放玩家单位的格子范围。
/// </summary>
[CreateAssetMenu(fileName = "UnitDeployAreaConfig", menuName = "Unit Deploy/Unit Deploy Area Config")]
public class UnitDeployAreaConfig : ScriptableObject
{
    [SerializeField] private Vector2Int rectangleStartCell = Vector2Int.zero;
    [SerializeField] private Vector2Int rectangleEndCell = Vector2Int.zero;
    [SerializeField] private List<Vector2Int> extraDeployableCells = new List<Vector2Int>();

    public Vector2Int RectangleStartCell => rectangleStartCell;
    public Vector2Int RectangleEndCell => rectangleEndCell;
    public IReadOnlyList<Vector2Int> ExtraDeployableCells => extraDeployableCells;

    /// <summary>
    /// 判断指定格子是否属于允许部署区域。
    /// </summary>
    public bool Contains(GridCell cell)
    {
        if(cell.X < 0 || cell.Y < 0) return false;

        Vector2Int cellPosition = new Vector2Int(cell.X, cell.Y);
        return IsInsideRectangle(cellPosition) || extraDeployableCells.Contains(cellPosition);
    }

    /// <summary>
    /// 获得当前配置在指定网格内覆盖的所有部署格子。
    /// </summary>
    public List<GridCell> GetDeployableCells(GridManager gridManager)
    {
        Normalize();

        List<GridCell> cells = new List<GridCell>();
        for(int x = rectangleStartCell.x; x <= rectangleEndCell.x; x++)
        {
            for(int y = rectangleStartCell.y; y <= rectangleEndCell.y; y++)
            {
                AddCellIfValid(cells, gridManager, new GridCell(x, y));
            }
        }

        foreach(Vector2Int extraCell in extraDeployableCells)
        {
            AddCellIfValid(cells, gridManager, new GridCell(extraCell.x, extraCell.y));
        }

        return cells;
    }

    /// <summary>
    /// 判断指定坐标是否在矩形部署范围内。
    /// </summary>
    public bool IsInsideRectangle(Vector2Int cellPosition)
    {
        return cellPosition.x >= rectangleStartCell.x &&
               cellPosition.x <= rectangleEndCell.x &&
               cellPosition.y >= rectangleStartCell.y &&
               cellPosition.y <= rectangleEndCell.y;
    }

    /// <summary>
    /// 修正坐标顺序、负数坐标和重复的额外部署坐标。
    /// </summary>
    public void Normalize(ISet<Vector2Int> validCells = null)
    {
        rectangleStartCell = ClampNonNegative(rectangleStartCell);
        rectangleEndCell = ClampNonNegative(rectangleEndCell);

        if(validCells != null && validCells.Count > 0)
        {
            rectangleStartCell = FindNearestValidCell(rectangleStartCell, validCells);
            rectangleEndCell = FindNearestValidCell(rectangleEndCell, validCells);
        }

        int minX = Mathf.Min(rectangleStartCell.x, rectangleEndCell.x);
        int minY = Mathf.Min(rectangleStartCell.y, rectangleEndCell.y);
        int maxX = Mathf.Max(rectangleStartCell.x, rectangleEndCell.x);
        int maxY = Mathf.Max(rectangleStartCell.y, rectangleEndCell.y);
        rectangleStartCell = new Vector2Int(minX, minY);
        rectangleEndCell = new Vector2Int(maxX, maxY);

        HashSet<Vector2Int> uniqueCells = new HashSet<Vector2Int>();
        for(int i = extraDeployableCells.Count - 1; i >= 0; i--)
        {
            Vector2Int cellPosition = ClampNonNegative(extraDeployableCells[i]);
            if(validCells != null && validCells.Count > 0 && !validCells.Contains(cellPosition))
            {
                extraDeployableCells.RemoveAt(i);
                continue;
            }

            if(IsInsideRectangle(cellPosition) || !uniqueCells.Add(cellPosition))
            {
                extraDeployableCells.RemoveAt(i);
                continue;
            }

            extraDeployableCells[i] = cellPosition;
        }
    }

    /// <summary>
    /// 在 Inspector 修改时自动进行基础坐标修正。
    /// </summary>
    private void OnValidate()
    {
        Normalize();
    }

    /// <summary>
    /// 如果格子在当前网格内则加入列表。
    /// </summary>
    private void AddCellIfValid(List<GridCell> cells, GridManager gridManager, GridCell cell)
    {
        if(cell.X < 0 || cell.Y < 0) return;
        if(gridManager != null && !gridManager.IsValidCell(cell)) return;
        if(cells.Contains(cell)) return;

        cells.Add(cell);
    }

    /// <summary>
    /// 将坐标修正为非负整数。
    /// </summary>
    private Vector2Int ClampNonNegative(Vector2Int cellPosition)
    {
        return new Vector2Int(Mathf.Max(0, cellPosition.x), Mathf.Max(0, cellPosition.y));
    }

    /// <summary>
    /// 从有效格子集合中查找距离最近的坐标。
    /// </summary>
    private Vector2Int FindNearestValidCell(Vector2Int cellPosition, ISet<Vector2Int> validCells)
    {
        if(validCells.Contains(cellPosition)) return cellPosition;

        Vector2Int nearestCell = cellPosition;
        int nearestDistance = int.MaxValue;
        foreach(Vector2Int validCell in validCells)
        {
            int distance = Mathf.Abs(validCell.x - cellPosition.x) + Mathf.Abs(validCell.y - cellPosition.y);
            if(distance >= nearestDistance) continue;

            nearestCell = validCell;
            nearestDistance = distance;
        }

        return nearestCell;
    }
}
