using UnityEngine;

/// <summary>
/// 追踪特定格子上的object，瓦片类型
/// </summary>
public class GridObject
{
    private GridCell cell_position;

    public GridCell GetCellPosion() => cell_position;
    public GridObjectType type = GridObjectType.Empty;
    public TerrainType terrainType = TerrainType.None;


    public GridObject(GridCell cell_position)
    {   
        this.cell_position = cell_position;    
    }


    public override string ToString()
    {
        return "Grid Object: " + cell_position;
    }


}

public enum GridObjectType { Path, Chamber, Wall, Empty }
