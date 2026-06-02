using UnityEngine;

[CreateAssetMenu(fileName = "T_ChaseToFrighten", menuName = "FSM/Transition/T_ChaseToFrighten")]
public class T_ChaseToFrighten : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        //需要后续修改
        return context.level.CurrentState == E_LevelState.frighten;
    }
}
