using UnityEngine;

/// <summary>
/// 显示玩家当前行动确认用的自身格边框高亮。
/// </summary>
public class PlayerActionCellHighlighter : MonoBehaviour
{
    [SerializeField] private Color attackCellColor = Color.yellow;
    [SerializeField] private Color waitCellColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private int sortingOrder = 125;

    private GameObject highlightObject;
    private Material lineMaterial;

    /// <summary>
    /// 显示攻击选择时的自身格黄色边框。
    /// </summary>
    public void ShowAttackCell(GridCell cell)
    {
        ShowCellBorder(cell, attackCellColor, "AttackActorCell");
    }

    /// <summary>
    /// 显示待命确认时的自身格浅蓝色边框。
    /// </summary>
    public void ShowWaitCell(GridCell cell)
    {
        ShowCellBorder(cell, waitCellColor, "WaitActorCell");
    }

    /// <summary>
    /// 清除当前行动确认高亮。
    /// </summary>
    public void Clear()
    {
        if(highlightObject != null)
        {
            Destroy(highlightObject);
            highlightObject = null;
        }
    }

    /// <summary>
    /// 为指定格子创建边框高亮。
    /// </summary>
    private void ShowCellBorder(GridCell cell, Color color, string objectName)
    {
        Clear();
        if(GridManager.Instance == null) return;

        highlightObject = new GameObject($"{objectName}_{cell.X}_{cell.Y}");
        highlightObject.transform.SetParent(transform);

        LineRenderer lineRenderer = highlightObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.material = GetLineMaterial();

        Vector3 center = GridManager.Instance.GetWorldInGrid(cell);
        float halfSize = 0.5f;
        lineRenderer.SetPosition(0, center + new Vector3(-halfSize, -halfSize, 0f));
        lineRenderer.SetPosition(1, center + new Vector3(-halfSize, halfSize, 0f));
        lineRenderer.SetPosition(2, center + new Vector3(halfSize, halfSize, 0f));
        lineRenderer.SetPosition(3, center + new Vector3(halfSize, -halfSize, 0f));
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
