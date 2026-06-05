using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
    [SerializeField] private UnitAttackDirectionPreview attackDirectionPreview;
    [SerializeField] private CombatPreviewUI combatPreviewUI;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private GameObject playerUnitPrefab;

    private readonly SwordAction swordAction = new SwordAction();
    private readonly BowAction bowAction = new BowAction();
    private readonly ConvertNeutralAction convertNeutralAction = new ConvertNeutralAction();
    private readonly WaitAction waitAction = new WaitAction();
    private Unit selectedUnit;
    private PlayerUnitActionType selectedAction = PlayerUnitActionType.None;
    private CombatPreviewData currentCombatPreview;
    private bool subscribedPhaseEvent;

    public bool HasSelectedUnit => selectedUnit != null;
    public bool IsMenuVisible => actionMenu != null && actionMenu.IsVisible;
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
        if(attackDirectionPreview == null) attackDirectionPreview = GetComponent<UnitAttackDirectionPreview>();
        if(attackDirectionPreview == null) attackDirectionPreview = gameObject.AddComponent<UnitAttackDirectionPreview>();
        if(combatPreviewUI == null) combatPreviewUI = GetComponent<CombatPreviewUI>();
        if(combatPreviewUI == null) combatPreviewUI = gameObject.AddComponent<CombatPreviewUI>();
        ResolveTargetCamera();
        ResolvePlayerUnitPrefab();
        convertNeutralAction.SetPlayerUnitPrefab(playerUnitPrefab);
    }

    /// <summary>
    /// 选择攻击行动时持续刷新鼠标悬停目标的受击边预览。
    /// </summary>
    private void Update()
    {
        RefreshCombatPreviewLifeState();
        RefreshAttackDirectionPreview();
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
        UnitFacingHoverController.SetSuppressed(false);
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
        HideCombatPreview();
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
    /// 只隐藏当前行动菜单，不清除单位选择和高亮。
    /// </summary>
    public void HideMenuOnly()
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

        if(combatPreviewUI != null && combatPreviewUI.IsVisible)
        {
            CancelCombatPreview();
            return true;
        }

        if(selectedAction != PlayerUnitActionType.None)
        {
            selectedAction = PlayerUnitActionType.None;
            ClearActionHighlights();
            RestoreMoveRangeIfCanMove();
            RefreshMenu();
            return true;
        }

        return false;
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

        if(selectedAction == PlayerUnitActionType.Bow)
        {
            TryExecuteBow(worldPosition);
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

        if(actionType == PlayerUnitActionType.Bow)
        {
            SelectBowAction();
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
        if(!UnitAttackSkillSet.HasSkill(selectedUnit, UnitAttackSkillType.Sword)) return;

        selectedAction = PlayerUnitActionType.Sword;
        HideMoveRange();
        interactionRangeHighlighter.ClearInteractionRange();

        if(selectedUnit != null)
        {
            selectedUnit.UnitMover.RefreshCurrentCell();
            actionCellHighlighter.ShowAttackCell(selectedUnit.CurrentCell);
        }

        attackRangeHighlighter.ShowAttackRange(selectedUnit, swordAction);
        actionMenu.SetSelectedAction(selectedAction);
        HideMenuOnly();
    }

    /// <summary>
    /// 选择 Bow 行动并显示自身格黄色边框与红色攻击范围。
    /// </summary>
    private void SelectBowAction()
    {
        if(!UnitAttackSkillSet.HasSkill(selectedUnit, UnitAttackSkillType.Bow)) return;

        selectedAction = PlayerUnitActionType.Bow;
        HideMoveRange();
        interactionRangeHighlighter.ClearInteractionRange();

        if(selectedUnit != null)
        {
            selectedUnit.UnitMover.RefreshCurrentCell();
            actionCellHighlighter.ShowAttackCell(selectedUnit.CurrentCell);
        }

        attackRangeHighlighter.ShowAttackRange(selectedUnit, bowAction);
        actionMenu.SetSelectedAction(selectedAction);
        HideMenuOnly();
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
        HideMenuOnly();
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
        HideMenuOnly();
    }

    /// <summary>
    /// 尝试对点击格子上的单位执行 Sword。
    /// </summary>
    private void TryExecuteSword(Vector3 worldPosition)
    {
        if(GridManager.Instance == null) return;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit target)) return;

        ShowCombatPreview(target, UnitAttackSkillType.Sword);
    }

    /// <summary>
    /// 尝试对点击格子上的单位执行 Bow。
    /// </summary>
    private void TryExecuteBow(Vector3 worldPosition)
    {
        if(GridManager.Instance == null) return;

        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit target)) return;

        ShowCombatPreview(target, UnitAttackSkillType.Bow);
    }

    /// <summary>
    /// 显示指定攻击目标的战斗伤害预览。
    /// </summary>
    private void ShowCombatPreview(Unit target, UnitAttackSkillType attackSkill)
    {
        if(!CombatResolver.TryBuildPreview(selectedUnit, target, attackSkill, out CombatPreviewData previewData)) return;

        currentCombatPreview = previewData;
        if(attackDirectionPreview != null)
        {
            attackDirectionPreview.Clear();
        }

        if(combatPreviewUI != null)
        {
            combatPreviewUI.Show(currentCombatPreview, ConfirmCombatPreview);
        }
    }

    /// <summary>
    /// 确认并执行当前预览中的战斗。
    /// </summary>
    private void ConfirmCombatPreview()
    {
        if(!currentCombatPreview.IsValid) return;

        if(CombatResolver.ExecuteCombat(currentCombatPreview))
        {
            HideCombatPreview();
            ClearAfterAction();
        }
    }

    /// <summary>
    /// 取消伤害预览并回到攻击二级菜单展开状态。
    /// </summary>
    private void CancelCombatPreview()
    {
        HideCombatPreview();
        selectedAction = PlayerUnitActionType.Attack;
        ClearActionHighlights();
        HideMoveRange();
        RefreshMenu();
        actionMenu.SetSelectedAction(PlayerUnitActionType.Attack);
    }

    /// <summary>
    /// 隐藏当前伤害预览。
    /// </summary>
    private void HideCombatPreview()
    {
        currentCombatPreview = default;
        if(combatPreviewUI != null)
        {
            combatPreviewUI.Hide();
        }
    }

    /// <summary>
    /// 当预览中的任意单位死亡或失效时自动隐藏伤害预览。
    /// </summary>
    private void RefreshCombatPreviewLifeState()
    {
        if(combatPreviewUI == null || !combatPreviewUI.IsVisible) return;
        if(!currentCombatPreview.IsValid)
        {
            HideCombatPreview();
            return;
        }

        if(currentCombatPreview.Attacker == null || currentCombatPreview.Defender == null)
        {
            HideCombatPreview();
            return;
        }

        if(!currentCombatPreview.Attacker.IsAlive || !currentCombatPreview.Defender.IsAlive)
        {
            HideCombatPreview();
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

        if(attackDirectionPreview != null)
        {
            attackDirectionPreview.Clear();
        }
    }

    /// <summary>
    /// 根据当前鼠标悬停目标刷新攻击受击边预览。
    /// </summary>
    private void RefreshAttackDirectionPreview()
    {
        if(attackDirectionPreview == null) return;
        if(combatPreviewUI != null && combatPreviewUI.IsVisible)
        {
            UnitFacingHoverController.SetSuppressed(false);
            attackDirectionPreview.Clear();
            return;
        }

        if(selectedUnit == null || !IsTargetingAttack())
        {
            UnitFacingHoverController.SetSuppressed(false);
            attackDirectionPreview.Clear();
            return;
        }

        UnitFacingHoverController.SetSuppressed(true);
        if(GridManager.Instance == null || Mouse.current == null || IsPointerOverUI())
        {
            attackDirectionPreview.Clear();
            return;
        }

        if(!TryGetHoveredAttackTarget(out Unit target))
        {
            attackDirectionPreview.Clear();
            return;
        }

        attackDirectionPreview.Show(selectedUnit, target);
    }

    /// <summary>
    /// 尝试获得当前鼠标悬停且可攻击的目标单位。
    /// </summary>
    private bool TryGetHoveredAttackTarget(out Unit target)
    {
        target = null;
        ResolveTargetCamera();
        if(targetCamera == null || Mouse.current == null || GridManager.Instance == null) return false;

        Vector3 worldPosition = GetMouseWorldPosition();
        if(TryGetHoveredUnitFromCollider(worldPosition, out target) && CanSelectedAttackTarget(target)) return true;
        if(TryGetHoveredUnitFromGrid(worldPosition, out target) && CanSelectedAttackTarget(target)) return true;

        target = null;
        return false;
    }

    /// <summary>
    /// 优先通过碰撞体获得鼠标悬停单位。
    /// </summary>
    private bool TryGetHoveredUnitFromCollider(Vector3 worldPosition, out Unit target)
    {
        target = null;

        Collider2D collider2D = Physics2D.OverlapPoint(worldPosition);
        if(collider2D != null && TryResolveAliveUnit(collider2D.gameObject, out target)) return true;

        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(ray, out RaycastHit hit) && TryResolveAliveUnit(hit.collider.gameObject, out target)) return true;

        return false;
    }

    /// <summary>
    /// 在没有碰撞体时通过鼠标所在格子的占用表获得单位。
    /// </summary>
    private bool TryGetHoveredUnitFromGrid(Vector3 worldPosition, out Unit target)
    {
        target = null;
        GridCell targetCell = GridManager.Instance.GetCellFromWorldPosition(worldPosition);
        if(!GridManager.Instance.IsValidCell(targetCell)) return false;

        Vector3 cellCenter = GridManager.Instance.GetWorldInGrid(targetCell);
        if(Mathf.Abs(worldPosition.x - cellCenter.x) > 0.5f || Mathf.Abs(worldPosition.y - cellCenter.y) > 0.5f) return false;

        return UnitGridOccupancy.TryGetUnit(targetCell, out target) && target != null && target.IsAlive;
    }

    /// <summary>
    /// 从对象或父对象中解析存活单位。
    /// </summary>
    private bool TryResolveAliveUnit(GameObject targetObject, out Unit target)
    {
        target = targetObject != null ? targetObject.GetComponentInParent<Unit>() : null;
        return target != null && target.IsAlive && target.gameObject.activeInHierarchy;
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
    /// 判断当前是否正在选择攻击目标。
    /// </summary>
    private bool IsTargetingAttack()
    {
        return selectedAction == PlayerUnitActionType.Sword || selectedAction == PlayerUnitActionType.Bow;
    }

    /// <summary>
    /// 判断当前选中攻击是否可以攻击指定目标。
    /// </summary>
    private bool CanSelectedAttackTarget(Unit target)
    {
        if(selectedAction == PlayerUnitActionType.Sword) return swordAction.CanExecute(selectedUnit, target);
        if(selectedAction == PlayerUnitActionType.Bow) return bowAction.CanExecute(selectedUnit, target);

        return false;
    }

    /// <summary>
    /// 检查鼠标是否正指向 UI。
    /// </summary>
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// 查找攻击目标预览使用的相机。
    /// </summary>
    private void ResolveTargetCamera()
    {
        if(targetCamera != null) return;

        targetCamera = Camera.main;
        if(targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
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
