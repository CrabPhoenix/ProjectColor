using UnityEngine;

/// <summary>
/// 在玩家选择攻击目标时闪烁显示受击单位将被攻击的格子边。
/// </summary>
public class UnitAttackDirectionPreview : MonoBehaviour
{
    [SerializeField] private Color frontColor = Color.green;
    [SerializeField] private Color sideColor = Color.yellow;
    [SerializeField] private Color backColor = Color.red;
    [SerializeField] private float minAlpha = 0.05f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float blinkDuration = 2f;
    [SerializeField] private float lineWidth = 0.12f;
    [SerializeField] private int sortingOrder = 260;

    private GameObject previewObject;
    private LineRenderer lineRenderer;
    private Material lineMaterial;
    private Unit currentAttacker;
    private Unit currentTarget;
    private Direction currentIncomingDirection = Direction.Invalid;
    private UnitFacingSide currentFacingSide = UnitFacingSide.Side;

    /// <summary>
    /// 每帧更新闪烁透明度。
    /// </summary>
    private void Update()
    {
        if(previewObject == null || !previewObject.activeSelf || lineRenderer == null) return;

        float safeDuration = Mathf.Max(0.01f, blinkDuration);
        float blinkValue = (Mathf.Sin(Time.time / safeDuration * Mathf.PI * 2f) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, blinkValue);
        Color baseColor = GetColor(currentFacingSide);
        Color color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    /// <summary>
    /// 显示攻击者当前将命中的目标格边。
    /// </summary>
    public void Show(Unit attacker, Unit target)
    {
        if(attacker == null || target == null || target.Facing == null || GridManager.Instance == null || !target.IsAlive)
        {
            Clear();
            return;
        }

        Direction incomingDirection = target.Facing.GetIncomingDirection(attacker);
        UnitFacingSide facingSide = target.Facing.GetSideFromIncomingDirection(incomingDirection);
        if(incomingDirection == Direction.Invalid)
        {
            Clear();
            return;
        }

        if(currentAttacker == attacker && currentTarget == target && currentIncomingDirection == incomingDirection && currentFacingSide == facingSide && previewObject != null)
        {
            previewObject.SetActive(true);
            return;
        }

        EnsurePreviewObject();
        currentAttacker = attacker;
        currentTarget = target;
        currentIncomingDirection = incomingDirection;
        currentFacingSide = facingSide;

        target.UnitMover.RefreshCurrentCell();
        Vector3 center = GridManager.Instance.GetWorldInGrid(target.CurrentCell);
        GetEdgePositions(center, incomingDirection, out Vector3 start, out Vector3 end);
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        previewObject.SetActive(true);
    }

    /// <summary>
    /// 清理当前攻击方向预览。
    /// </summary>
    public void Clear()
    {
        currentAttacker = null;
        currentTarget = null;
        currentIncomingDirection = Direction.Invalid;
        currentFacingSide = UnitFacingSide.Side;
        if(previewObject != null)
        {
            previewObject.SetActive(false);
        }
    }

    /// <summary>
    /// 创建预览边对象。
    /// </summary>
    private void EnsurePreviewObject()
    {
        if(previewObject != null) return;

        previewObject = new GameObject("UnitAttackDirectionPreview");
        previewObject.transform.SetParent(transform);

        lineRenderer = previewObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.material = GetLineMaterial();
        previewObject.SetActive(false);
    }

    /// <summary>
    /// 获得指定方向对应的格子边起止点。
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
    /// 获得预览边使用的运行时材质。
    /// </summary>
    private Material GetLineMaterial()
    {
        if(lineMaterial != null) return lineMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        lineMaterial = new Material(shader);
        return lineMaterial;
    }

    /// <summary>
    /// 根据受击方位获得预览颜色。
    /// </summary>
    private Color GetColor(UnitFacingSide facingSide)
    {
        switch(facingSide)
        {
            case UnitFacingSide.Front:
                return frontColor;
            case UnitFacingSide.Back:
                return backColor;
            default:
                return sideColor;
        }
    }
}
