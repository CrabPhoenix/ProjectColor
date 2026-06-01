using UnityEngine;

/// <summary>
/// 传送门的config，目前只能在两点之间传
/// </summary>
[CreateAssetMenu(fileName = "PortalConfig", menuName = "Level/PortalConfig")]
public class PortalConfig : ScriptableObject
{
    public Vector2 point1;
    public Vector2Int entryPoint1Direction;
    public Vector2 point2;
    public Vector2Int entryPoint2Direction;
}
