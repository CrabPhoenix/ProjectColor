using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责所有阵营单位共用的格子移动逻辑和移动校验。
/// </summary>
public class UnitMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;

    private Unit unit;
    private GridCell currentCell;
    private bool isMoving;

    public GridCell CurrentCell => currentCell;
    public bool IsMoving => isMoving;

    /// <summary>
    /// 缓存单位组件。
    /// </summary>
    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    /// <summary>
    /// 启动时将单位吸附到当前格子中心并注册占用。
    /// </summary>
    private void Start()
    {
        RefreshCurrentCell();
        SnapToCurrentCellCenter();
        UnitGridOccupancy.RegisterUnit(unit, currentCell);
    }

    /// <summary>
    /// 禁用单位时取消其格子占用。
    /// </summary>
    private void OnDisable()
    {
        UnitGridOccupancy.UnregisterUnit(unit);
    }

    /// <summary>
    /// 获得当前单位一格内可以移动到的所有格子。
    /// </summary>
    public List<GridCell> GetMovableCells()
    {
        return GetMovableCells(1);
    }

    /// <summary>
    /// 获得当前单位在指定范围内可以移动到的所有格子。
    /// </summary>
    public List<GridCell> GetMovableCells(int maxMoveDistance)
    {
        RefreshCurrentCell();
        return UnitMovementUtility.GetMovableCells(unit, currentCell, maxMoveDistance);
    }

    /// <summary>
    /// 检查当前单位能否移动到相邻目标格子。
    /// </summary>
    public bool CanMoveToCell(GridCell targetCell)
    {
        return CanMoveToCell(targetCell, 1);
    }

    /// <summary>
    /// 检查当前单位能否移动到指定范围内的目标格子。
    /// </summary>
    public bool CanMoveToCell(GridCell targetCell, int maxMoveDistance)
    {
        if(GridManager.Instance == null) return false;

        RefreshCurrentCell();
        return UnitMovementUtility.IsCellInMoveRange(unit, currentCell, targetCell, maxMoveDistance);
    }

    /// <summary>
    /// 检查当前单位能否沿指定方向移动一格。
    /// </summary>
    public bool CanMove(Direction direction)
    {
        if(GridManager.Instance == null) return false;

        RefreshCurrentCell();
        GridCell targetCell = GridManager.Instance.GetNeighborCell(currentCell, direction);
        return CanMoveToCell(targetCell);
    }

    /// <summary>
    /// 尝试沿指定方向移动一格。
    /// </summary>
    public bool TryMove(Direction direction)
    {
        if(GridManager.Instance == null) return false;

        RefreshCurrentCell();
        GridCell targetCell = GridManager.Instance.GetNeighborCell(currentCell, direction);
        return TryMoveToCell(targetCell);
    }

    /// <summary>
    /// 尝试移动到相邻目标格子。
    /// </summary>
    public bool TryMoveToCell(GridCell targetCell)
    {
        return TryMoveToCell(targetCell, 1);
    }

    /// <summary>
    /// 尝试移动到指定范围内的目标格子。
    /// </summary>
    public bool TryMoveToCell(GridCell targetCell, int maxMoveDistance)
    {
        RefreshCurrentCell();
        if(!CanMoveToCell(targetCell, maxMoveDistance)) return false;
        if(!UnitGridOccupancy.MoveUnit(unit, currentCell, targetCell)) return false;

        currentCell = targetCell;
        StopAllCoroutines();
        StartCoroutine(MoveToPosition(GridManager.Instance.GetWorldInGrid(currentCell)));
        return true;
    }

    /// <summary>
    /// 通过当前位置刷新当前格子坐标。
    /// </summary>
    public void RefreshCurrentCell()
    {
        if(GridManager.Instance == null) return;
        currentCell = GridManager.Instance.GetCellFromWorldPosition(transform.position);
    }

    /// <summary>
    /// 将单位吸附到当前格子中心。
    /// </summary>
    private void SnapToCurrentCellCenter()
    {
        if(GridManager.Instance == null) return;
        transform.position = GridManager.Instance.GetWorldInGrid(currentCell);
    }

    /// <summary>
    /// 将单位平滑移动到目标世界坐标。
    /// </summary>
    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;
        while(Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
