using UnityEngine;

[CreateAssetMenu(fileName = "T_ChaseToDefault", menuName = "FSM/Transition/T_ChaseToDefault")]
public class T_ChaseToDefault : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        return context.level.CurrentState == E_LevelState.defaultDay;    
    }
}
