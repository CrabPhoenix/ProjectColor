using UnityEngine;

[CreateAssetMenu(fileName = "T_FrightenToEaten", menuName = "FSM/Transition/T_FrightenToEaten")]
public class T_FrightenToEaten : Transition
{
    public override bool ShouldTransition(GameObject owner, StateContext context)
    {
        return owner.GetComponent<NPC_Controller>().isEaten;
    }
}
    

