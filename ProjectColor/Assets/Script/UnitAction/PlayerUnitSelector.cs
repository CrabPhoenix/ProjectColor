using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 负责玩家鼠标选择玩家单位，并协调移动范围、行动菜单和攻击输入。
/// </summary>
public class PlayerUnitSelector : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private PlayerMoveRangeHighlighter moveRangeHighlighter;
    [SerializeField] private PlayerUnitMovementController movementController;
    [SerializeField] private PlayerUnitActionController actionController;
    [SerializeField] private TurnPhaseCameraController cameraController;

    private Unit selectedUnit;

    /// <summary>
    /// 初始化依赖组件。
    /// </summary>
    private void Awake()
    {
        if(targetCamera == null) targetCamera = Camera.main;
        if(moveRangeHighlighter == null) moveRangeHighlighter = GetComponent<PlayerMoveRangeHighlighter>();
        if(moveRangeHighlighter == null) moveRangeHighlighter = gameObject.AddComponent<PlayerMoveRangeHighlighter>();
        if(movementController == null) movementController = GetComponent<PlayerUnitMovementController>();
        if(movementController == null) movementController = gameObject.AddComponent<PlayerUnitMovementController>();
        if(actionController == null) actionController = GetComponent<PlayerUnitActionController>();
        if(actionController == null) actionController = gameObject.AddComponent<PlayerUnitActionController>();
        ResolveCameraController();
        actionController.OnSelectionCleared += ClearSelection;
    }

    /// <summary>
    /// 取消事件订阅。
    /// </summary>
    private void OnDestroy()
    {
        if(actionController != null)
        {
            actionController.OnSelectionCleared -= ClearSelection;
        }
    }

    /// <summary>
    /// 检测鼠标输入并处理选择、移动、行动和取消。
    /// </summary>
    private void Update()
    {
        if(!GameStageManager.IsGameplayActive())
        {
            if(selectedUnit != null)
            {
                ClearSelection();
            }

            return;
        }

        if(TurnManager.Instance != null && TurnManager.Instance.CurrentPhase != TurnPhase.Player) return;
        if(selectedUnit != null && selectedUnit.UnitMover != null && selectedUnit.UnitMover.IsMoving) return;

        if(IsRightClickThisFrame())
        {
            if(actionController != null && actionController.HandleRightClick())
            {
                return;
            }

            if(selectedUnit != null)
            {
                if(!movementController.CanUnitMoveThisPhase(selectedUnit))
                {
                    ClearSelection();
                    return;
                }

                if(actionController != null && actionController.IsMenuVisible)
                {
                    actionController.HideMenuOnly();
                    return;
                }

                ClearSelection();
                return;
            }
        }

        if(!IsLeftClickThisFrame()) return;
        if(IsPointerOverUI()) return;

        Vector3 worldPosition = GetMouseWorldPosition();
        if(TrySwitchToClickedPlayerUnit(worldPosition))
        {
            return;
        }

        if(selectedUnit != null && actionController != null && actionController.TryHandleWorldClick(worldPosition))
        {
            SyncSelectionFromActionController();
            return;
        }

        if(selectedUnit != null && movementController.TryMoveSelectedUnit(worldPosition))
        {
            actionController.HideMenu();
            RefreshMoveRange();
            StartCoroutine(RestoreMenuAfterMovement(selectedUnit));
            return;
        }

        SelectUnitAtPosition(worldPosition);
    }

    /// <summary>
    /// 当点击另一个可操作玩家单位时切换选择。
    /// </summary>
    private bool TrySwitchToClickedPlayerUnit(Vector3 worldPosition)
    {
        if(GridManager.Instance == null) return false;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit unit)) return false;
        if(unit == selectedUnit) return false;
        if(!CanSelectUnit(unit)) return false;

        SelectUnit(unit);
        return true;
    }

    /// <summary>
    /// 根据世界坐标选择玩家单位。
    /// </summary>
    private void SelectUnitAtPosition(Vector3 worldPosition)
    {
        if(GridManager.Instance == null) return;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit unit) || !CanSelectUnit(unit))
        {
            ClearSelection();
            return;
        }

        SelectUnit(unit);
    }

    /// <summary>
    /// 设置当前选中单位并刷新 UI 与摄像机。
    /// </summary>
    private void SelectUnit(Unit unit)
    {
        selectedUnit = unit;
        movementController.SetSelectedUnit(unit);
        actionController.SetSelectedUnit(unit);
        RefreshMoveRange();
        FocusCameraOnUnit(unit);
    }

    /// <summary>
    /// 判断单位是否可以被玩家操作。
    /// </summary>
    private bool CanSelectUnit(Unit unit)
    {
        if(unit == null || !unit.CanPlayerControl || !unit.IsAlive) return false;

        UnitActionState actionState = unit.GetComponent<UnitActionState>();
        return actionState == null || actionState.TurnState != UnitTurnState.Acted;
    }

    /// <summary>
    /// 刷新当前选中单位的移动范围显示。
    /// </summary>
    private void RefreshMoveRange()
    {
        moveRangeHighlighter.ClearMoveRange();
        if(selectedUnit != null && movementController.CanUnitMoveThisPhase(selectedUnit))
        {
            moveRangeHighlighter.ShowMoveRange(selectedUnit.UnitMover);
        }
    }

    /// <summary>
    /// 清除当前选择、行动菜单和移动范围。
    /// </summary>
    private void ClearSelection()
    {
        selectedUnit = null;
        movementController.ClearSelectedUnit();
        moveRangeHighlighter.ClearMoveRange();
        if(actionController != null && actionController.HasSelectedUnit)
        {
            actionController.ClearSelectedUnit();
        }
    }

    /// <summary>
    /// 同步行动控制器清理后的选择状态。
    /// </summary>
    private void SyncSelectionFromActionController()
    {
        if(actionController != null && !actionController.HasSelectedUnit)
        {
            selectedUnit = null;
            movementController.ClearSelectedUnit();
            moveRangeHighlighter.ClearMoveRange();
        }
    }

    /// <summary>
    /// 点击玩家单位时让摄像机立即移动到单位上方。
    /// </summary>
    private void FocusCameraOnUnit(Unit unit)
    {
        ResolveCameraController();
        if(cameraController == null || unit == null) return;

        cameraController.StopFollowing();
        cameraController.SetManualControlEnabled(true);
        cameraController.FocusOnUnit(unit);
    }

    /// <summary>
    /// 等待单位移动结束后恢复行动菜单。
    /// </summary>
    private IEnumerator RestoreMenuAfterMovement(Unit unit)
    {
        UnitMover unitMover = unit != null ? unit.UnitMover : null;
        while(unitMover != null && unitMover.IsMoving)
        {
            yield return null;
        }

        if(selectedUnit == unit && unit != null && unit.IsAlive)
        {
            actionController.RefreshMenu();
        }
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

    /// <summary>
    /// 检查鼠标左键是否在本帧按下。
    /// </summary>
    private bool IsLeftClickThisFrame()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    /// <summary>
    /// 检查鼠标右键是否在本帧按下。
    /// </summary>
    private bool IsRightClickThisFrame()
    {
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
    }

    /// <summary>
    /// 检查鼠标是否正在指向 UI。
    /// </summary>
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// 获得鼠标当前指向的世界坐标。
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        if(targetCamera == null) targetCamera = Camera.main;
        if(targetCamera == null) return Vector3.zero;

        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0;
        return worldPosition;
    }
}
