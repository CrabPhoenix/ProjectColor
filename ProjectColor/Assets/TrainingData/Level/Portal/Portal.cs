using UnityEngine;

/// <summary>
/// 负责检测是否触发传送门碰撞并执行传送
/// </summary>
public class Portal : MonoBehaviour
{
    public Vector2Int entryDiretion {get; set;} 

    private void OnTriggerEnter2D(Collider2D other) 
    {
        other.GetComponentInParent<ITeleport>().Teleport(transform.position, entryDiretion);
    }
}
