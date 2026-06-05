using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

/// <summary>
/// 水地形 Tile，普通单位不能在该地形上移动、部署或关卡配置。
/// </summary>
[CreateAssetMenu(fileName = "WaterTile", menuName = "RuleTile/WaterTile")]
public class WaterTile : RuleTile<WaterTile.Neighbor>
{
    [FormerlySerializedAs("path_tiles")]
    public List<TileBase> plateTiles = new List<TileBase>();

    /// <summary>
    /// 水地形 RuleTile 使用的邻接类型。
    /// </summary>
    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int plate = 3;
    }

    /// <summary>
    /// 判断当前邻接 Tile 是否满足水地形规则。
    /// </summary>
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch(neighbor)
        {
            case Neighbor.plate:
                return plateTiles.Contains(tile);
        }

        return base.RuleMatch(neighbor, tile);
    }
}
