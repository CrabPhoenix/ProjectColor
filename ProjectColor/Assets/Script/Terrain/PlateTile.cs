using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

/// <summary>
/// 平面地形 Tile，普通单位可以在该地形上移动、部署和关卡配置。
/// </summary>
[CreateAssetMenu(fileName = "PlateTile", menuName = "RuleTile/PlateTile")]
public class PlateTile : RuleTile<PlateTile.Neighbor>
{
    [FormerlySerializedAs("wall_tiles")]
    public List<TileBase> waterTiles = new List<TileBase>();

    /// <summary>
    /// 平面地形 RuleTile 使用的邻接类型。
    /// </summary>
    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int water = 4;
    }

    /// <summary>
    /// 判断当前邻接 Tile 是否满足平面地形规则。
    /// </summary>
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch(neighbor)
        {
            case Neighbor.water:
                return waterTiles.Contains(tile);
        }

        return base.RuleMatch(neighbor, tile);
    }
}
