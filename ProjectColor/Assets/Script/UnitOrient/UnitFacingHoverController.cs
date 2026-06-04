using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 检测鼠标当前悬停的单位，并在悬停期间显示单位朝向。
/// </summary>
public class UnitFacingHoverController : MonoBehaviour
{
    private static UnitFacingHoverController instance;
    private static bool isSuppressed;

    [SerializeField] private Camera targetCamera;
    [SerializeField] private UnitFacingIndicator indicator;

    private Unit currentHoverUnit;

    /// <summary>
    /// 设置普通朝向悬停显示是否被其他系统临时压制。
    /// </summary>
    public static void SetSuppressed(bool suppressed)
    {
        isSuppressed = suppressed;
        if(isSuppressed && instance != null)
        {
            instance.ClearHover();
        }
    }

    /// <summary>
    /// 确保场景中存在一个朝向悬停控制器。
    /// </summary>
    public static void EnsureExists()
    {
        if(instance != null) return;
        if(FindFirstObjectByType<UnitFacingHoverController>() != null) return;

        GameObject controllerObject = new GameObject("UnitFacingHoverController");
        controllerObject.AddComponent<UnitFacingHoverController>();
    }

    /// <summary>
    /// 初始化单例和显示组件。
    /// </summary>
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ResolveReferences();
    }

    /// <summary>
    /// 每帧刷新鼠标悬停单位。
    /// </summary>
    private void Update()
    {
        ResolveReferences();

        if(!CanShowFacing() || !TryGetHoverUnit(out Unit unit))
        {
            ClearHover();
            return;
        }

        if(unit == currentHoverUnit)
        {
            indicator?.Show(unit);
            return;
        }

        currentHoverUnit = unit;
        indicator?.Show(currentHoverUnit);
    }

    /// <summary>
    /// 清理当前悬停显示。
    /// </summary>
    private void ClearHover()
    {
        currentHoverUnit = null;
        indicator?.Clear();
    }

    /// <summary>
    /// 判断当前阶段和输入状态是否允许显示朝向。
    /// </summary>
    private bool CanShowFacing()
    {
        if(isSuppressed) return false;
        if(Mouse.current == null || targetCamera == null || GridManager.Instance == null) return false;
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return false;

        GameStageManager stageManager = FindFirstObjectByType<GameStageManager>();
        if(stageManager == null) return true;

        return stageManager.CurrentStage == GameStage.Deployment || stageManager.CurrentStage == GameStage.Gameplay;
    }

    /// <summary>
    /// 尝试获得鼠标当前悬停的存活单位。
    /// </summary>
    private bool TryGetHoverUnit(out Unit unit)
    {
        unit = null;
        Vector3 worldPosition = GetMouseWorldPosition();

        if(TryGetUnitFromCollider(worldPosition, out unit)) return true;
        if(TryGetUnitFromGrid(worldPosition, out unit)) return true;

        return false;
    }

    /// <summary>
    /// 优先通过单位碰撞体获得悬停单位。
    /// </summary>
    private bool TryGetUnitFromCollider(Vector3 worldPosition, out Unit unit)
    {
        unit = null;

        Collider2D collider2D = Physics2D.OverlapPoint(worldPosition);
        if(collider2D != null && TryResolveAliveUnit(collider2D.gameObject, out unit)) return true;

        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(ray, out RaycastHit hit) && TryResolveAliveUnit(hit.collider.gameObject, out unit)) return true;

        return false;
    }

    /// <summary>
    /// 在没有碰撞体时通过当前鼠标所在格子的占用表获得单位。
    /// </summary>
    private bool TryGetUnitFromGrid(Vector3 worldPosition, out Unit unit)
    {
        unit = null;
        GridCell cell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!GridManager.Instance.IsValidCell(cell)) return false;

        Vector3 cellCenter = GridManager.Instance.GetWorldInGrid(cell);
        if(Mathf.Abs(worldPosition.x - cellCenter.x) > 0.5f || Mathf.Abs(worldPosition.y - cellCenter.y) > 0.5f) return false;

        return UnitGridOccupancy.TryGetUnit(cell, out unit) && unit != null && unit.IsAlive;
    }

    /// <summary>
    /// 从对象或父对象中解析存活单位。
    /// </summary>
    private bool TryResolveAliveUnit(GameObject targetObject, out Unit unit)
    {
        unit = targetObject != null ? targetObject.GetComponentInParent<Unit>() : null;
        return unit != null && unit.IsAlive && unit.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// 获得鼠标当前指向的世界坐标。
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    /// <summary>
    /// 查找或创建悬停显示所需引用。
    /// </summary>
    private void ResolveReferences()
    {
        if(targetCamera == null) targetCamera = Camera.main;
        if(indicator == null) indicator = GetComponent<UnitFacingIndicator>();
        if(indicator == null) indicator = gameObject.AddComponent<UnitFacingIndicator>();
    }
}
