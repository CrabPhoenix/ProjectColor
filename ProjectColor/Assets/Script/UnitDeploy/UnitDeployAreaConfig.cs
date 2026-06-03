using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 保存部署阶段中允许摆放玩家单位的格子坐标。
/// </summary>
[CreateAssetMenu(fileName = "UnitDeployAreaConfig", menuName = "Unit Deploy/Unit Deploy Area Config")]
public class UnitDeployAreaConfig : ScriptableObject
{
    [SerializeField] private bool allowAllValidCells = true;
    [SerializeField] private List<Vector2Int> deployableCells = new List<Vector2Int>();

    public bool AllowAllValidCells => allowAllValidCells;
    public IReadOnlyList<Vector2Int> DeployableCells => deployableCells;

    /// <summary>
    /// 判断指定格子是否属于允许部署区域。
    /// </summary>
    public bool Contains(GridCell cell)
    {
        if(allowAllValidCells) return true;

        Vector2Int cellPosition = new Vector2Int(cell.X, cell.Y);
        return deployableCells.Contains(cellPosition);
    }
}
