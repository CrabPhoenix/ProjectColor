using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WallTile", menuName = "RuleTile/WallTile")]
public class WallTile : RuleTile<WallTile.Neighbor> {
    public List<TileBase> path_tiles = new List<TileBase>();

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int path = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.path: return path_tiles.Contains(tile);            
        }
        return base.RuleMatch(neighbor, tile);
    }
}