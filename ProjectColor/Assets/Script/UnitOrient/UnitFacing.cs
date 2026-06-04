using System;
using UnityEngine;

/// <summary>
/// 记录单位当前朝向，并让单位本地 Y 轴正方向指向该朝向。
/// </summary>
public class UnitFacing : MonoBehaviour
{
    [SerializeField] private Direction currentDirection = Direction.Right;

    private Unit unit;

    public Direction CurrentDirection => currentDirection;
    public event Action<UnitFacing> OnFacingChanged;

    /// <summary>
    /// 缓存单位组件。
    /// </summary>
    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    /// <summary>
    /// 将单位朝向设置为指定方向。
    /// </summary>
    public void Face(Direction direction)
    {
        if(direction == Direction.Invalid) return;
        if(currentDirection == direction)
        {
            ApplyRotation();
            return;
        }

        currentDirection = direction;
        ApplyRotation();
        OnFacingChanged?.Invoke(this);
    }

    /// <summary>
    /// 根据阵营设置默认朝向。
    /// </summary>
    public void FaceTeamDefault(UnitTeam team)
    {
        Face(GetDefaultDirection(team));
    }

    /// <summary>
    /// 获得正面相邻格。
    /// </summary>
    public GridCell GetFrontCell()
    {
        return GetNeighborCell(currentDirection);
    }

    /// <summary>
    /// 获得背面相邻格。
    /// </summary>
    public GridCell GetBackCell()
    {
        return GetNeighborCell(GetOppositeDirection(currentDirection));
    }

    /// <summary>
    /// 获得两个侧面相邻格。
    /// </summary>
    public GridCell[] GetSideCells()
    {
        Direction[] sideDirections = GetSideDirections(currentDirection);
        return new[]
        {
            GetNeighborCell(sideDirections[0]),
            GetNeighborCell(sideDirections[1])
        };
    }

    /// <summary>
    /// 根据攻击者位置判断攻击来自当前单位的哪一面。
    /// </summary>
    public UnitFacingSide GetIncomingSide(Unit attacker)
    {
        Direction incomingDirection = GetIncomingDirection(attacker);
        return GetSideFromIncomingDirection(incomingDirection);
    }

    /// <summary>
    /// 根据攻击者位置获得攻击来源方向。
    /// </summary>
    public Direction GetIncomingDirection(Unit attacker)
    {
        if(attacker == null) return Direction.Invalid;

        Vector2 incomingVector = attacker.transform.position - transform.position;
        return GetClosestDirection(incomingVector);
    }

    /// <summary>
    /// 根据攻击来源方向判断其属于正面、侧面或背面。
    /// </summary>
    public UnitFacingSide GetSideFromIncomingDirection(Direction incomingDirection)
    {
        if(incomingDirection == currentDirection) return UnitFacingSide.Front;
        if(incomingDirection == GetOppositeDirection(currentDirection)) return UnitFacingSide.Back;

        return UnitFacingSide.Side;
    }

    /// <summary>
    /// 获得指定阵营的默认朝向。
    /// </summary>
    public static Direction GetDefaultDirection(UnitTeam team)
    {
        switch(team)
        {
            case UnitTeam.Player:
            case UnitTeam.Ally:
                return Direction.Right;
            case UnitTeam.Enemy:
                return Direction.Left;
            case UnitTeam.Neutral:
                return Direction.Down;
            default:
                return Direction.Right;
        }
    }

    /// <summary>
    /// 根据世界方向向量获得最接近的四方向朝向。
    /// </summary>
    public static Direction GetClosestDirection(Vector2 worldDirection)
    {
        if(worldDirection.sqrMagnitude <= Mathf.Epsilon) return Direction.Invalid;

        worldDirection.Normalize();
        Direction bestDirection = Direction.Invalid;
        float bestDot = float.NegativeInfinity;

        TrySelectCloserDirection(worldDirection, Direction.Right, Vector2.right, ref bestDirection, ref bestDot);
        TrySelectCloserDirection(worldDirection, Direction.Up, Vector2.up, ref bestDirection, ref bestDot);
        TrySelectCloserDirection(worldDirection, Direction.Left, Vector2.left, ref bestDirection, ref bestDot);
        TrySelectCloserDirection(worldDirection, Direction.Down, Vector2.down, ref bestDirection, ref bestDot);

        return bestDirection;
    }

    /// <summary>
    /// 获得指定方向的相反方向。
    /// </summary>
    public static Direction GetOppositeDirection(Direction direction)
    {
        switch(direction)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            default:
                return Direction.Invalid;
        }
    }

    /// <summary>
    /// 获得指定方向两侧的方向。
    /// </summary>
    public static Direction[] GetSideDirections(Direction direction)
    {
        switch(direction)
        {
            case Direction.Up:
            case Direction.Down:
                return new[] { Direction.Left, Direction.Right };
            case Direction.Left:
            case Direction.Right:
                return new[] { Direction.Up, Direction.Down };
            default:
                return new[] { Direction.Invalid, Direction.Invalid };
        }
    }

    /// <summary>
    /// 将当前朝向转换为单位旋转。
    /// </summary>
    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, GetZRotation(currentDirection));
    }

    /// <summary>
    /// 获得指定方向对应的 Z 轴旋转角。
    /// </summary>
    private float GetZRotation(Direction direction)
    {
        switch(direction)
        {
            case Direction.Up:
                return 0f;
            case Direction.Right:
                return -90f;
            case Direction.Down:
                return 180f;
            case Direction.Left:
                return 90f;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// 获得当前单位在指定方向上的相邻格。
    /// </summary>
    private GridCell GetNeighborCell(Direction direction)
    {
        if(unit == null || GridManager.Instance == null) return default;

        return GridManager.Instance.GetNeighborCell(unit.CurrentCell, direction);
    }

    /// <summary>
    /// 比较当前候选方向是否更接近目标世界方向。
    /// </summary>
    private static void TrySelectCloserDirection(Vector2 worldDirection, Direction candidateDirection, Vector2 candidateVector, ref Direction bestDirection, ref float bestDot)
    {
        float dot = Vector2.Dot(worldDirection, candidateVector);
        if(dot <= bestDot) return;

        bestDot = dot;
        bestDirection = candidateDirection;
    }
}
