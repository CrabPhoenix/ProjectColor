using System;
using UnityEngine;

/// <summary>
/// 记录部署阶段中一种玩家单位的 Prefab 和可摆放数量。
/// </summary>
[Serializable]
public class UnitDeployEntry
{
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private int count = 1;

    public Unit UnitPrefab => unitPrefab;
    public int Count => Mathf.Max(0, count);
}
