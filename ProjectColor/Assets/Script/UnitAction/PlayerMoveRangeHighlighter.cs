using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责显示玩家单位点击后的可移动格子边框高亮。
/// </summary>
public class PlayerMoveRangeHighlighter : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private int sortingOrder = 100;

    private readonly List<GameObject> highlightObjects = new List<GameObject>();
    private Material lineMaterial;

    /// <summary>
    /// 显示指定单位的可移动格子范围。
    /// </summary>
    public void ShowMoveRange(UnitMover unitMover)
    {
        ClearMoveRange();
        if(unitMover == null || GridManager.Instance == null) return;

        List<GridCell> movableCells = unitMover.GetMovableCells(unitMover.MoveRange);
        foreach(GridCell cell in movableCells)
        {
            CreateCellBorder(cell);
        }
    }

    /// <summary>
    /// 清除当前显示的移动范围。
    /// </summary>
    public void ClearMoveRange()
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
    /// 为指定格子创建白色边框。
    /// </summary>
    private void CreateCellBorder(GridCell cell)
    {
        GameObject borderObject = new GameObject($"MoveRange_{cell.X}_{cell.Y}");
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
        lineRenderer.SetPosition(0, center + new Vector3(-halfSize, -halfSize, 0));
        lineRenderer.SetPosition(1, center + new Vector3(-halfSize, halfSize, 0));
        lineRenderer.SetPosition(2, center + new Vector3(halfSize, halfSize, 0));
        lineRenderer.SetPosition(3, center + new Vector3(halfSize, -halfSize, 0));

        highlightObjects.Add(borderObject);
    }

    /// <summary>
    /// 获得高亮边框使用的运行时材质。
    /// </summary>
    private Material GetLineMaterial()
    {
        if(lineMaterial != null) return lineMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        lineMaterial = new Material(shader);
        return lineMaterial;
    }
}
