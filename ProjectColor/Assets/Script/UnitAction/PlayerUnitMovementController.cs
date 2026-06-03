using System.Collections;
using UnityEngine;

/// <summary>
/// 负责玩家选中单位后的点击移动输入和每阶段移动状态限制。
/// </summary>
public class PlayerUnitMovementController : MonoBehaviour
{
    [SerializeField] private TurnPhaseCameraController cameraController;

    private Unit selectedUnit;
    private Coroutine cameraFollowCoroutine;

    /// <summary>
    /// 初始化摄像机控制器引用。
    /// </summary>
    private void Awake()
    {
        ResolveCameraController();
    }

    /// <summary>
    /// 设置当前准备移动的玩家单位。
    /// </summary>
    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
    }

    /// <summary>
    /// 清除当前选中的玩家单位。
    /// </summary>
    public void ClearSelectedUnit()
    {
        selectedUnit = null;
    }

    /// <summary>
    /// 判断指定玩家单位本阶段是否还可以移动。
    /// </summary>
    public bool CanUnitMoveThisPhase(Unit unit)
    {
        if(unit == null || !unit.CanPlayerControl || !unit.IsAlive) return false;

        UnitActionState actionState = unit.GetComponent<UnitActionState>();
        return actionState == null || actionState.CanMove;
    }

    /// <summary>
    /// 尝试把当前选中单位移动到鼠标点击的格子。
    /// </summary>
    public bool TryMoveSelectedUnit(Vector3 worldPosition)
    {
        if(selectedUnit == null || GridManager.Instance == null) return false;
        if(!CanUnitMoveThisPhase(selectedUnit)) return false;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        bool moved = selectedUnit.UnitMover.TryMoveToCell(targetCell, selectedUnit.UnitMover.MoveRange);
        if(!moved) return false;

        UnitActionState actionState = selectedUnit.GetComponent<UnitActionState>();
        if(actionState != null)
        {
            actionState.MarkMoved();
        }

        FollowCameraWhileMoving(selectedUnit);
        return true;
    }

    /// <summary>
    /// 在单位移动期间让摄像机跟随单位并关闭玩家手动控制。
    /// </summary>
    private void FollowCameraWhileMoving(Unit unit)
    {
        if(unit == null) return;

        ResolveCameraController();
        if(cameraController == null) return;

        if(cameraFollowCoroutine != null)
        {
            StopCoroutine(cameraFollowCoroutine);
        }

        cameraController.FollowUnit(unit);
        cameraFollowCoroutine = StartCoroutine(StopCameraFollowAfterMove(unit));
    }

    /// <summary>
    /// 等待单位移动结束后恢复玩家阶段的摄像机手动控制。
    /// </summary>
    private IEnumerator StopCameraFollowAfterMove(Unit unit)
    {
        UnitMover unitMover = unit != null ? unit.UnitMover : null;
        while(unitMover != null && unitMover.IsMoving)
        {
            yield return null;
        }

        if(cameraController != null && TurnManager.Instance != null && TurnManager.Instance.CurrentPhase == TurnPhase.Player)
        {
            cameraController.StopFollowing();
            cameraController.SetManualControlEnabled(true);
        }

        cameraFollowCoroutine = null;
    }

    /// <summary>
    /// 查找或创建回合摄像机控制器。
    /// </summary>
    private void ResolveCameraController()
    {
        if(cameraController != null) return;

        cameraController = FindFirstObjectByType<TurnPhaseCameraController>();
        if(cameraController != null) return;

        Camera mainCamera = Camera.main;
        if(mainCamera == null) return;

        cameraController = mainCamera.GetComponent<TurnPhaseCameraController>();
        if(cameraController == null)
        {
            cameraController = mainCamera.gameObject.AddComponent<TurnPhaseCameraController>();
        }
    }
}
