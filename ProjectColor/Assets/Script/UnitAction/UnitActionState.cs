using UnityEngine;

/// <summary>
/// 记录单位在当前所属阵营阶段内的移动与行动状态。
/// </summary>
public class UnitActionState : MonoBehaviour
{
    private Unit unit;
    private UnitTurnState turnState = UnitTurnState.Ready;
    private bool subscribedPhaseEvent;

    public UnitTurnState TurnState => turnState;
    public bool HasActed => turnState == UnitTurnState.Acted;
    public bool HasMoved => turnState == UnitTurnState.MovedOnly || turnState == UnitTurnState.Acted;
    public bool CanMove => unit != null && unit.IsAlive && turnState == UnitTurnState.Ready;
    public bool CanAct => unit != null && unit.IsAlive && turnState != UnitTurnState.Acted;
    public bool CanOpenActionMenu => CanAct;

    /// <summary>
    /// 缓存单位组件。
    /// </summary>
    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    /// <summary>
    /// 尝试订阅回合阶段事件。
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
    /// 取消订阅回合阶段事件。
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
    /// 标记本阶段已经移动但还没有执行行动。
    /// </summary>
    public void MarkMoved()
    {
        if(turnState == UnitTurnState.Ready)
        {
            turnState = UnitTurnState.MovedOnly;
        }
    }

    /// <summary>
    /// 标记本阶段已经执行行动。
    /// </summary>
    public void MarkActed()
    {
        turnState = UnitTurnState.Acted;
    }

    /// <summary>
    /// 重置本阶段操作状态。
    /// </summary>
    public void ResetAction()
    {
        turnState = UnitTurnState.Ready;
    }

    /// <summary>
    /// 进入所属阵营阶段时重置操作状态。
    /// </summary>
    private void HandlePhaseChanged(TurnPhase phase)
    {
        if(unit == null) return;
        if(IsUnitPhase(phase, unit.Team))
        {
            ResetAction();
        }
    }

    /// <summary>
    /// 判断阶段是否对应指定阵营。
    /// </summary>
    private bool IsUnitPhase(TurnPhase phase, UnitTeam team)
    {
        return (phase == TurnPhase.Player && team == UnitTeam.Player)
            || (phase == TurnPhase.Ally && team == UnitTeam.Ally)
            || (phase == TurnPhase.Enemy && team == UnitTeam.Enemy)
            || (phase == TurnPhase.Neutral && team == UnitTeam.Neutral);
    }

    /// <summary>
    /// 尝试订阅回合阶段事件。
    /// </summary>
    private void TrySubscribePhaseEvent()
    {
        if(subscribedPhaseEvent || TurnManager.Instance == null) return;

        TurnManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        subscribedPhaseEvent = true;
    }
}
