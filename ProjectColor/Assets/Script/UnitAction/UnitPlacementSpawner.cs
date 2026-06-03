using UnityEngine;

/// <summary>
/// 根据单位摆放配置在有效格子的中心生成单位。
/// </summary>
public class UnitPlacementSpawner : MonoBehaviour
{
    [SerializeField] private UnitPlacementConfig config;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool clearGeneratedUnitsBeforeSpawn = true;
    [SerializeField] private bool skipPlayerUnits = true;

    /// <summary>
    /// 启动时根据配置生成单位。
    /// </summary>
    private void Start()
    {
        if(spawnOnStart)
        {
            SpawnUnits();
        }
    }

    /// <summary>
    /// 根据配置中的单位信息生成单位。
    /// </summary>
    public void SpawnUnits()
    {
        if(config == null || GridManager.Instance == null || !GridManager.Instance.IsGridReady()) return;

        if(clearGeneratedUnitsBeforeSpawn)
        {
            ClearGeneratedUnits();
        }

        foreach(UnitPlacementCell cell in config.Cells)
        {
            if(!cell.HasUnit || cell.UnitPrefab == null) continue;
            if(skipPlayerUnits && cell.UnitTeam == UnitTeam.Player) continue;

            GridCell gridCell = new GridCell(cell.CellPosition.x, cell.CellPosition.y);
            if(!GridManager.Instance.IsCellWalkable(gridCell)) continue;

            Vector3 spawnPosition = GridManager.Instance.GetWorldInGrid(gridCell);
            Transform parent = GetOrCreateTeamParent(cell.UnitTeam);
            Unit unit = Instantiate(cell.UnitPrefab, spawnPosition, Quaternion.identity, parent);
            GameObject unitObject = unit.gameObject;
            unitObject.name = cell.UnitPrefab.name;
            if(unitObject.GetComponent<UnitPlacementSpawned>() == null)
            {
                unitObject.AddComponent<UnitPlacementSpawned>();
            }
        }
    }

    /// <summary>
    /// 清理之前由配置生成的单位。
    /// </summary>
    public void ClearGeneratedUnits()
    {
        UnitPlacementSpawned[] spawnedUnits = FindObjectsByType<UnitPlacementSpawned>(FindObjectsSortMode.None);
        foreach(UnitPlacementSpawned spawnedUnit in spawnedUnits)
        {
            Destroy(spawnedUnit.gameObject);
        }
    }

    /// <summary>
    /// 获得或创建指定阵营的单位父物体。
    /// </summary>
    private Transform GetOrCreateTeamParent(UnitTeam team)
    {
        GameObject unitRoot = GameObject.Find("Unit");
        if(unitRoot == null)
        {
            unitRoot = new GameObject("Unit");
        }

        string teamName = team.ToString();
        Transform teamParent = unitRoot.transform.Find(teamName);
        if(teamParent != null) return teamParent;

        GameObject teamObject = new GameObject(teamName);
        teamObject.transform.SetParent(unitRoot.transform);
        return teamObject.transform;
    }
}
