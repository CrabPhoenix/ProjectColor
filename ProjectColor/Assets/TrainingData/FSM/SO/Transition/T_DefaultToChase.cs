using UnityEngine;

[CreateAssetMenu(fileName = "T_DefaultToChase", menuName = "FSM/Transition/T_DefaultToChase")]
public class T_DefaultToChase : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        return context.level.CurrentState == E_LevelState.chaseNight;
        
    }
}
