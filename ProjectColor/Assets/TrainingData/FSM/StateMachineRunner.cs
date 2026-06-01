using UnityEngine;
using System;
using UnityEngine.InputSystem;


/// <summary>
/// 在gameobject上执行FSM
/// </summary>
public class StateMachineRunner : ResetBehavior
{
    public StateMachine stateMachine;
    private State currentState;
    private StateContext context;
    private LevelManager level;


    void Start()
    {
        level = GameObject.FindWithTag("Level").GetComponent<LevelManager>();
        if(level == null) throw new Exception ("检查FSM Runner中的level获取");
        context = new StateContext(level);

        currentState = stateMachine.initialState;
        currentState.Enter(gameObject, context);
    }


    void Update()
    {
        //检测所有的transition，如果符合转化条件的且处于其from状态时退出当前状态，设定新状态，进入新状态
        foreach (Transition transition in stateMachine.transitions)
        {
            if(currentState != transition.fromState) continue;

            if(transition.ShouldTransition(gameObject, context))
            {
                currentState.Exit(gameObject, context);
                currentState = transition.toState;
                currentState.Enter(gameObject, context);
                return;
            }
        }

        currentState.Tick(gameObject, context);
    }
    
    public override void Reset_1()
    {
        
    }

}
