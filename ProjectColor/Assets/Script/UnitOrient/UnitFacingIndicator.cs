using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 显示当前悬停单位所在格子的正面、侧面和背面边框。
/// </summary>
public class UnitFacingIndicator : MonoBehaviour
{
    [SerializeField] private Color frontColor = Color.green;
    [SerializeField] private Color backColor = Color.red;
    [SerializeField] private Color sideColor = Color.yellow;
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private int sortingOrder = 160;

    private readonly List<GameObject> edgeObjects = new List<GameObject>();
    private Material lineMaterial;
    private Unit currentUnit;
    private GridCell currentCell;
    private Direction currentDirection = Direction.Invalid;

    /// <summary>
    /// 显示指定单位的朝向边框。
    /// </summary>
    public void Show(Unit unit)
    {
        if(unit == null || !unit.IsAlive || unit.Facing == null || GridManager.Instance == null)
        {
            Clear();
            return;
        }

        GridCell cell = unit.CurrentCell;
        Direction frontDirection = unit.Facing.CurrentDirection;
        if(currentUnit == unit && currentCell == cell && currentDirection == frontDirection && edgeObjects.Count > 0) return;

        Clear();
        currentUnit = unit;
        currentCell = cell;
        currentDirection = frontDirection;
        Direction backDirection = UnitFacing.GetOppositeDirection(frontDirection);
        Direction[] sideDirections = UnitFacing.GetSideDirections(frontDirection);

        CreateEdge(cell, frontDirection, frontColor, "Front");
        CreateEdge(cell, backDirection, backColor, "Back");
        CreateEdge(cell, sideDirections[0], sideColor, "SideA");
        CreateEdge(cell, sideDirections[1], sideColor, "SideB");
    }

    /// <summary>
    /// 清理当前显示的所有朝向边框。
    /// </summary>
    public void Clear()
    {
        foreach(GameObject edgeObject in edgeObjects)
        {
            if(edgeObject != null)
            {
                Destroy(edgeObject);
            }
        }

        edgeObjects.Clear();
        currentUnit = null;
        currentCell = default;
        currentDirection = Direction.Invalid;
    }

    /// <summary>
    /// 创建指定方向上的格子边框。
    /// </summary>
    private void CreateEdge(GridCell cell, Direction direction, Color color, string edgeName)
    {
        if(direction == Direction.Invalid) return;

        GameObject edgeObject = new GameObject($"UnitFacing_{edgeName}_{cell.X}_{cell.Y}");
        edgeObject.transform.SetParent(transform);

        LineRenderer lineRenderer = edgeObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.material = GetLineMaterial();

        Vector3 center = GridManager.Instance.GetWorldInGrid(cell);
        GetEdgePositions(center, direction, out Vector3 start, out Vector3 end);
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        edgeObjects.Add(edgeObject);
    }

    /// <summary>
    /// 获得指定边对应的世界坐标起止点。
    /// </summary>
    private void GetEdgePositions(Vector3 center, Direction direction, out Vector3 start, out Vector3 end)
    {
        const float halfSize = 0.5f;
        switch(direction)
        {
            case Direction.Up:
                start = center + new Vector3(-halfSize, halfSize, 0f);
                end = center + new Vector3(halfSize, halfSize, 0f);
                return;
            case Direction.Down:
                start = center + new Vector3(-halfSize, -halfSize, 0f);
                end = center + new Vector3(halfSize, -halfSize, 0f);
                return;
            case Direction.Left:
                start = center + new Vector3(-halfSize, -halfSize, 0f);
                end = center + new Vector3(-halfSize, halfSize, 0f);
                return;
            case Direction.Right:
                start = center + new Vector3(halfSize, -halfSize, 0f);
                end = center + new Vector3(halfSize, halfSize, 0f);
                return;
            default:
                start = center;
                end = center;
                return;
        }
    }

    /// <summary>
    /// 获得朝向边框使用的运行时材质。
    /// </summary>
    private Material GetLineMaterial()
    {
        if(lineMaterial != null) return lineMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        lineMaterial = new Material(shader);
        return lineMaterial;
    }
}
