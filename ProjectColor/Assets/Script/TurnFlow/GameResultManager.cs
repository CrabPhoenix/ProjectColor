using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 检查场上单位阵营数量并在满足条件时结束对局。
/// </summary>
public class GameResultManager : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    private readonly HashSet<Unit> subscribedUnits = new HashSet<Unit>();
    private bool hasResult;

    /// <summary>
    /// 初始化回合管理器引用。
    /// </summary>
    private void Awake()
    {
        ResolveTurnManager();
    }

    /// <summary>
    /// 启用时订阅回合事件并绑定场上单位事件。
    /// </summary>
    private void OnEnable()
    {
        ResolveTurnManager();
        if(turnManager != null)
        {
            turnManager.OnPhaseChanged += HandlePhaseChanged;
        }

        RefreshUnitSubscriptions();
    }

    /// <summary>
    /// 启动时立即检查一次初始胜负状态。
    /// </summary>
    private void Start()
    {
        RefreshUnitSubscriptions();
        CheckGameResult();
    }

    /// <summary>
    /// 禁用时取消所有事件订阅。
    /// </summary>
    private void OnDisable()
    {
        if(turnManager != null)
        {
            turnManager.OnPhaseChanged -= HandlePhaseChanged;
        }

        foreach(Unit unit in subscribedUnits)
        {
            UnsubscribeUnit(unit);
        }

        subscribedUnits.Clear();
    }

    /// <summary>
    /// 根据当前场上存活单位判定胜负。
    /// </summary>
    public void CheckGameResult()
    {
        if(hasResult) return;
        if(!GameStageManager.IsGameplayActive()) return;

        ResolveTurnManager();
        if(turnManager == null || turnManager.IsGameOver) return;

        UnitGridOccupancy.RebuildFromScene();
        int playerCount = UnitGridOccupancy.GetAliveUnits(UnitTeam.Player).Count;
        int enemyCount = UnitGridOccupancy.GetAliveUnits(UnitTeam.Enemy).Count;
        int neutralCount = UnitGridOccupancy.GetAliveUnits(UnitTeam.Neutral).Count;

        if(playerCount <= 0)
        {
            EndGame(GameResult.Defeat);
            return;
        }

        if(enemyCount <= 0 && neutralCount <= 0)
        {
            EndGame(GameResult.Victory);
        }
    }

    /// <summary>
    /// 阶段变化时刷新订阅并检查一次胜负。
    /// </summary>
    private void HandlePhaseChanged(TurnPhase phase)
    {
        RefreshUnitSubscriptions();
        CheckGameResult();
    }

    /// <summary>
    /// 单位存活状态变化时检查胜负。
    /// </summary>
    private void HandleUnitAliveStateChanged(Unit unit)
    {
        RefreshUnitSubscriptions();
        CheckGameResult();
    }

    /// <summary>
    /// 单位阵营变化时检查胜负。
    /// </summary>
    private void HandleUnitTeamChanged(Unit unit)
    {
        RefreshUnitSubscriptions();
        CheckGameResult();
    }

    /// <summary>
    /// 重新绑定当前场景内所有单位的状态事件。
    /// </summary>
    private void RefreshUnitSubscriptions()
    {
        Unit[] sceneUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach(Unit unit in sceneUnits)
        {
            if(unit == null || subscribedUnits.Contains(unit)) continue;

            unit.OnAliveStateChanged += HandleUnitAliveStateChanged;
            unit.OnTeamChanged += HandleUnitTeamChanged;
            subscribedUnits.Add(unit);
        }

        subscribedUnits.RemoveWhere(unit => unit == null);
    }

    /// <summary>
    /// 取消单个单位的事件订阅。
    /// </summary>
    private void UnsubscribeUnit(Unit unit)
    {
        if(unit == null) return;

        unit.OnAliveStateChanged -= HandleUnitAliveStateChanged;
        unit.OnTeamChanged -= HandleUnitTeamChanged;
    }

    /// <summary>
    /// 结束当前对局并通知回合管理器。
    /// </summary>
    private void EndGame(GameResult result)
    {
        hasResult = true;
        turnManager.EndGame(result);
    }

    /// <summary>
    /// 查找当前场景中的回合管理器。
    /// </summary>
    private void ResolveTurnManager()
    {
        if(turnManager != null) return;

        turnManager = GetComponent<TurnManager>();
        if(turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }
    }
}
