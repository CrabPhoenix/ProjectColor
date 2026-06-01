using UnityEngine;

/// <summary>
/// 追逐状态，npc会根据特定算法追逐玩家
/// </summary>
[CreateAssetMenu(fileName = "S_Chase", menuName = "FSM/State/S_Chase")]
public class S_Chase : State
{
    public override void Enter(GameObject owner, StateContext context)
    {
        owner.GetComponent<NPC_Animator>().SetDefault(true);
        owner.GetComponent<NPC_Controller>().getNextCellPosition = AI_Navigation.GetDefaultCellPosition;
    }

    public override void Exit(GameObject owner, StateContext context)
    {
        owner.GetComponent<NPC_Animator>().SetDefault(false);
    }

    public override void Tick(GameObject owner, StateContext context)
    {
        owner.GetComponent<NPC_Controller>().SetChaseTarget();
    }
}
