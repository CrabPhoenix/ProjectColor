using System;
using UnityEngine;

/// <summary>
/// 处理玩家的分数
/// </summary>
public class InGameUI : ResetBehavior
{
    private int score = 0;
    public event Action<int> OnScoreUpdate;
    public int Score 
    { 
        get => score;
        set
        {
            score = value;
            OnScoreUpdate?.Invoke(score);
        } 
        
    }

    void OnEnable()
    {
        GameEvent.OnCookieEaten += HandleScoreUpdate;
    }

    void OnDisable()
    {
        GameEvent.OnCookieEaten -= HandleScoreUpdate;
    }

    private void HandleScoreUpdate()
    {
        Score += 100;
    }

    public override void Reset_1()
    {
        Score = 0;
    }
}
