using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 负责生成cookie
/// </summary>
public class CookieSpawner : ResetBehavior
{
    [SerializeField] private GameObject cookie;
    [SerializeField] private GameObject superCookie;
    private GridManager grid;
    private LevelManager level;

    void Start()
    {
        grid = GridManager.Instance;
        level = GetComponentInParent<LevelManager>();
 
    }

    void OnEnable()
    {
        GameEvent.OnGameStart += SpawnCookies;
    }

    void OnDisable()
    {
        GameEvent.OnGameStart -= SpawnCookies;
    }

    //遍历所有的格子，在config设定的坐标生成superCookie，在其余所有能走的地方生成cookie
    private void SpawnCookies()
    {
        foreach (GridObject gridObject in grid.GetGridObject())
        {
            Vector2 cellPosition = grid.GetWorldInGrid(gridObject.GetCellPosion());

            if (level.SuperCookieConfig.positions.Contains(cellPosition))
            {
                GameObject obj = Instantiate(superCookie, cellPosition, Quaternion.identity);
                obj.transform.parent = transform;
                continue;
            }

            if (gridObject.type == GridObjectType.Path && NotPortal(cellPosition))
            {
                GameObject obj = Instantiate(cookie, cellPosition, Quaternion.identity);
                obj.transform.parent = transform;
            }
        }
    }

    //将所有传送门点位排除
    private bool NotPortal(Vector2 gridPosition)
    {
        return gridPosition != level.PortalConfig.point1 && gridPosition != level.PortalConfig.point2;
    }

    public override void Reset_1()
    {
        
    }

    public override void Reset_2()
    {
        base.Reset_2();
        SpawnCookies();

    }
}
