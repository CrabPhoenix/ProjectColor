using UnityEngine;

/// <summary>
/// 状态机，包含初始状态与所有的transition
/// </summary>
[CreateAssetMenu(menuName = "FSM/StateMachine")]
public class StateMachine : ScriptableObject
{
    public State initialState;
    public Transition[] transitions;

}
