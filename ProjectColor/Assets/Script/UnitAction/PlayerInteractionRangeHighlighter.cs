using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 显示玩家互动行动当前可选目标格子的边框高亮。
/// </summary>
public class PlayerInteractionRangeHighlighter : MonoBehaviour
{
    [SerializeField] private Color highlightColor = new Color(0.35f, 1f, 0.75f, 1f);
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private int sortingOrder = 115;

    private readonly List<GameObject> highlightObjects = new List<GameObject>();
    private Material lineMaterial;

    /// <summary>
    /// 显示指定互动行动当前可选择的目标格子。
    /// </summary>
    public void ShowInteractionRange(Unit unit, UnitActionBase action)
    {
        ClearInteractionRange();
        if(unit == null || action == null || GridManager.Instance == null) return;

        List<GridCell> targetCells = action.GetTargetCells(unit);
        foreach(GridCell cell in targetCells)
        {
            CreateCellBorder(cell);
        }
    }

    /// <summary>
    /// 清除当前互动范围显示。
    /// </summary>
    public void ClearInteractionRange()
    {
        foreach(GameObject highlightObject in highlightObjects)
        {
            if(highlightObject != null)
            {
                Destroy(highlightObject);
            }
        }

        highlightObjects.Clear();
    }

    /// <summary>
    /// 为指定格子创建互动边框高亮。
    /// </summary>
    private void CreateCellBorder(GridCell cell)
    {
        GameObject borderObject = new GameObject($"InteractionRange_{cell.X}_{cell.Y}");
        borderObject.transform.SetParent(transform);

        LineRenderer lineRenderer = borderObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = highlightColor;
        lineRenderer.endColor = highlightColor;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.material = GetLineMaterial();

        Vector3 center = GridManager.Instance.GetWorldInGrid(cell);
        float halfSize = 0.5f;
        lineRenderer.SetPosition(0, center + new Vector3(-halfSize, -halfSize, 0f));
        lineRenderer.SetPosition(1, center + new Vector3(-halfSize, halfSize, 0f));
        lineRenderer.SetPosition(2, center + new Vector3(halfSize, halfSize, 0f));
        lineRenderer.SetPosition(3, center + new Vector3(halfSize, -halfSize, 0f));

        highlightObjects.Add(borderObject);
    }

    /// <summary>
    /// 获得边框高亮使用的运行时材质。
    /// </summary>
    private Material GetLineMaterial()
    {
        if(lineMaterial != null) return lineMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        lineMaterial = new Material(shader);
        return lineMaterial;
    }
}
