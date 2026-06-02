using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Transition的SO抽象类，需实现起始状态，终点状态与变化状态的条件
/// </summary>
public abstract class Transition : ScriptableObject
{
    public State fromState;
    public State toState;
    
    public abstract bool ShouldTransition(GameObject owner, StateContext context); 

}
