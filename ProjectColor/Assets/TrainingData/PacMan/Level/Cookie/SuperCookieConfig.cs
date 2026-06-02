using UnityEngine;

/// <summary>
/// 保存所有superCookie生成位置的config
/// </summary>
[CreateAssetMenu(fileName = "SuperCookieConfig", menuName = "Level/SuperCookieConfig")]
public class SuperCookieConfig : ScriptableObject
{
    public Vector2[] positions;
}
