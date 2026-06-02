using System;
using UnityEngine;

/// <summary>
/// 负责记录并处理玩家数据与相关事件
/// </summary>
public class PlayerAttribute : ResetBehavior
{
    public int maxHp = 3;
    private int Hp;
    public event Action<int> OnHpUpdate;

    public int HP 
    { 
        get => Hp;
        set
        {
            Hp = value;
            if(Hp <= 0)
            {
                GameEvent.GameLose();
            }
            OnHpUpdate?.Invoke(Hp);
        }
    }


    void Start()
    {
        HP = maxHp;
    }

    public void HandleDead()
    {
        HP--;
    }

    public override void Reset_1()
    {
        HP = 3;
    }
}
