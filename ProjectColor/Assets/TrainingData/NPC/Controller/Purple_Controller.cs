using UnityEngine;

public class Purple_Controller : NPC_Controller
{
    //绿色会追踪玩家身前2格的位置
    public override void SetChaseTarget()
    {
        base.SetChaseTarget();

        Vector3 playerPos = Player.position;
        Vector2 playerDir = Player.GetComponent<PlayerMovement>().CurrentMoveDirection;

        Vector3 offset = new Vector3(playerDir.x, playerDir.y, 0) * 2f;

        Final_target_position = playerPos + offset;
    }
}
