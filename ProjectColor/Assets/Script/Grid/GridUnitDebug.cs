using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 挂在 Grid 组件所在物体上，用于在 Scene 视图显示有效格子的单位占用情况。
/// </summary>
public class GridUnitDebug : MonoBehaviour
{
    [SerializeField] private bool showUnitDebug;
    [SerializeField] private bool showOnlyOccupiedCells;
    [SerializeField] private Color emptyCellColor = Color.gray;
    [SerializeField] private Color occupiedCellColor = Color.green;

    /// <summary>
    /// 切换单位占用 Debug 的显示状态。
    /// </summary>
    public void ToggleVisibility()
    {
        showUnitDebug = !showUnitDebug;
    }

    /// <summary>
    /// 切换是否只显示已经被单位占用的格子。
    /// </summary>
    public void ToggleOnlyOccupiedCells()
    {
        showOnlyOccupiedCells = !showOnlyOccupiedCells;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在 Scene 视图绘制每个有效格子的占用文本。
    /// </summary>
    private void OnDrawGizmos()
    {
        if(!showUnitDebug) return;

        GridManager gridManager = GetComponent<GridManager>();
        if(gridManager == null) gridManager = GridManager.Instance;
        if(gridManager == null || !gridManager.IsGridReady()) return;

        GridObject[,] gridObjects = gridManager.GetGridObject();
        if(gridObjects == null) return;

        UnitGridOccupancy.RebuildFromScene(gridManager);

        foreach(GridObject gridObject in gridObjects)
        {
            if(gridObject == null) continue;
            GridCell cell = gridObject.GetCellPosion();
            if(!gridManager.IsCellWalkable(cell)) continue;

            bool hasUnit = UnitGridOccupancy.TryGetUnit(cell, out Unit unit);
            if(showOnlyOccupiedCells && !hasUnit) continue;

            Vector3 cellCenter = gridManager.GetWorldInGrid(cell);
            GUIStyle textStyle = new GUIStyle();
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.normal.textColor = hasUnit ? occupiedCellColor : emptyCellColor;

            string label = hasUnit ? $"{unit.name}\n{unit.Team}" : "Empty";
            Handles.Label(cellCenter, label, textStyle);
        }
    }
#endif
}
