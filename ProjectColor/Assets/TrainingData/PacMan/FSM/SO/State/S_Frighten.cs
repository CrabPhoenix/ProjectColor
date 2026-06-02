using UnityEngine;

/// <summary>
/// 惊恐状态，四个npc会随机移动，在快要结束时闪烁。
/// </summary>
[CreateAssetMenu(fileName = "S_Frighten", menuName = "FSM/State/S_Frighten")]
public class S_Frighten : State
{
    public override void Enter(GameObject owner, StateContext context)
    {
        owner.GetComponent<NPC_Animator>().EnterFrighten();

        NPC_Controller npc = owner.GetComponent<NPC_Controller>();

        npc.getNextCellPosition = AI_Navigation.GetFrightenPosition;

        owner.GetComponentInChildren<NPC_Collision>().enabled = true;     
    }

    public override void Exit(GameObject owner, StateContext context)
    {       
        context.frightenTimeoutBefore = false;
        owner.GetComponent<NPC_Animator>().ExitFrighten();

        owner.GetComponentInChildren<NPC_Collision>().enabled = false;
    }

    public override void Tick(GameObject owner, StateContext context)
    {
        if(context.level.FrightenTimer < 3.0f && !context.frightenTimeoutBefore)
        {
            owner.GetComponent<NPC_Animator>().EnterFrightenTimeout();
            context.frightenTimeoutBefore = true;
        }
    }
}
