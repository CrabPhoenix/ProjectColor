using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 负责管理重置游戏的相关事件。
/// </summary>
public class ResetManager : MonoBehaviour
{
    [SerializeField] private float resetDelay = 3f;
    private List<ResetBehavior> resetObjects = new();
    

    private void OnEnable() 
    {
        GameEvent.OnGameStart += InitializeGameStart;
        GameEvent.OnGameWin += StopAll;
        GameEvent.OnGameLose += StopAll;
        GameEvent.OnPlayerDead += InitializePlayerDead;
        GameEvent.OnGameRestart += InitializeLevelRestart;
    }

    private void OnDisable() 
    {
        GameEvent.OnGameStart -= InitializeGameStart;
        GameEvent.OnGameWin -= StopAll;
        GameEvent.OnGameLose -= StopAll;
        GameEvent.OnPlayerDead -= InitializePlayerDead;
        GameEvent.OnGameRestart -= InitializeLevelRestart;
    }

    public void RegisterObjectToReset(ResetBehavior resetObject)
    {
        resetObjects.Add(resetObject);
    }

    #region 延迟一段时间Start

    private void InitializeGameStart()
    {
        StartCoroutine(StartWithDelay());
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        StartAll();
    }

    //启动所有不为空的物体
    private void StartAll()
    {
        foreach (ResetBehavior resetObject in resetObjects)
        {
            if(resetObject == null) continue;

            resetObject.enabled = true;
        }
    }

    #endregion


    #region 死亡后延迟一段时间Reset

    private void InitializePlayerDead()
    {
        StopAll();

        PlayerAttribute player = GameObject.FindWithTag("Player").GetComponent<PlayerAttribute>();
        player.HandleDead();

        if(player.HP > 0) StartCoroutine(PlayerDeadWithDelay());
    }

    private IEnumerator PlayerDeadWithDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        PlayerDead();
    }

    //死亡时启动所有不为空的物体，将玩家移回出生点
    private void PlayerDead()
    {
        foreach (ResetBehavior resetObject in resetObjects)
        {
            if(resetObject == null) continue;

            if(resetObject.GetType() == typeof(PlayerMovement) || resetObject.GetType() == typeof(PlayerAnimator))
            {
                resetObject.Reset_1();
            }

            resetObject.enabled = true;
        }
    }

    #endregion


    #region 将游戏进行重置
    private void InitializeLevelRestart()
    {
        StopAll();

        StartCoroutine(LevelRestartWithDelay());
    }

    private IEnumerator LevelRestartWithDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        GameRestart();
    }

    private void GameRestart()
    {
        foreach (ResetBehavior resetObject in resetObjects.ToList())
        {
            if(resetObject == null) continue;

            resetObject.Reset_1();
            resetObject.Reset_2();
        }

        foreach (ResetBehavior resetObject in resetObjects)
        {
            if(resetObject == null) continue;

            resetObject.enabled = true;
        }
    }

    #endregion

    //暂停所有不为空的物体 
    private void StopAll()
    {
        foreach (ResetBehavior resetObject in resetObjects)
        {
            if(resetObject == null) continue;

            resetObject.enabled = false;
        }
    }

}
