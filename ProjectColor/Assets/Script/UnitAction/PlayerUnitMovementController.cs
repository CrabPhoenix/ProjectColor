using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责玩家选中单位后的点击移动输入和每回合移动次数限制。
/// </summary>
public class PlayerUnitMovementController : MonoBehaviour
{
    [SerializeField] private int moveRange = 2;

    private Unit selectedUnit;
    private readonly HashSet<Unit> movedUnits = new HashSet<Unit>();
    private bool subscribedPhaseEvent;

    /// <summary>
    /// 尝试订阅回合阶段变化事件。
    /// </summary>
    private void OnEnable()
    {
        TrySubscribePhaseEvent();
    }

    /// <summary>
    /// 启动时再次尝试订阅，处理 TurnManager 初始化顺序。
    /// </summary>
    private void Start()
    {
        TrySubscribePhaseEvent();
    }

    /// <summary>
    /// 取消订阅回合阶段变化事件。
    /// </summary>
    private void OnDisable()
    {
        if(TurnManager.Instance != null && subscribedPhaseEvent)
        {
            TurnManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        subscribedPhaseEvent = false;
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
        return unit != null && unit.CanPlayerControl && unit.IsAlive && !movedUnits.Contains(unit);
    }

    /// <summary>
    /// 尝试把当前选中单位移动到鼠标点击的格子。
    /// </summary>
    public bool TryMoveSelectedUnit(Vector3 worldPosition)
    {
        if(selectedUnit == null || GridManager.Instance == null) return false;
        if(!CanUnitMoveThisPhase(selectedUnit)) return false;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        bool moved = selectedUnit.UnitMover.TryMoveToCell(targetCell, moveRange);
        if(moved)
        {
            movedUnits.Add(selectedUnit);
            ClearSelectedUnit();
        }

        return moved;
    }

    /// <summary>
    /// 进入玩家阶段时重置所有玩家单位的移动次数。
    /// </summary>
    private void HandlePhaseChanged(TurnPhase phase)
    {
        if(phase == TurnPhase.Player)
        {
            movedUnits.Clear();
        }
    }

    /// <summary>
    /// 尝试订阅回合阶段变化事件。
    /// </summary>
    private void TrySubscribePhaseEvent()
    {
        if(subscribedPhaseEvent || TurnManager.Instance == null) return;

        TurnManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        subscribedPhaseEvent = true;
    }
}
