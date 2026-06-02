using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制 AI 单位自动选择 Sword 攻击或向最近可伤害目标移动。
/// </summary>
[RequireComponent(typeof(UnitMover))]
public class UnitRandomAI : MonoBehaviour
{
    private readonly SwordAction swordAction = new SwordAction();
    private Unit unit;
    private UnitMover unitMover;

    /// <summary>
    /// 缓存单位和移动组件。
    /// </summary>
    private void Awake()
    {
        unit = GetComponent<Unit>();
        unitMover = GetComponent<UnitMover>();
    }

    /// <summary>
    /// 执行一次 AI 行动协程：先攻击，否则移动一格，移动后若可攻击则攻击。
    /// </summary>
    public IEnumerator ActRoutine()
    {
        if(unit == null || !unit.UsesRandomAI || !unit.IsAlive) yield break;
        if(!CanUnitAct()) yield break;

        UnitGridOccupancy.RebuildFromScene();
        if(TryAttackAdjacentTarget()) yield break;

        bool moved = TryMoveTowardNearestDamageTarget();
        if(moved)
        {
            while(unitMover != null && unitMover.IsMoving)
            {
                yield return null;
            }

            UnitGridOccupancy.RebuildFromScene();
            TryAttackAdjacentTarget();
        }
    }

    /// <summary>
    /// 兼容旧调用入口，启动一次 AI 行动协程。
    /// </summary>
    public void Act()
    {
        StartCoroutine(ActRoutine());
    }

    /// <summary>
    /// 判断当前 AI 单位是否还能执行行动。
    /// </summary>
    private bool CanUnitAct()
    {
        UnitActionState actionState = unit.GetComponent<UnitActionState>();
        return actionState == null || actionState.CanAct;
    }

    /// <summary>
    /// 尝试攻击相邻一格内的合法目标。
    /// </summary>
    private bool TryAttackAdjacentTarget()
    {
        if(!CanUnitAct()) return false;

        List<Unit> targets = swordAction.GetAttackableTargets(unit);
        if(targets.Count == 0) return false;

        Unit target = GetPreferredTarget(targets);
        return swordAction.Execute(unit, target);
    }

    /// <summary>
    /// 尝试向最近可伤害目标移动一格。
    /// </summary>
    private bool TryMoveTowardNearestDamageTarget()
    {
        if(GridManager.Instance == null || unitMover == null) return false;

        unitMover.RefreshCurrentCell();
        if(!TryFindNextCellTowardTarget(out GridCell nextCell)) return false;

        return unitMover.TryMoveToCell(nextCell);
    }

    /// <summary>
    /// 使用 BFS 找到通向最近可伤害目标的下一格。
    /// </summary>
    private bool TryFindNextCellTowardTarget(out GridCell nextCell)
    {
        nextCell = default;

        Queue<GridCell> cellsToCheck = new Queue<GridCell>();
        Dictionary<GridCell, GridCell> firstSteps = new Dictionary<GridCell, GridCell>();
        HashSet<GridCell> checkedCells = new HashSet<GridCell>();

        GridCell startCell = unit.CurrentCell;
        cellsToCheck.Enqueue(startCell);
        checkedCells.Add(startCell);

        while(cellsToCheck.Count > 0)
        {
            GridCell currentCell = cellsToCheck.Dequeue();
            if(currentCell != startCell && CanAttackAnyTargetFromCell(currentCell))
            {
                nextCell = firstSteps[currentCell];
                return true;
            }

            foreach(Direction direction in UnitMovementUtility.FourDirections)
            {
                GridCell neighborCell = GridManager.Instance.GetNeighborCell(currentCell, direction);
                if(checkedCells.Contains(neighborCell)) continue;
                checkedCells.Add(neighborCell);

                if(!UnitMovementUtility.CanUnitMoveToCell(unit, neighborCell)) continue;

                cellsToCheck.Enqueue(neighborCell);
                firstSteps[neighborCell] = currentCell == startCell ? neighborCell : firstSteps[currentCell];
            }
        }

        return false;
    }

    /// <summary>
    /// 判断从指定格子是否可以攻击任意合法目标。
    /// </summary>
    private bool CanAttackAnyTargetFromCell(GridCell actorCell)
    {
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach(Unit target in allUnits)
        {
            if(target == null || !target.IsAlive) continue;
            if(target.UnitMover != null)
            {
                target.UnitMover.RefreshCurrentCell();
            }

            if(swordAction.CanAttackFromCell(unit, actorCell, target))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 在多个可攻击目标中选择生命值最低的目标。
    /// </summary>
    private Unit GetPreferredTarget(List<Unit> targets)
    {
        Unit preferredTarget = targets[0];
        int preferredHealth = GetCurrentHealth(preferredTarget);

        for(int i = 1; i < targets.Count; i++)
        {
            int currentHealth = GetCurrentHealth(targets[i]);
            if(currentHealth < preferredHealth)
            {
                preferredTarget = targets[i];
                preferredHealth = currentHealth;
            }
        }

        return preferredTarget;
    }

    /// <summary>
    /// 获得单位当前生命值。
    /// </summary>
    private int GetCurrentHealth(Unit target)
    {
        UnitHealth health = target != null ? target.GetComponent<UnitHealth>() : null;
        return health != null ? health.CurrentHealth : int.MaxValue;
    }
}
