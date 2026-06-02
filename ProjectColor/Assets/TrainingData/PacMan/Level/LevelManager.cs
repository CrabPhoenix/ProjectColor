using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 负责管理关卡的,时间和对应状态,传送门，cookie生成
/// /// </summary>
public class LevelManager : ResetBehavior
{
    [SerializeField] private PortalConfig portalConfig;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private SuperCookieConfig superCookieConfig;

    //目前存在bug：当frighten time时间设定的特别长时会出现eaten状态的npc在完成轨迹后当关卡处于frighten状态时npc依然会处于eaten状态，
    //因为目前不存在eaten重新转为frighten的transform。所以frighten time时间不能超过10秒
    [SerializeField] private E_LevelState startlevelState = E_LevelState.defaultDay;
    [SerializeField] private float DefaultTime = 5f;
    [SerializeField] private float chaseTime = 10f;
    [SerializeField] private float frightenTime = 10f;

    private E_LevelState previousState;
    private E_LevelState currentState;
    private float normalTimer;
    private float frightenTimer;

    public E_LevelState CurrentState => currentState;
    public PortalConfig PortalConfig => portalConfig;
    public SuperCookieConfig SuperCookieConfig => superCookieConfig; 
    public float FrightenTimer { get => frightenTimer; set => frightenTimer = value; }


    void OnEnable()
    {
        GameEvent.OnSuperCookieEaten += SetFrighten;
    }

    void OnDisable()
    {
        GameEvent.OnSuperCookieEaten -= SetFrighten;
    }


    void Start()
    {
        InitializeTimer();
        CreatePortal();
    }

    

    void Update()
    {
        CheckState();
        DecreaseTime();     
    }

    //根据不同的初始状态进行初始化
    private void InitializeTimer()
    {
        if(startlevelState == E_LevelState.frighten) 
        {
            startlevelState = E_LevelState.defaultDay;
            throw new Exception("不要设置frighten为初始状态,已将初始状态设定为default");
        }
        ResetFrightenTimer();

        if(startlevelState == E_LevelState.defaultDay) 
        {
            normalTimer = DefaultTime;
            currentState = E_LevelState.defaultDay; 
            previousState = currentState;
            Debug.Log("进入default");
            return;
        }
        else if(startlevelState == E_LevelState.chaseNight) 
        {
            normalTimer = chaseTime;
            currentState = E_LevelState.chaseNight;
            previousState = currentState;
            Debug.Log("进入chase");
            return;
        }
        
    }

    //创造关卡中的传送门
    private void CreatePortal()
    {
        GameObject portal1 = Instantiate(portalPrefab, PortalConfig.point1, Quaternion.identity);
        portal1.GetComponent<Portal>().entryDiretion = PortalConfig.entryPoint1Direction;
        portal1.transform.parent = this.transform;

        GameObject portal2 = Instantiate(portalPrefab, PortalConfig.point2, Quaternion.identity);
        portal2.GetComponent<Portal>().entryDiretion = PortalConfig.entryPoint2Direction;
        portal2.transform.parent = this.transform;
    }

    //检查当前状态，若超过时间，切换切换状态
    private void CheckState()
    {
        if(currentState == E_LevelState.frighten && FrightenTimer <= 0)
        {
            currentState = previousState;
            ResetFrightenTimer();
            Debug.Log("离开frighten");
            return;
        }
        if(currentState == E_LevelState.defaultDay && normalTimer <= 0)
        {
            currentState = E_LevelState.chaseNight;
            previousState = currentState;  
            ResetChaseTimer();
            Debug.Log("进入chase");
            return;
        }
        if(currentState == E_LevelState.chaseNight && normalTimer <= 0)
        {
            currentState = E_LevelState.defaultDay;
            previousState = currentState;
            ResetDefaultTimer();
            Debug.Log("进入default");
            return;
        }
                   
    }

    private void ResetChaseTimer()
    {
        normalTimer = chaseTime;
    }

    private void ResetDefaultTimer()
    {
        normalTimer = DefaultTime;
    }

    private void ResetFrightenTimer()
    {
        FrightenTimer = frightenTime;
    }

    private void SetFrighten()
    {
        currentState = E_LevelState.frighten;
    }

    //根据不同的状态计时
    private void DecreaseTime()
    {
        if(currentState == E_LevelState.frighten) FrightenTimer -= Time.deltaTime; 
        else normalTimer -= Time.deltaTime;
    }

    public override void Reset_1()
    {
        InitializeTimer();
    }
}

public enum E_LevelState {defaultDay, chaseNight, frighten}
