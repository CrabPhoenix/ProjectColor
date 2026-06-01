using System;
using UnityEngine;

/// <summary>
/// 负责游戏中全球型事件的glue代码，提供执行的静态方法和为这些方法提供相应函数的event
/// </summary>
public static class GameEvent
{
    public static event Action OnCookieEaten;
    public static event Action OnSuperCookieEaten;
    public static event Action OnPlayerDead;
    public static event Action OnGameWin;
    public static event Action OnGameLose;
    public static event Action OnGameStart;
    public static event Action OnGameRestart;

    public static void CookieEaten() => OnCookieEaten?.Invoke();
    public static void SuperCookieEaten() => OnSuperCookieEaten?.Invoke();
    public static void PlayerDead() => OnPlayerDead?.Invoke();
    public static void GameWin() => OnGameWin?.Invoke();
    public static void GameLose() => OnGameLose?.Invoke();
    public static void GameStart() => OnGameStart?.Invoke();
    public static void GameRestart() => OnGameRestart?.Invoke();

}
