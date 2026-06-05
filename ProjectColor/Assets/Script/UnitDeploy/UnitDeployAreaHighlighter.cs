using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在部署阶段显示所有可部署格子的浅红色闪烁填充。
/// </summary>
public class UnitDeployAreaHighlighter : MonoBehaviour
{
    [SerializeField] private Color highlightColor = new Color(1f, 0.2f, 0.2f, 0.35f);
    [SerializeField] private float minAlpha = 0.12f;
    [SerializeField] private float maxAlpha = 0.45f;
    [SerializeField] private float blinkDuration = 2f;
    [SerializeField] private int sortingOrder = 70;

    private readonly Dictionary<GridCell, GameObject> cellObjects = new Dictionary<GridCell, GameObject>();
    private UnitDeployAreaConfig areaConfig;
    private Material highlightMaterial;
    private Mesh quadMesh;
    private Transform highlightRoot;
    private bool isVisible;

    /// <summary>
    /// 隐藏当前场景中所有部署范围高亮，避免部署 UI 被关闭后残留显示。
    /// </summary>
    public static void HideAll()
    {
        UnitDeployAreaHighlighter[] highlighters = Resources.FindObjectsOfTypeAll<UnitDeployAreaHighlighter>();
        foreach(UnitDeployAreaHighlighter highlighter in highlighters)
        {
            if(highlighter == null) continue;
            if(!highlighter.gameObject.scene.IsValid()) continue;

            highlighter.Hide();
        }

        ClearOrphanHighlightRoots();
    }

    /// <summary>
    /// 销毁时清理部署范围高亮根物体。
    /// </summary>
    private void OnDestroy()
    {
        Clear();
        if(highlightRoot != null)
        {
            Destroy(highlightRoot.gameObject);
        }
    }

    /// <summary>
    /// 每帧更新闪烁颜色和占格显示状态。
    /// </summary>
    private void Update()
    {
        if(!isVisible) return;

        RefreshCells();
        UpdateBlinkColor();
    }

    /// <summary>
    /// 显示指定部署范围配置的所有可部署格。
    /// </summary>
    public void Show(UnitDeployAreaConfig config)
    {
        areaConfig = config;
        isVisible = true;
        RefreshCells();
        UpdateBlinkColor();
    }

    /// <summary>
    /// 隐藏并清理所有部署格高亮。
    /// </summary>
    public void Hide()
    {
        isVisible = false;
        Clear();
    }

    /// <summary>
    /// 重新生成当前可部署格显示。
    /// </summary>
    public void RefreshCells()
    {
        if(areaConfig == null || GridManager.Instance == null)
        {
            Clear();
            return;
        }

        HashSet<GridCell> visibleCells = new HashSet<GridCell>();
        foreach(GridCell cell in areaConfig.GetDeployableCells(GridManager.Instance))
        {
            if(!GridManager.Instance.IsCellWalkable(cell)) continue;
            if(UnitGridOccupancy.IsCellOccupied(cell)) continue;

            visibleCells.Add(cell);
            if(cellObjects.TryGetValue(cell, out GameObject existingObject))
            {
                AlignCellObject(existingObject, cell);
                continue;
            }

            cellObjects.Add(cell, CreateCellObject(cell));
        }

        List<GridCell> cellsToRemove = new List<GridCell>();
        foreach(GridCell cell in cellObjects.Keys)
        {
            if(!visibleCells.Contains(cell))
            {
                cellsToRemove.Add(cell);
            }
        }

        foreach(GridCell cell in cellsToRemove)
        {
            Destroy(cellObjects[cell]);
            cellObjects.Remove(cell);
        }
    }

    /// <summary>
    /// 清理当前所有高亮对象。
    /// </summary>
    private void Clear()
    {
        foreach(GameObject cellObject in cellObjects.Values)
        {
            if(cellObject != null)
            {
                Destroy(cellObject);
            }
        }

        cellObjects.Clear();
    }

    /// <summary>
    /// 清理可能因为部署面板被禁用而遗留在场景中的高亮根物体。
    /// </summary>
    private static void ClearOrphanHighlightRoots()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach(GameObject sceneObject in allObjects)
        {
            if(sceneObject == null) continue;
            if(sceneObject.name != "UnitDeployAreaHighlights") continue;
            if(!sceneObject.scene.IsValid()) continue;

            if(Application.isPlaying)
            {
                Destroy(sceneObject);
            }
            else
            {
                DestroyImmediate(sceneObject);
            }
        }
    }

    /// <summary>
    /// 创建单个格子的填充显示对象。
    /// </summary>
    private GameObject CreateCellObject(GridCell cell)
    {
        GameObject cellObject = new GameObject($"DeployArea_{cell.X}_{cell.Y}");
        cellObject.transform.SetParent(GetHighlightRoot(), false);
        AlignCellObject(cellObject, cell);

        MeshFilter meshFilter = cellObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = GetQuadMesh();

        MeshRenderer meshRenderer = cellObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = GetHighlightMaterial();
        meshRenderer.sortingOrder = sortingOrder;

        return cellObject;
    }

    /// <summary>
    /// 将部署范围高亮对象对齐到格子中心的世界坐标。
    /// </summary>
    private void AlignCellObject(GameObject cellObject, GridCell cell)
    {
        if(cellObject == null || GridManager.Instance == null) return;

        Transform cellTransform = cellObject.transform;
        cellTransform.position = GridManager.Instance.GetWorldInGrid(cell);
        cellTransform.rotation = Quaternion.identity;
        cellTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 获得独立的世界空间高亮根物体，避免被 UI 或控制器父物体影响位置。
    /// </summary>
    private Transform GetHighlightRoot()
    {
        if(highlightRoot != null) return highlightRoot;

        GameObject rootObject = new GameObject("UnitDeployAreaHighlights");
        rootObject.transform.position = Vector3.zero;
        rootObject.transform.rotation = Quaternion.identity;
        rootObject.transform.localScale = Vector3.one;
        highlightRoot = rootObject.transform;
        return highlightRoot;
    }

    /// <summary>
    /// 更新共享材质的闪烁透明度。
    /// </summary>
    private void UpdateBlinkColor()
    {
        Material material = GetHighlightMaterial();
        float safeDuration = Mathf.Max(0.01f, blinkDuration);
        float blinkValue = (Mathf.Sin(Time.time / safeDuration * Mathf.PI * 2f) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, blinkValue);
        material.color = new Color(highlightColor.r, highlightColor.g, highlightColor.b, alpha);
    }

    /// <summary>
    /// 获得部署高亮使用的运行时材质。
    /// </summary>
    private Material GetHighlightMaterial()
    {
        if(highlightMaterial != null) return highlightMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        highlightMaterial = new Material(shader);
        return highlightMaterial;
    }

    /// <summary>
    /// 获得单位格子大小的方形网格。
    /// </summary>
    private Mesh GetQuadMesh()
    {
        if(quadMesh != null) return quadMesh;

        quadMesh = new Mesh
        {
            vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f)
            },
            triangles = new[] { 0, 1, 2, 0, 2, 3 },
            uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f)
            }
        };
        quadMesh.RecalculateBounds();
        return quadMesh;
    }
}
