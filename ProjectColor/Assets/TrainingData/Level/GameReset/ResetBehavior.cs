using UnityEngine;

/// <summary>
/// 为monobehaviour附加标签，继承于此的类可以被reset。目前存在bug，当有多个需要reset的script绑定在一个物体时只有第一个会被reset
/// </summary>
public abstract class ResetBehavior : MonoBehaviour
{
    protected ResetManager resetManager;
    protected virtual void Awake()
    {
        resetManager = GameObject.FindWithTag("Level").GetComponent<ResetManager>();
        RegisterReset();
    }

    //根据序列号大小按顺序执行重置
    public abstract void Reset_1();
    public virtual void Reset_2(){}

    private void RegisterReset()
    {
        resetManager.RegisterObjectToReset(this);
    }

    
}
