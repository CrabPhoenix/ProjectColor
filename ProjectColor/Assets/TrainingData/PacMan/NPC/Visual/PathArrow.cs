using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 将箭头的方向指向移动方向并将颜色改成npc相同的颜色
/// </summary>
public class PathArrow : MonoBehaviour
{
    private SpriteRenderer sprite;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void SetUp(Color color, Vector2 direction)
    {
        sprite.color = color;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
