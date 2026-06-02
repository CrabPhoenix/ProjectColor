using System;
using UnityEngine;

/// <summary>
/// 控制玩家单位的行动菜单、行动选择和行动目标点击。
/// </summary>
public class PlayerUnitActionController : MonoBehaviour
{
    [SerializeField] private PlayerUnitActionMenu actionMenu;
    [SerializeField] private PlayerAttackRangeHighlighter attackRangeHighlighter;
    [SerializeField] private PlayerInteractionRangeHighlighter interactionRangeHighlighter;
    [SerializeField] private PlayerActionCellHighlighter actionCellHighlighter;
    [SerializeField] private PlayerMoveRangeHighlighter moveRangeHighlighter;
    [SerializeField] private GameObject playerUnitPrefab;

    private readonly SwordAction swordAction = new SwordAction();
    private readonly ConvertNeutralAction convertNeutralAction = new ConvertNeutralAction();
    private readonly WaitAction waitAction = new WaitAction();
    private Unit selectedUnit;
    private PlayerUnitActionType selectedAction = PlayerUnitActionType.None;
    private bool subscribedPhaseEvent;

    public bool HasSelectedUnit => selectedUnit != null;
    public event Action OnSelectionCleared;

    /// <summary>
    /// 初始化依赖组件。
    /// </summary>
    private void Awake()
    {
        if(actionMenu == null) actionMenu = GetComponent<PlayerUnitActionMenu>();
        if(actionMenu == null) actionMenu = gameObject.AddComponent<PlayerUnitActionMenu>();
        if(attackRangeHighlighter == null) attackRangeHighlighter = GetComponent<PlayerAttackRangeHighlighter>();
        if(attackRangeHighlighter == null) attackRangeHighlighter = gameObject.AddComponent<PlayerAttackRangeHighlighter>();
        if(interactionRangeHighlighter == null) interactionRangeHighlighter = GetComponent<PlayerInteractionRangeHighlighter>();
        if(interactionRangeHighlighter == null) interactionRangeHighlighter = gameObject.AddComponent<PlayerInteractionRangeHighlighter>();
        if(actionCellHighlighter == null) actionCellHighlighter = GetComponent<PlayerActionCellHighlighter>();
        if(actionCellHighlighter == null) actionCellHighlighter = gameObject.AddComponent<PlayerActionCellHighlighter>();
        if(moveRangeHighlighter == null) moveRangeHighlighter = GetComponent<PlayerMoveRangeHighlighter>();
        ResolvePlayerUnitPrefab();
        convertNeutralAction.SetPlayerUnitPrefab(playerUnitPrefab);
    }

    /// <summary>
    /// 启用时尝试订阅回合阶段事件。
    /// </summary>
    private void OnEnable()
    {
        TrySubscribePhaseEvent();
    }

    /// <summary>
    /// 启动时再次尝试订阅回合阶段事件。
    /// </summary>
    private void Start()
    {
        TrySubscribePhaseEvent();
    }

    /// <summary>
    /// 禁用时取消订阅回合阶段事件。
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
    /// 设置当前选中的玩家单位并在其还能行动时显示菜单。
    /// </summary>
    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        selectedAction = PlayerUnitActionType.None;
        ClearActionHighlights();

        if(selectedUnit == null || !CanSelectedUnitOpenActionMenu())
        {
            actionMenu.Hide();
            return;
        }

        actionMenu.Show(selectedUnit, selectedAction, HandleActionSelected);
    }

    /// <summary>
    /// 清除当前选中的玩家单位和行动 UI。
    /// </summary>
    public void ClearSelectedUnit()
    {
        selectedUnit = null;
        selectedAction = PlayerUnitActionType.None;
        ClearActionHighlights();
        actionMenu.Hide();
    }

    /// <summary>
    /// 临时隐藏当前行动菜单。
    /// </summary>
    public void HideMenu()
    {
        actionMenu.Hide();
    }

    /// <summary>
    /// 在当前单位仍可行动时重新显示行动菜单。
    /// </summary>
    public void RefreshMenu()
    {
        if(selectedUnit == null || !CanSelectedUnitOpenActionMenu())
        {
            actionMenu.Hide();
            return;
        }

        actionMenu.Show(selectedUnit, selectedAction, HandleActionSelected);
    }

    /// <summary>
    /// 处理右键取消输入。
    /// </summary>
    public bool HandleRightClick()
    {
        if(selectedUnit == null) return false;

        if(selectedAction != PlayerUnitActionType.None)
        {
            selectedAction = PlayerUnitActionType.None;
            ClearActionHighlights();
            RestoreMoveRangeIfCanMove();
            actionMenu.SetSelectedAction(selectedAction);
            return true;
        }

        ClearAfterAction();
        return true;
    }

    /// <summary>
    /// 处理玩家在世界中的点击，尝试执行已选攻击或互动。
    /// </summary>
    public bool TryHandleWorldClick(Vector3 worldPosition)
    {
        if(selectedUnit == null || selectedAction == PlayerUnitActionType.None) return false;

        if(selectedAction == PlayerUnitActionType.Sword)
        {
            TryExecuteSword(worldPosition);
            return true;
        }

        if(selectedAction == PlayerUnitActionType.ConvertNeutral)
        {
            TryExecuteConvertNeutral(worldPosition);
            return true;
        }

        if(selectedAction == PlayerUnitActionType.Wait)
        {
            TryConfirmWait(worldPosition);
            return true;
        }

        return selectedAction == PlayerUnitActionType.Attack || selectedAction == PlayerUnitActionType.Interaction;
    }

    /// <summary>
    /// 处理菜单行动选择。
    /// </summary>
    private void HandleActionSelected(PlayerUnitActionType actionType)
    {
        if(selectedUnit == null || !CanSelectedUnitAct()) return;

        if(actionType == PlayerUnitActionType.Attack)
        {
            SelectAttackCategory();
            return;
        }

        if(actionType == PlayerUnitActionType.Sword)
        {
            SelectSwordAction();
            return;
        }

        if(actionType == PlayerUnitActionType.Interaction)
        {
            SelectConvertNeutralAction();
            return;
        }

        if(actionType == PlayerUnitActionType.Wait)
        {
            if(selectedAction == PlayerUnitActionType.Wait)
            {
                ExecuteWait();
            }
            else
            {
                SelectWaitAction();
            }
        }
    }

    /// <summary>
    /// 选择攻击分类并展开二级菜单。
    /// </summary>
    private void SelectAttackCategory()
    {
        selectedAction = PlayerUnitActionType.Attack;
        ClearActionHighlights();
        HideMoveRange();
        actionMenu.SetSelectedAction(selectedAction);
    }

    /// <summary>
    /// 选择 Sword 行动并显示自身格黄色边框与红色攻击范围。
    /// </summary>
    private void SelectSwordAction()
    {
        selectedAction = PlayerUnitActionType.Sword;
        HideMoveRange();
        interactionRangeHighlighter.ClearInteractionRange();

        if(selectedUnit != null)
        {
            selectedUnit.UnitMover.RefreshCurrentCell();
            actionCellHighlighter.ShowAttackCell(selectedUnit.CurrentCell);
        }

        attackRangeHighlighter.ShowAttackRange(selectedUnit);
        actionMenu.SetSelectedAction(selectedAction);
    }

    /// <summary>
    /// 选择转化中立单位互动并显示可转化目标范围。
    /// </summary>
    private void SelectConvertNeutralAction()
    {
        selectedAction = PlayerUnitActionType.ConvertNeutral;
        HideMoveRange();
        attackRangeHighlighter.ClearAttackRange();
        actionCellHighlighter.Clear();
        interactionRangeHighlighter.ShowInteractionRange(selectedUnit, convertNeutralAction);
        actionMenu.SetSelectedAction(PlayerUnitActionType.Interaction);
    }

    /// <summary>
    /// 选择待命行动并高亮当前单位所在格边框。
    /// </summary>
    private void SelectWaitAction()
    {
        selectedAction = PlayerUnitActionType.Wait;
        HideMoveRange();
        attackRangeHighlighter.ClearAttackRange();
        interactionRangeHighlighter.ClearInteractionRange();

        if(selectedUnit != null)
        {
            selectedUnit.UnitMover.RefreshCurrentCell();
            actionCellHighlighter.ShowWaitCell(selectedUnit.CurrentCell);
        }

        actionMenu.SetSelectedAction(selectedAction);
    }

    /// <summary>
    /// 尝试对点击格子上的单位执行 Sword。
    /// </summary>
    private void TryExecuteSword(Vector3 worldPosition)
    {
        if(GridManager.Instance == null) return;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit target)) return;

        if(swordAction.Execute(selectedUnit, target))
        {
            ClearAfterAction();
        }
    }

    /// <summary>
    /// 尝试对点击格子上的中立单位执行转化。
    /// </summary>
    private void TryExecuteConvertNeutral(Vector3 worldPosition)
    {
        if(GridManager.Instance == null) return;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit target)) return;

        if(convertNeutralAction.Execute(selectedUnit, target))
        {
            ClearAfterAction();
        }
    }

    /// <summary>
    /// 点击当前单位所在格时确认待命。
    /// </summary>
    private void TryConfirmWait(Vector3 worldPosition)
    {
        if(GridManager.Instance == null || selectedUnit == null) return;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        selectedUnit.UnitMover.RefreshCurrentCell();
        if(targetCell != selectedUnit.CurrentCell) return;

        ExecuteWait();
    }

    /// <summary>
    /// 执行待命并消耗本阶段行动。
    /// </summary>
    private void ExecuteWait()
    {
        if(waitAction.Execute(selectedUnit, null))
        {
            ClearAfterAction();
        }
    }

    /// <summary>
    /// 隐藏当前移动范围白色高亮。
    /// </summary>
    private void HideMoveRange()
    {
        if(moveRangeHighlighter != null)
        {
            moveRangeHighlighter.ClearMoveRange();
        }
    }

    /// <summary>
    /// 在单位仍可移动时恢复移动范围白色高亮。
    /// </summary>
    private void RestoreMoveRangeIfCanMove()
    {
        if(moveRangeHighlighter == null || selectedUnit == null) return;

        UnitActionState actionState = selectedUnit.GetComponent<UnitActionState>();
        if(actionState != null && !actionState.CanMove) return;

        moveRangeHighlighter.ShowMoveRange(selectedUnit.UnitMover);
    }

    /// <summary>
    /// 清除当前行动范围和确认格高亮。
    /// </summary>
    private void ClearActionHighlights()
    {
        if(attackRangeHighlighter != null)
        {
            attackRangeHighlighter.ClearAttackRange();
        }

        if(interactionRangeHighlighter != null)
        {
            interactionRangeHighlighter.ClearInteractionRange();
        }

        if(actionCellHighlighter != null)
        {
            actionCellHighlighter.Clear();
        }
    }

    /// <summary>
    /// 判断当前选中单位是否还能打开行动菜单。
    /// </summary>
    private bool CanSelectedUnitOpenActionMenu()
    {
        if(selectedUnit == null || !selectedUnit.IsAlive) return false;

        UnitActionState actionState = selectedUnit.GetComponent<UnitActionState>();
        return actionState == null || actionState.CanOpenActionMenu;
    }

    /// <summary>
    /// 判断当前选中单位是否还能行动。
    /// </summary>
    private bool CanSelectedUnitAct()
    {
        if(selectedUnit == null || !selectedUnit.IsAlive) return false;

        UnitActionState actionState = selectedUnit.GetComponent<UnitActionState>();
        return actionState == null || actionState.CanAct;
    }

    /// <summary>
    /// 行动完成后清理选择并通知选择器。
    /// </summary>
    private void ClearAfterAction()
    {
        ClearSelectedUnit();
        OnSelectionCleared?.Invoke();
    }

    /// <summary>
    /// 离开玩家阶段时隐藏所有玩家行动菜单与高亮。
    /// </summary>
    private void HandlePhaseChanged(TurnPhase phase)
    {
        if(phase == TurnPhase.Player) return;

        ClearSelectedUnit();
        OnSelectionCleared?.Invoke();
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

    /// <summary>
    /// 自动查找默认玩家单位 Prefab。
    /// </summary>
    private void ResolvePlayerUnitPrefab()
    {
        if(playerUnitPrefab != null) return;

#if UNITY_EDITOR
        playerUnitPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/PlayerUnit.prefab");
#endif
    }
}
