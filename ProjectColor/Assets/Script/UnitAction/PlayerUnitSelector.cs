using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 负责玩家鼠标左键选择玩家单位，并触发移动范围显示和移动控制。
/// </summary>
public class PlayerUnitSelector : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private PlayerMoveRangeHighlighter moveRangeHighlighter;
    [SerializeField] private PlayerUnitMovementController movementController;

    private Unit selectedUnit;

    /// <summary>
    /// 初始化依赖组件。
    /// </summary>
    private void Awake()
    {
        if(targetCamera == null) targetCamera = Camera.main;
        if(moveRangeHighlighter == null) moveRangeHighlighter = GetComponent<PlayerMoveRangeHighlighter>();
        if(movementController == null) movementController = GetComponent<PlayerUnitMovementController>();
    }

    /// <summary>
    /// 检测鼠标左键输入并处理选择或移动。
    /// </summary>
    private void Update()
    {
        if(!IsLeftClickThisFrame()) return;
        if(IsPointerOverUI()) return;
        if(TurnManager.Instance != null && TurnManager.Instance.CurrentPhase != TurnPhase.Player) return;

        Vector3 worldPosition = GetMouseWorldPosition();
        if(selectedUnit != null && movementController.TryMoveSelectedUnit(worldPosition))
        {
            ClearSelection();
            return;
        }

        SelectUnitAtPosition(worldPosition);
    }

    /// <summary>
    /// 根据世界坐标选择玩家单位。
    /// </summary>
    private void SelectUnitAtPosition(Vector3 worldPosition)
    {
        if(GridManager.Instance == null) return;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit unit) || !movementController.CanUnitMoveThisPhase(unit))
        {
            ClearSelection();
            return;
        }

        selectedUnit = unit;
        movementController.SetSelectedUnit(unit);
        moveRangeHighlighter.ShowMoveRange(unit.UnitMover);
    }

    /// <summary>
    /// 清除当前选择和移动范围显示。
    /// </summary>
    private void ClearSelection()
    {
        selectedUnit = null;
        movementController.ClearSelectedUnit();
        moveRangeHighlighter.ClearMoveRange();
    }

    /// <summary>
    /// 检查鼠标左键是否在本帧按下。
    /// </summary>
    private bool IsLeftClickThisFrame()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    /// <summary>
    /// 检查鼠标是否正指向 UI，避免点击按钮时同时选中单位。
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
