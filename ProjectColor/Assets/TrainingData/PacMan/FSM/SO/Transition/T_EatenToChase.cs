using UnityEngine;

[CreateAssetMenu(fileName = "T_EatenToChase", menuName = "FSM/Transition/T_EatenToChase")]
public class T_EatenToChase : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        return !owner.GetComponent<NPC_Controller>().isEaten && context.level.CurrentState == E_LevelState.chaseNight;
    }
}
    

