using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 转化中立单位的互动行动，用玩家单位 Prefab 替换相邻中立单位。
/// </summary>
public class ConvertNeutralAction : InteractionActionBase
{
    private GameObject playerUnitPrefab;

    public override string ActionName => "转化";

    /// <summary>
    /// 设置转化后生成的玩家单位 Prefab。
    /// </summary>
    public void SetPlayerUnitPrefab(GameObject prefab)
    {
        playerUnitPrefab = prefab;
    }

    /// <summary>
    /// 判断目标是否可以被当前玩家单位转化。
    /// </summary>
    public override bool CanExecute(Unit actor, Unit target)
    {
        if(!CanExecute(actor)) return false;
        if(playerUnitPrefab == null) return false;
        if(actor.Team != UnitTeam.Player) return false;
        if(target == null || !target.IsAlive) return false;
        if(target.Team != UnitTeam.Neutral) return false;

        return IsInInteractionRange(actor, target);
    }

    /// <summary>
    /// 执行转化，生成玩家单位替代中立单位并继承当前血量。
    /// </summary>
    public override bool Execute(Unit actor, Unit target)
    {
        if(!CanExecute(actor, target)) return false;
        if(GridManager.Instance == null) return false;

        target.UnitMover.RefreshCurrentCell();
        GridCell targetCell = target.CurrentCell;
        int inheritedHealth = GetCurrentHealth(target);
        Vector3 spawnPosition = GridManager.Instance.GetWorldInGrid(targetCell);
        Quaternion spawnRotation = target.transform.rotation;

        target.SetAlive(false);
        target.gameObject.SetActive(false);

        GameObject playerObject = Object.Instantiate(playerUnitPrefab, spawnPosition, spawnRotation, GetPlayerUnitParent());
        playerObject.name = playerUnitPrefab.name;

        Unit playerUnit = playerObject.GetComponent<Unit>();
        if(playerUnit == null)
        {
            Object.Destroy(playerObject);
            Object.Destroy(target.gameObject);
            return false;
        }

        UnitHealth playerHealth = playerObject.GetComponent<UnitHealth>();
        if(playerHealth != null)
        {
            playerHealth.SetCurrentHealth(inheritedHealth);
        }

        UnitActionState playerActionState = playerObject.GetComponent<UnitActionState>();
        if(playerActionState != null)
        {
            playerActionState.MarkActed();
        }

        playerUnit.UnitMover.RefreshCurrentCell();
        UnitGridOccupancy.RegisterUnit(playerUnit, targetCell);
        Object.Destroy(target.gameObject);

        MarkActionConsumed(actor);
        return true;
    }

    /// <summary>
    /// 获得当前可以被转化的目标格子。
    /// </summary>
    public override List<GridCell> GetTargetCells(Unit actor)
    {
        List<GridCell> targetCells = new List<GridCell>();
        if(actor == null || GridManager.Instance == null) return targetCells;

        actor.UnitMover.RefreshCurrentCell();
        foreach(Direction direction in UnitMovementUtility.FourDirections)
        {
            GridCell targetCell = GridManager.Instance.GetNeighborCell(actor.CurrentCell, direction);
            if(!UnitGridOccupancy.TryGetUnit(targetCell, out Unit target)) continue;
            if(!CanExecute(actor, target)) continue;

            targetCells.Add(targetCell);
        }

        return targetCells;
    }

    /// <summary>
    /// 读取被转化单位当前生命值。
    /// </summary>
    private int GetCurrentHealth(Unit target)
    {
        UnitHealth targetHealth = target != null ? target.GetComponent<UnitHealth>() : null;
        return targetHealth != null ? targetHealth.CurrentHealth : 100;
    }

    /// <summary>
    /// 获取或创建玩家单位生成父物体。
    /// </summary>
    private Transform GetPlayerUnitParent()
    {
        GameObject unitRoot = GameObject.Find("Unit");
        if(unitRoot == null)
        {
            unitRoot = new GameObject("Unit");
        }

        Transform playerParent = unitRoot.transform.Find("Player");
        if(playerParent == null)
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.transform.SetParent(unitRoot.transform);
            playerParent = playerObject.transform;
        }

        return playerParent;
    }
}
