using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// 负责玩家移动时的动画,目前暂时关闭了播放死亡动画
/// </summary>
public class PlayerAnimator : ResetBehavior
{
    private Animator animator;
    private PlayerMovement playerMovement;


    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }


    void OnEnable()
    {
        playerMovement.OnDirectionChange += HandleDirectionChange;
        GameEvent.OnPlayerDead += TriggerPlayerDead;
    }

    private void OnDisable()
    {
        playerMovement.OnDirectionChange -= HandleDirectionChange;
        GameEvent.OnPlayerDead -= TriggerPlayerDead;
    }

    private void TriggerPlayerDead()
    {
        animator.SetTrigger("dead");
    }

    private void HandleDirectionChange(Vector2 direction)
    {
        animator.SetInteger("move_x", (int)direction.x);
        animator.SetInteger("move_y", (int)direction.y);
    }

    public override void Reset_1()
    {
        HandleDirectionChange(playerMovement.CurrentMoveDirection);
    }
}
