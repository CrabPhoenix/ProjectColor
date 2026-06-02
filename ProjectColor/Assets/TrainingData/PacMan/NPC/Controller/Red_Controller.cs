using UnityEngine;

public class Red_Controller : NPC_Controller
{
    //红色会追踪玩家的位置
    public override void SetChaseTarget()
    {
        base.SetChaseTarget();

        Vector3 playerPos = Player.position;

        Final_target_position = playerPos;
    }
}
