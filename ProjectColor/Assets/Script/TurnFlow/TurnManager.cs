using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制玩家、敌方和中立阵营的回合阶段顺序。
/// </summary>
public class TurnManager : MonoBehaviour
{
    private static TurnManager instance;

    [SerializeField] private float aiActionMinDelay = 0.5f;
    [SerializeField] private float aiActionDelay = 0.5f;
    [SerializeField] private float phaseNoticeDuration = 2f;
    [SerializeField] private TurnPhaseCameraController cameraController;

    private TurnPhase currentPhase = TurnPhase.Player;
    private bool isRunningPhase;

    public static TurnManager Instance => instance;
    public TurnPhase CurrentPhase => currentPhase;
    public event Action<TurnPhase> OnPhaseChanged;
    public event Action<TurnPhase, float> OnPhaseNoticeRequested;

    /// <summary>
    /// 建立回合管理器单例。
    /// </summary>
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ResolveCameraController();
    }

    /// <summary>
    /// 将 AI 行动延迟限制在设定的最小延迟内，防止过快或过慢。
    /// </summary>
    private void OnValidate()
    {
        aiActionMinDelay = Mathf.Max(0, aiActionMinDelay);
        aiActionDelay = Mathf.Max(aiActionMinDelay, aiActionDelay);
    }

    /// <summary>
    /// 游戏开始时进入玩家阶段。
    /// </summary>
    private void Start()
    {
        BeginPlayerPhase();
    }

    /// <summary>
    /// 玩家点击 End Phase 后结束玩家阶段。
    /// </summary>
    public void EndPlayerPhase()
    {
        if(currentPhase != TurnPhase.Player || isRunningPhase) return;
        StartCoroutine(RunAutoPhases());
    }

    /// <summary>
    /// 开始玩家阶段，等待玩家手动结束。
    /// </summary>
    private void BeginPlayerPhase()
    {
        SetPhase(TurnPhase.Player);
        ResolveCameraController();
        if(cameraController != null)
        {
            cameraController.FocusRandomPlayerUnit();
        }
        isRunningPhase = false;
    }

    /// <summary>
    /// 按敌方阶段和中立阶段顺序自动执行 AI 行动。
    /// </summary>
    private IEnumerator RunAutoPhases()
    {
        isRunningPhase = true;
        ResolveCameraController();
        if(cameraController != null)
        {
            cameraController.SetManualControlEnabled(false);
            cameraController.StopFollowing();
        }

        yield return RunAiPhase(TurnPhase.Ally, UnitTeam.Ally);
        yield return RunAiPhase(TurnPhase.Enemy, UnitTeam.Enemy);
        yield return RunAiPhase(TurnPhase.Neutral, UnitTeam.Neutral);

        BeginPlayerPhase();
    }

    /// <summary>
    /// 执行指定阵营所有存活单位的 AI 行动。
    /// </summary>
    private IEnumerator RunAiPhase(TurnPhase phase, UnitTeam team)
    {
        SetPhase(phase);
        UnitGridOccupancy.RebuildFromScene();
        List<Unit> units = UnitGridOccupancy.GetAliveUnits(team);
        Unit firstUnit = GetFirstActingUnit(units);
        if(firstUnit != null)
        {
            ResolveCameraController();
            if(cameraController != null)
            {
                cameraController.FollowUnit(firstUnit);
            }
        }

        OnPhaseNoticeRequested?.Invoke(phase, phaseNoticeDuration);
        if(phaseNoticeDuration > 0)
        {
            yield return new WaitForSeconds(phaseNoticeDuration);
        }

        foreach(Unit unit in units)
        {
            if(unit == null || !unit.IsAlive) continue;

            UnitRandomAI ai = unit.GetComponent<UnitRandomAI>();
            if(ai != null)
            {
                ResolveCameraController();
                if(cameraController != null)
                {
                    cameraController.FollowUnit(unit);
                }

                yield return ai.ActRoutine();
            }

            UnitMover unitMover = unit.UnitMover;
            while(unitMover != null && unitMover.IsMoving)
            {
                yield return null;
            }

            float actionDelay = aiActionDelay;
            if(actionDelay > 0)
            {
                yield return new WaitForSeconds(actionDelay);
            }
        }
    }

    /// <summary>
    /// 获得当前阶段第一个会行动的存活单位。
    /// </summary>
    private Unit GetFirstActingUnit(List<Unit> units)
    {
        foreach(Unit unit in units)
        {
            if(unit == null || !unit.IsAlive) continue;
            if(unit.GetComponent<UnitRandomAI>() == null) continue;

            return unit;
        }

        return null;
    }

    /// <summary>
    /// 切换当前阶段并广播阶段变化。
    /// </summary>
    private void SetPhase(TurnPhase phase)
    {
        currentPhase = phase;
        OnPhaseChanged?.Invoke(currentPhase);
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
