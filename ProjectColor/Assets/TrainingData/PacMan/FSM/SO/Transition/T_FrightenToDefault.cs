using UnityEngine;

[CreateAssetMenu(fileName = "T_FrightenToDefault", menuName = "FSM/Transition/T_FrightenToDefault")]
public class T_FrightenToDefault : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        return context.level.CurrentState == E_LevelState.defaultDay;
    }
}
