using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 被吃状态，会前往地图中央的chamber并绕一圈后复活
/// </summary>
[CreateAssetMenu(fileName = "S_Eaten", menuName = "FSM/State/S_Eaten")]
public class S_Eaten : State
{
    public override void Enter(GameObject owner, StateContext context)
    {
        owner.GetComponent<NPC_Animator>().SetEaten(true);

        NPC_Controller npc =  owner.GetComponent<NPC_Controller>();
        npc.isEaten = true; 
        npc.getNextCellPosition = AI_Navigation.GetEatenCellPosition;

        //初始化绕圈所需的参数
        List<Vector3> chamberPoints = npc.Config.chamberPoints;

        if(chamberPoints.Count > 0)
        {
            context.chamberPoints = chamberPoints;
            context.currentTargetIndexInChamber = 0;
            context.currentTargetInChamber = chamberPoints[context.currentTargetIndexInChamber];
            context.chamberNumber = chamberPoints.Count;
             
        }
        else
        {
            throw new Exception("检查NPC Config中的chamber");
        }

        npc.Final_target_position = context.currentTargetInChamber; 
    }

    public override void Exit(GameObject owner, StateContext context)
    {       
        owner.GetComponent<NPC_Animator>().SetEaten(false);
        

    }

    public override void Tick(GameObject owner, StateContext context)
    {
        //根据config中的轨迹绕圈
        NPC_Controller npc = owner.GetComponent<NPC_Controller>();
        Vector3 npcPosition = owner.transform.position;

        if(Vector2.Distance(npcPosition, context.currentTargetInChamber) < 0.1f)
        {
            
            context.currentTargetIndexInChamber++;

            if (HasReachedFinalPointIdx(context))
            {
                npc.isEaten = false;
                return;
            }
            
            context.currentTargetInChamber = context.chamberPoints[context.currentTargetIndexInChamber];

            npc.Final_target_position = context.currentTargetInChamber; 
        }

    }

    //判断当前目标是否是最后一个
    private bool HasReachedFinalPointIdx(StateContext context)
    {
        return context.currentTargetIndexInChamber >= context.chamberNumber;
    }
}
