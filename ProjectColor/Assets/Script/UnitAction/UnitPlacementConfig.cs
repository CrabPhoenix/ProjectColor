using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 保存关卡中所有有效格子的单位摆放配置。
/// </summary>
[CreateAssetMenu(fileName = "UnitPlacementConfig", menuName = "Unit/Unit Placement Config")]
public class UnitPlacementConfig : ScriptableObject
{
    [SerializeField] private List<UnitPlacementCell> cells = new List<UnitPlacementCell>();

    public IReadOnlyList<UnitPlacementCell> Cells => cells;

    /// <summary>
    /// 用新的有效格子列表替换当前配置。
    /// </summary>
    public void SetCells(List<UnitPlacementCell> newCells)
    {
        cells = newCells ?? new List<UnitPlacementCell>();
    }

    /// <summary>
    /// 检查指定格子是否已经存在于配置中。
    /// </summary>
    public bool ContainsCell(Vector2Int cellPosition)
    {
        foreach(UnitPlacementCell cell in cells)
        {
            if(cell.CellPosition == cellPosition) return true;
        }

        return false;
    }

    /// <summary>
    /// 尝试获得指定格子的摆放信息。
    /// </summary>
    public bool TryGetCell(Vector2Int cellPosition, out UnitPlacementCell placementCell)
    {
        foreach(UnitPlacementCell cell in cells)
        {
            if(cell.CellPosition == cellPosition)
            {
                placementCell = cell;
                return true;
            }
        }

        placementCell = null;
        return false;
    }

    /// <summary>
    /// 检查配置中是否存在重复的格子坐标。
    /// </summary>
    public bool HasDuplicateCells()
    {
        HashSet<Vector2Int> usedCells = new HashSet<Vector2Int>();
        foreach(UnitPlacementCell cell in cells)
        {
            if(!usedCells.Add(cell.CellPosition)) return true;
        }

        return false;
    }
}

