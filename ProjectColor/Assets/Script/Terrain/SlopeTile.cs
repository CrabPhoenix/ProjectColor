using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 斜坡地形 Tile，可在 RuleTile 规则中把平地和水地形作为邻接限制条件。
/// </summary>
[CreateAssetMenu(fileName = "SlopeTile", menuName = "RuleTile/SlopeTile")]
public class SlopeTile : RuleTile<SlopeTile.Neighbor>
{
    public List<TileBase> plateTiles = new List<TileBase>();
    public List<TileBase> waterTiles = new List<TileBase>();

    /// <summary>
    /// 斜坡地形 RuleTile 使用的邻接类型。
    /// </summary>
    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int plate = 3;
        public const int water = 4;
        public const int Null = 5;
        public const int NotNull = 6;
    }

    /// <summary>
    /// 判断当前邻接 Tile 是否满足斜坡地形规则。
    /// </summary>
    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch(neighbor)
        {
            case Neighbor.Null:
                return tile == null;
            case Neighbor.NotNull:
                return tile != null;
            case Neighbor.plate:
                return plateTiles.Contains(tile);
            case Neighbor.water:
                return waterTiles.Contains(tile);
        }

        return base.RuleMatch(neighbor, tile);
    }
}
