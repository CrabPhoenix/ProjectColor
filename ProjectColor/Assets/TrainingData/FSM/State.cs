using System.Net.Http.Headers;
using UnityEngine;

/// <summary>
/// 状态的SO抽象类，tick就是update的行为
/// </summary>
public abstract class State : ScriptableObject
{
    public abstract void Enter(GameObject owner, StateContext context);
    public abstract void Tick(GameObject owner, StateContext context);
    public abstract void Exit(GameObject owner, StateContext context);
}
