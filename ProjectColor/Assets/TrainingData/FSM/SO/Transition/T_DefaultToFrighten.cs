using UnityEngine;

[CreateAssetMenu(fileName = "T_DefaultToFrighten", menuName = "FSM/Transition/T_DefaultToFrighten")]
public class T_DefaultToFrighten : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        //需要后续修改
        return context.level.CurrentState == E_LevelState.frighten;
    }
}

