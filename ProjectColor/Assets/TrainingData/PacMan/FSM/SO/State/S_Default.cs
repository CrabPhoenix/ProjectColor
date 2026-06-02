using UnityEngine;

/// <summary>
/// 默认状态，四个npc会寻找四个角落
/// </summary>
[CreateAssetMenu(fileName = "S_Default", menuName = "FSM/State/S_Default")]
public class S_Default : State
{
    public override void Enter(GameObject owner, StateContext context)
    {
        owner.GetComponent<NPC_Animator>().SetDefault(true);
        
        NPC_Controller npc =  owner.GetComponent<NPC_Controller>();

        npc.getNextCellPosition = AI_Navigation.GetDefaultCellPosition;
        npc.Final_target_position = npc.Config.defaultTargetPosition;
    }

    public override void Exit(GameObject owner, StateContext context)
    {
        owner.GetComponent<NPC_Animator>().SetDefault(false);
    }

    public override void Tick(GameObject owner, StateContext context)
    {
        
    }
}
