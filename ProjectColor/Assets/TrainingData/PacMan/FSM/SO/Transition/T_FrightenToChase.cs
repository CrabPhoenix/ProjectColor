using UnityEngine;

[CreateAssetMenu(fileName = "T_FrightenToChase", menuName = "FSM/Transition/T_FrightenToChase")]
public class T_FrightenToChase : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        return context.level.CurrentState == E_LevelState.chaseNight;
    }
}
