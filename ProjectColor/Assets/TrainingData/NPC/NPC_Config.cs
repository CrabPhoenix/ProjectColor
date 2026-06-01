using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责npc属性的配置文件
/// </summary>
[CreateAssetMenu(fileName = "NPC_Config", menuName = "NPC/NPC_Config")]
public class NPC_Config : ScriptableObject
{
    public Vector3 spawnPosition;
    public Vector2 defaultTargetPosition;
    public Vector2 chaseTargetPositionExceptPlayer; //蓝色专用
    public List<Vector3> chamberPoints; 
    public Color npcColor;
    public float speed;
}
