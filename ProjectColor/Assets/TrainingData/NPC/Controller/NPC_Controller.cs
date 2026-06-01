using System;
using NUnit.Framework.Interfaces;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 控制NPC的移动的总类
/// </summary>
public class NPC_Controller : ResetBehavior, ITeleport
{
    [SerializeField] private Transform player;
    
    [SerializeField] private NPC_Config config;

    
    private Vector2 next_target_position;
    private Vector2 current_direction;
    private GridManager grid;
    private LevelManager level;
    private Vector2 final_target_position;
    private float speed;

    public GetNextCellPosition getNextCellPosition {get; set;}
    public event Action<Vector2> OnDirectionChange;
    public NPC_Config Config => config; 
    public Transform Player => player; 
    public Vector2 Next_target_position { get => next_target_position; set => next_target_position = value; }
    public Vector2 Current_direction 
    { 
        get => current_direction;
        set
        {
           //改变方向时广播信号
           current_direction = value;
           OnDirectionChange?.Invoke(current_direction);

        } 
    }
    public Vector2 Final_target_position 
    { 
        get => final_target_position; 
        set => final_target_position = grid.GetCurrentCellWorldPosition(value); 
    }
    public bool isEaten;


    void Start()
    {
        grid = GridManager.Instance;
        level = GameObject.FindWithTag("Level").GetComponent<LevelManager>();
        speed = config.speed;
        InitializeTarget();
    }

    void Update()
    {
        //当移动到目标格子的中心后寻找下一个格子
        
        if(AI_Navigation.HasReachedTargetCellCenter(transform.position, Next_target_position, Current_direction))
        {
            UpdateNewTargetPosition(transform.position, Final_target_position);
        }
        

        //移动到邻格
        Debug.DrawLine(transform.position, Next_target_position, Color.cyan);
        transform.position = Vector2.MoveTowards(transform.position, Next_target_position, speed * Time.deltaTime);

        /*
        Debug.Log(
            $"pos={transform.position} target={Next_target_position} dir={Current_direction}"
        );
        */
    }


    //获得需要的邻格
    private void UpdateNewTargetPosition(Vector2 current_position, Vector2 final_position)
    {
        (Vector2 new_direction, Vector3 new_cell) result = getNextCellPosition(current_direction, current_position, final_position);

        Next_target_position = result.new_cell;
        Current_direction = result.new_direction;   
    }

    //NPC执行的传送
    public void Teleport(Vector3 entryPoint, Vector2Int entrylDirection)
    {
         //获得传送门config中的两点位置，自身触发位置的格子坐标
        PortalConfig config = level.PortalConfig;
        Vector3 point1 = grid.GetCurrentCellWorldPosition(config.point1);
        Vector3 point2 = grid.GetCurrentCellWorldPosition(config.point2);
        Vector3 triggerPoint = grid.GetCurrentCellWorldPosition(entryPoint);

        //玩家方向在传送后保持一致
        Current_direction = entrylDirection;
             
        //根据不同的传送门改变位置，设定向前的目标
        if(triggerPoint == point1)
        {
            Vector3 nextToPortal = grid.GetNeighborCellPositionFromWorldDirection(point2, entrylDirection);
            transform.position = nextToPortal + new Vector3(entrylDirection.x * 0.1f, entrylDirection.x * 0.1f, 0);
            UpdateNewTargetPosition(nextToPortal, final_target_position);
        }
        if(triggerPoint == point2)
        {
            Vector3 nextToPortal = grid.GetNeighborCellPositionFromWorldDirection(point1, entrylDirection);
            transform.position = nextToPortal + new Vector3(entrylDirection.x * 0.1f, entrylDirection.x * 0.1f, 0);
            UpdateNewTargetPosition(nextToPortal, final_target_position);
        }   
    }

    //为npc提供第一个目标
    private void InitializeTarget()
    {
        getNextCellPosition = AI_Navigation.GetDefaultCellPosition;
        Final_target_position = grid.GetStartPosition();
        UpdateNewTargetPosition(transform.position, Final_target_position);
    }
    
    //负责设定追逐目标的虚方法，在不同颜色的npc中分别实现
    public virtual void SetChaseTarget(){}

    public override void Reset_1()
    {
        transform.position = config.spawnPosition;
        
        InitializeTarget();
    }
}
