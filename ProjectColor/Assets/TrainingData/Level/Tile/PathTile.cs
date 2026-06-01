using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "PathTile", menuName = "RuleTile/PathTile")]
public class PathTile : RuleTile<PathTile.Neighbor>
{
    public List<TileBase> wall_tiles = new ();

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int wall = 4;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.wall: return wall_tiles.Contains(tile);            
        }
        return base.RuleMatch(neighbor, tile);
    }    
}
