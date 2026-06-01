using UnityEngine;

/// <summary>
/// 负责portal传送的端口
/// </summary>
public interface ITeleport
{
    public abstract void Teleport(Vector3 entryPoint, Vector2Int entrylDirection);
}
