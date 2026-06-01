using UnityEngine;

public class Blue_Controller : NPC_Controller
{
    //蓝色会追踪玩家，但当靠近玩家时会离开试图前往地图中央
    public override void SetChaseTarget()
    {
        base.SetChaseTarget();

        Vector3 playerPos = Player.position;
        
        if(Vector2.Distance(Player.position, transform.position) < 6f)
        {
            Final_target_position = Config.chaseTargetPositionExceptPlayer;
        }
        else
        {
            Final_target_position = playerPos;
        }
        
    }
}
