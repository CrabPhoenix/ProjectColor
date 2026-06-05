using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 挂在 Grid 物体上，用于在 Scene 视图中观察有效格子的地形识别结果。
/// </summary>
public class TerrainDebug : MonoBehaviour
{
    [SerializeField] private bool showTerrainDebug;
    [SerializeField] private bool showOnlyTerrains;
    [SerializeField] private Color emptyTerrainColor = Color.gray;
    [SerializeField] private Color plateTerrainColor = new Color(0.6f, 0.9f, 1f);
    [SerializeField] private Color waterTerrainColor = Color.blue;
    [SerializeField] private Color slopeTerrainColor = Color.green;

    /// <summary>
    /// 切换地形 Debug 的显示状态。
    /// </summary>
    public void ToggleVisibility()
    {
        showTerrainDebug = !showTerrainDebug;
    }

    /// <summary>
    /// 切换是否只显示存在地形的格子。
    /// </summary>
    public void ToggleOnlyTerrains()
    {
        showOnlyTerrains = !showOnlyTerrains;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在 Scene 视图绘制每个有效格子的地形名称。
    /// </summary>
    private void OnDrawGizmos()
    {
        if(!showTerrainDebug) return;

        GridManager gridManager = GetComponent<GridManager>();
        if(gridManager == null) gridManager = GridManager.Instance;
        if(gridManager == null || !gridManager.IsGridReady()) return;

        GridObject[,] gridObjects = gridManager.GetGridObject();
        if(gridObjects == null) return;

        foreach(GridObject gridObject in gridObjects)
        {
            if(gridObject == null) continue;

            GridCell cell = gridObject.GetCellPosion();
            TerrainType terrainType = gridManager.GetTerrainType(cell);
            if(showOnlyTerrains && terrainType == TerrainType.None) continue;

            Vector3 cellCenter = gridManager.GetWorldInGrid(cell);
            GUIStyle textStyle = new GUIStyle();
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.normal.textColor = GetTerrainColor(terrainType);

            Handles.Label(cellCenter, terrainType.ToString(), textStyle);
        }
    }

    /// <summary>
    /// 根据地形类型获得 Debug 文本颜色。
    /// </summary>
    private Color GetTerrainColor(TerrainType terrainType)
    {
        if(terrainType == TerrainType.Plate) return plateTerrainColor;
        if(terrainType == TerrainType.Water) return waterTerrainColor;
        if(terrainType == TerrainType.Slope) return slopeTerrainColor;
        return emptyTerrainColor;
    }
#endif
}
