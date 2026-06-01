using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 处理玩家的碰撞
/// </summary>
public class PlayerCollision : ResetBehavior
{
    private LevelManager level;

    void Start()
    {
        level = GameObject.FindWithTag("Level").GetComponent<LevelManager>();
    }


    private void OnTriggerEnter2D(Collider2D other) 
    {

        if ( ShouldListenCollision(other) && GetComponent<PlayerCollision>().enabled)
        {
            GameEvent.PlayerDead();
        }
    }

    //当其是npc，不处于frighten与eaten状态时为真
    private bool ShouldListenCollision(Collider2D other)
    {
        if(!other.CompareTag("NPC")) return false;

        bool isFrighten = level.CurrentState == E_LevelState.frighten;
        bool isEaten = other.GetComponentInParent<NPC_Controller>().isEaten;

        return !isEaten && !isFrighten;
    }

    public override void Reset_1()
    {
        
    }
}
