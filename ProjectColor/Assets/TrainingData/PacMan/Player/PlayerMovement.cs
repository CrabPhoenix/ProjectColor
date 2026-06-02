using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

/// <summary>
/// 负责玩家的移动
/// </summary>
public class PlayerMovement : ResetBehavior, ITeleport
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private float speed = 5.0f;
    private bool isMoving;
    private Vector2 previous_move_direction;
    private Vector2 currentMoveDirection;
    private Vector2 move_target;
    private GridManager grid;
    private LevelManager level;

    public Vector2 CurrentMoveDirection
    {
        get => currentMoveDirection;
        set
        {
            currentMoveDirection = value;
            OnDirectionChange?.Invoke(currentMoveDirection);
        }
    } 

    public event Action<Vector2> OnDirectionChange;

    void OnDisable()
    {
        OnDirectionChange?.Invoke(Vector2.zero);
    }

    void Start()
    {
        grid = GridManager.Instance;
        level = GameObject.FindWithTag("Level").GetComponent<LevelManager>();

        InitializeDirections();
    }

    void Update()
    {
        if (!isMoving) { return; }

        //当玩家没有经过格子中点时不让玩家改变移动方向的轴
        if (!grid.HasMoveGridCenterInDirection(previous_move_direction, transform.position))
        {
            move_target = GetTarget(previous_move_direction);
        }
        else
        {
            move_target = GetTarget(CurrentMoveDirection); 
        }
        Move();
    }

    #region 处理玩家的移动

    //确认玩家的移动状态，按下时允许移动，松开时禁止移动，获得玩家的输入的方向
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            isMoving = true;
            return;
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            isMoving = false;
            return;
        }

        Vector2 new_move_direction = context.ReadValue<Vector2>();
        if (!IsMovingDirectionValid(new_move_direction)) return;
        
        HandleDirection(new_move_direction);

        //广播方向修改的信号
        OnDirectionChange?.Invoke(new_move_direction);
    }

    //限制玩家的输入方向为4向
    private bool IsMovingDirectionValid(Vector2 move_direction)
    {
        
        if(Mathf.Abs(move_direction.x) == 1 && Mathf.Abs(move_direction.y) == 0){return true;}
        if(Mathf.Abs(move_direction.x) == 0 && Mathf.Abs(move_direction.y) == 1){return true;}
        return false;
    }

    //处理输入方向，若坐标轴相同则正常，若不相同则记录此前的方向
    private void HandleDirection(Vector2 new_move_direction)
    {
        previous_move_direction = CurrentMoveDirection;
        CurrentMoveDirection = new_move_direction;

        if (IsDirectionsInSameAxis())
        {
            //前后方向的轴相同则统一为新方向
            previous_move_direction = new_move_direction;
            CurrentMoveDirection = new_move_direction;
        }
    }

    //判断前后两次输入的方向是否在同一坐标轴
    private bool IsDirectionsInSameAxis()
    {
        if(CurrentMoveDirection == previous_move_direction) return false;
        if(Mathf.Abs(CurrentMoveDirection.x) != Mathf.Abs(previous_move_direction.x) || 
            Mathf.Abs(CurrentMoveDirection.y) != Mathf.Abs(previous_move_direction.y) ) return false;
        
        return true;
    }

    //移动
    private void Move()
    {
        Debug.DrawRay(transform.position, (Vector3)move_target - transform.position, Color.cyan);
        transform.position = Vector2.MoveTowards(transform.position, move_target, speed * Time.deltaTime);
    }

    

    #endregion

    //获得下一个目标
    private Vector3 GetTarget(Vector2 move_direction)
    {
        if(move_direction == Vector2.zero) return transform.position;
        
        /*进行激光检测并确定目标。
        RaycastHit2D hit = Physics2D.Raycast(transform.position, move_direction, Mathf.Infinity, LayerMask.GetMask("Tilemap"));
        if(hit.collider == null) {return transform.position;}

        if(Vector2.Distance(transform.position, hit.point) > grid.GetGridSize()) 
        {
            return grid.GetNeighborPositionFromDirection(transform.position, move_direction);
        }*/

        if(grid.IsPlayerNeighborCellWalkable(transform.position, move_direction)) 
        {
            return grid.GetNeighborCellPositionFromWorldDirection(transform.position, move_direction);
        }

        return grid.GetCurrentCellWorldPosition(transform.position);
    }

    //玩家执行的传送
    public void Teleport(Vector3 entryPoint, Vector2Int entrylDirection)
    {
        //获得传送门config中的两点位置，自身触发位置的格子坐标
        PortalConfig config = level.PortalConfig;
        Vector3 point1 = grid.GetCurrentCellWorldPosition(config.point1);
        Vector3 point2 = grid.GetCurrentCellWorldPosition(config.point2);
        Vector3 triggerPoint = grid.GetCurrentCellWorldPosition(entryPoint);

        //玩家方向在传送后保持一致
        CurrentMoveDirection = entrylDirection;
        previous_move_direction = entrylDirection;       

        //根据不同的传送门改变位置，设定向前的目标
        if(triggerPoint == point1)
        {
            Vector3 nextToPortal = grid.GetNeighborCellPositionFromWorldDirection(point2, entrylDirection);
            transform.position = nextToPortal + new Vector3(entrylDirection.x * 0.1f, entrylDirection.x * 0.1f, 0);
            move_target = grid.GetNeighborCellPositionFromWorldDirection(nextToPortal, entrylDirection);
        }
        if(triggerPoint == point2)
        {
            Vector3 nextToPortal = grid.GetNeighborCellPositionFromWorldDirection(point1, entrylDirection);
            transform.position = nextToPortal + new Vector3(entrylDirection.x * 0.1f, entrylDirection.x * 0.1f, 0);
            move_target = grid.GetNeighborCellPositionFromWorldDirection(nextToPortal, entrylDirection);   
        }

    }

    #region 初始化相关

    //将玩家移动到出生点
    public override void Reset_1()
    {
        transform.position = config.spawnPosition;
        InitializeDirections();
    }

    //开始时向右走
    private void InitializeDirections()
    {
        isMoving = true;
        previous_move_direction = Vector2.right;
        CurrentMoveDirection = Vector2.right;
    }

    #endregion

}
