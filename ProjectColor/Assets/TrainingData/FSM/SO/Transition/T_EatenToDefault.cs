using UnityEngine;

[CreateAssetMenu(fileName = "T_EatenToDefault", menuName = "FSM/Transition/T_EatenToDefault")]
public class T_EatenToDefault : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        return !owner.GetComponent<NPC_Controller>().isEaten && context.level.CurrentState == E_LevelState.defaultDay;
    }
}
    

