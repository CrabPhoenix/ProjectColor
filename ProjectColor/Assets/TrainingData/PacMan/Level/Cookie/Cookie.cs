using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// 处理cookie相关的事件,当场上所有cookie被吃掉时胜利
/// </summary>
public class Cookie : ResetBehavior
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        //当与玩家接触时自毁
        if(!other.CompareTag("Player") || !GetComponent<Cookie>().enabled) return;

        if(EatAllCookies()) GameEvent.GameWin();

        if(CompareTag("SuperCookie")) GameEvent.SuperCookieEaten();

        GameEvent.CookieEaten();
        
        Destroy(gameObject);
    }

    //判断是否吃掉所有饼干，因为吃最后一个时会出现异步情况，因此在吃最后一个时判断其是否为最后一个
    private bool EatAllCookies()
    {
        return transform.parent.childCount == 1;
    }

    public override void Reset_1()
    {
        Destroy(gameObject);
    }
}
