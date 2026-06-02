using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制允许随机行动的单位，随机移动到四方向一格内的有效格子。
/// </summary>
[RequireComponent(typeof(UnitMover))]
public class UnitRandomAI : MonoBehaviour
{
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
    /// 执行一次随机移动。
    /// </summary>
    public void Act()
    {
        if(unit == null || !unit.UsesRandomAI) return;

        List<GridCell> movableCells = unitMover.GetMovableCells();
        if(movableCells.Count == 0) return;

        int index = Random.Range(0, movableCells.Count);
        unitMover.TryMoveToCell(movableCells[index]);
    }
}
