using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 显示玩家 Sword 行动可攻击目标格子的边框高亮。
/// </summary>
public class PlayerAttackRangeHighlighter : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.red;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private int sortingOrder = 110;

    private readonly List<GameObject> highlightObjects = new List<GameObject>();
    private Material lineMaterial;

    /// <summary>
    /// 显示指定单位当前可攻击目标格子。
    /// </summary>
    public void ShowAttackRange(Unit unit)
    {
        ShowAttackRange(unit, new SwordAction());
    }

    /// <summary>
    /// 显示指定攻击行动当前可攻击的目标格子。
    /// </summary>
    public void ShowAttackRange(Unit unit, UnitActionBase attackAction)
    {
        ClearAttackRange();
        if(unit == null || attackAction == null || GridManager.Instance == null) return;

        List<GridCell> targetCells = attackAction.GetTargetCells(unit);
        foreach(GridCell cell in targetCells)
        {
            CreateCellBorder(cell);
        }
    }

    /// <summary>
    /// 清除攻击范围显示。
    /// </summary>
    public void ClearAttackRange()
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
    /// 为指定格子创建攻击边框。
    /// </summary>
    private void CreateCellBorder(GridCell cell)
    {
        GameObject borderObject = new GameObject($"AttackRange_{cell.X}_{cell.Y}");
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
