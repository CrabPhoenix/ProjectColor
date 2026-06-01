using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 将敌人以最短距离导航至特定目标点
/// </summary>
public static class AI_Navigation
{
    // 在default状态下获得自身附近离目标最近的格子以及相应的方向
    public static (Vector2 new_direction, Vector3 new_target) GetDefaultCellPosition
    (Vector2 current_direction, Vector3 current_position, Vector3 final_position = new())
    {
        GridManager gridManager = GridManager.Instance;

        //验证格子是否可移动, 将可行的格子加入数组种
        List<Vector2> walkable_dir = new List<Vector2>();

        foreach (Vector2 dir in GetDirection())
        {
            //去除与当前移动相反的方向以防止在两个格子之间反复
            if(IsTurnBack(dir, current_direction)) continue;
            
            //将所有剩下的能够移动的格子方向加入数组
            if(gridManager.IsPlayerNeighborCellWalkable(current_position, dir)) walkable_dir.Add(dir);      
        }

        if(walkable_dir == null) throw new System.Exception("出现四面都是无效方向的格子");

        return GetClosestTargetAndDirection(walkable_dir, current_position, final_position);
    }

    // 在eaten状态下获得自身附近离目标最近的格子以及相应的方向
    public static (Vector2 new_direction, Vector3 new_target) GetEatenCellPosition
    (Vector2 current_direction, Vector3 current_position, Vector3 final_position = new())
    {
        GridManager gridManager = GridManager.Instance;

        //验证格子是否可移动, 将可行的格子加入数组种
        List<Vector2> walkable_dir = new List<Vector2>();

        foreach (Vector2 dir in GetDirection())
        {
            //去除与当前移动相反的方向以防止在两个格子之间反复
            if(IsTurnBack(dir, current_direction)) continue;
            
            //将所有剩下的能够移动的格子方向加入数组
            if(gridManager.IsNPCNeighborCellWalkable(current_position, dir)) walkable_dir.Add(dir);      
        }

        if(walkable_dir == null) throw new System.Exception("出现四面都是无效方向的格子");

        return GetClosestTargetAndDirection(walkable_dir, current_position, final_position);
    }

    // 在frighten状态下获得自身附近随机的可移动格子以及相应的方向
    public static (Vector2 new_direction, Vector3 new_target) GetFrightenPosition
    (Vector2 current_direction, Vector3 current_position, Vector3 final_position = new())
    {
        GridManager gridManager = GridManager.Instance;

        //验证格子是否可移动, 将可行的格子加入数组种
        List<Vector2> walkable_dir = new List<Vector2>();

        foreach (Vector2 dir in GetDirection())
        {
            //去除与当前移动相反的方向以防止在两个格子之间反复
            if(IsTurnBack(dir, current_direction)) continue;
            
            //将所有剩下的能够移动的格子方向加入数组
            if(gridManager.IsPlayerNeighborCellWalkable(current_position, dir)) walkable_dir.Add(dir);      
        }

        if(walkable_dir == null) throw new System.Exception("出现四面都是无效方向的格子");

        //选择一个随机的可移动方向
        Vector2 direction = walkable_dir[Random.Range(0, walkable_dir.Count)];
        Vector3 target = gridManager.GetNeighborCellPositionFromWorldDirection(current_position, direction);

        if(target == current_position) throw new System.Exception("检查AI_Navigation");

        return (direction, target);

    }

    //计算每个有效方向的格子离目标点的距离并选择距离最短的一个，并返回相应的方向与对应的邻格
    public static (Vector2 new_direction, Vector3 new_target) GetClosestTargetAndDirection(List<Vector2> walkable_dir, Vector3 current_position, Vector3 final_position)
    {
        
        GridManager gridManager = GridManager.Instance;
        Vector3 target = current_position;
        Vector2 direction = new();
        float min_distance = float.MaxValue;
        
        foreach (Vector2 dir in walkable_dir)
        {
            

            Vector3 neighbor_position = gridManager.GetNeighborCellPositionFromWorldDirection(current_position, dir);
            float distance = Vector2.Distance(final_position, neighbor_position);

            if(min_distance > distance) 
            {
                min_distance = distance;
                target = neighbor_position;
                direction = dir;
            }
        }
        if(target == current_position) throw new System.Exception("检查AI_Navigation");

        return (direction, target);
    }
    
    //检测是否要回头
    public static bool IsTurnBack(Vector2 new_dir, Vector2 old_dir)
    {
        
        return new_dir == -old_dir;
    }

    public static List<Vector2> GetDirection()
    {
        return new List<Vector2>()
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
        };
    }

    //检测是否移动到了目标格子的中心
    public static bool HasReachedTargetCellCenter(Vector2 current_position, Vector2 target_position, Vector2 current_direction)
    {
        
        GridManager grid = GridManager.Instance;

        Vector2 current_cell = grid.GetCurrentCellWorldPosition(current_position);
        Vector2 target_cell = grid.GetCurrentCellWorldPosition(target_position);

        //没有进入目标格子前都否，进入格子后判断是否抵达格子中心
        if(current_cell != target_cell) return false;

        return grid.HasMoveGridCenterInDirection(current_direction, current_position);  
    }

}

//获得下一个点位的委托，根据不同的state分配不同的GetPosition
public delegate (Vector2 new_direction, Vector3 new_target) GetNextCellPosition
    (Vector2 current_direction, Vector3 current_position, Vector3 final_position = new());

