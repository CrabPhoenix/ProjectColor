using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public partial class GridManager : MonoBehaviour
{
    //singuleton
    private static GridManager instance;
    public static GridManager Instance => instance; //等价于 Instance{get{return instance;}}

    [SerializeField] private Tilemap tilemap;
    
    private GameGrid grid;
    private GridRenderer gridRenderer;
    private bool show_grid;


    void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        InitializeGrid();
    }

    //对格子系统初始化
    private void InitializeGrid()
    {
        //确定瓦片地图的边界
        if(tilemap == null) {return;}
        tilemap.CompressBounds();

        grid = new GameGrid(tilemap.size.x, tilemap.size.y);
        gridRenderer = new GridRenderer(tilemap.origin, grid);

        //将所有瓦片的世界坐标转化为格子坐标并获得它的类型并转化为格子类型
        foreach(GridObject gridObject in grid.GetGridObjects())
        {
            //转化坐标
            Vector3 world_position = gridRenderer.GetWorldPositionFromCell(gridObject.GetCellPosion());
            int val_x = Mathf.FloorToInt(world_position.x);
            int val_y = Mathf.FloorToInt(world_position.y);
            TileBase tile = tilemap.GetTile(new Vector3Int(val_x, val_y , 0));

            gridObject.type = GridObjectType.Empty;
            gridObject.terrainType = TerrainType.None;
            if(tile == null) continue;

            //获得并转化其类型
            Type tile_type = tile.GetType();

            if(tile_type == typeof(PlateTile))
            {
                gridObject.type = GridObjectType.Path;
                gridObject.terrainType = TerrainType.Plate;
            }
            else if(tile_type == typeof(WaterTile))
            {
                gridObject.type = GridObjectType.Wall;
                gridObject.terrainType = TerrainType.Water;
            }
            else if(tile_type == typeof(SlopeTile))
            {
                gridObject.type = GridObjectType.Path;
                gridObject.terrainType = TerrainType.Slope;
            }
        }
    }


    #if UNITY_EDITOR
    // 绘制Gizmos图
    private void OnDrawGizmos()
    {
        if(tilemap == null || grid == null || show_grid) {return;}

        
        Gizmos.color = Color.cyan;
        GridObject[,] gridObjects = grid.GetGridObjects();

        //画出grids的下边与左边，并在格子中间写出其位置与类型，内部格子的上边与右边由后续格子的下边与左边覆盖
        foreach(GridObject gridObject in gridObjects)
        {
            //画出grids的下边与左边
            GridCell current_cell = gridObject.GetCellPosion();
            Vector3 cell_world_position = gridRenderer.GetWorldPositionFromCell(current_cell);
            
            int world_pos_x = (int)cell_world_position.x;
            int world_pos_y = (int)cell_world_position.y;

            Vector3 vertical_start = new Vector3(world_pos_x, world_pos_y);
            Vector3 vertical_end = new Vector3(world_pos_x, world_pos_y + 1);
            Vector3 horizontal_start = new Vector3(world_pos_x, world_pos_y);
            Vector3 horizontal_end = new Vector3(world_pos_x + 1, world_pos_y);

            Gizmos.DrawLine(vertical_start, vertical_end);
            Gizmos.DrawLine(horizontal_start, horizontal_end);

            //区分墙壁和路径
            Vector3 cell_center = new Vector3(world_pos_x + 0.5f, world_pos_y + 0.5f);
            GUIStyle text_style = new GUIStyle();
            text_style.alignment = TextAnchor.MiddleCenter;

            if(gridObject.terrainType == TerrainType.Water)
            {
                text_style.normal.textColor = Color.blue;
                Handles.Label(cell_center, $"({current_cell.X}, {current_cell.Y})", text_style);
            }
            else if(gridObject.terrainType == TerrainType.Plate)
            {
                text_style.normal.textColor = Color.white;
                Handles.Label(cell_center, $"({current_cell.X}, {current_cell.Y})", text_style);
            }
            else if(gridObject.terrainType == TerrainType.Slope)
            {
                text_style.normal.textColor = Color.green;
                Handles.Label(cell_center, $"({current_cell.X}, {current_cell.Y})", text_style);
            }
            else if(gridObject.terrainType == TerrainType.None)
            {
                text_style.normal.textColor = Color.gray;
                Handles.Label(cell_center, "None", text_style);
            }
                
        }

        //统一画出所有外围格子的右边与上边
        int tilemap_width = grid.Width;
        int tilemap_height = grid.Height;

        Vector3 first_cell_world_space = gridRenderer.GetWorldPositionFromCell(new GridCell(0,0));
        Vector3 last_cell_world_endSpace = gridRenderer.GetWorldPositionFromCell(new GridCell(tilemap_width, tilemap_height));

        float first_world_space_x = first_cell_world_space.x;
        float first_world_space_y = first_cell_world_space.y;
        
        float last_world_space_x = last_cell_world_endSpace.x;
        float last_world_space_y = last_cell_world_endSpace.y;

        Vector3 final_horizontal_start = new Vector3(first_world_space_x, last_world_space_y);
        Vector3 final_vertical_start = new Vector3(last_world_space_x, first_world_space_y);
        Vector3 final_end = new Vector3(last_world_space_x, last_world_space_y);

        Gizmos.DrawLine(final_horizontal_start, final_end);
        Gizmos.DrawLine(final_vertical_start, final_end);

        
    }
    #endif

    public void RegenerateGrid()
    {
        InitializeGrid();
    }


    public void ToggleVisibility()
    {
        show_grid = !show_grid;
    }
}

