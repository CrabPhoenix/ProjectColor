using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在游戏中显示npc当前的预期路径
/// </summary>
public class NPC_Debugger : MonoBehaviour
{
    [SerializeField] private GameObject visulizer;
    
    private NPC_Controller npc;
    private List<(Vector2 dir, Vector3 pos)> path_positions = new();
    private List<GameObject> path_visulizers = new();
    private GridManager gridManager;
    private NPC_Config config;
    private Transform debug;

    
    void Start()
    {
        gridManager = GridManager.Instance;
        npc = GetComponent<NPC_Controller>();
        config = npc.Config;
        debug = GameObject.FindGameObjectWithTag("Debug").transform;
    }

    
    void Update()
    {
        CleanUp();
        UpdatePath();
        UpdateVisulization();
    }
 

    private void UpdatePath()
    {
        //通过当前移动方向，终点和下一个目标格以在路径的每个格子的中心点生成标记
        Vector2 final_target = npc.Final_target_position;
        Vector2 current_direction = npc.Current_direction;
        (Vector2 newDir, Vector2 newTarget) nextResult = (current_direction, npc.Next_target_position);
        
        int counter = 100;
        
        while (nextResult.newTarget != final_target && counter > 0)
        {
            //通过当前位置获得下一个位置并递归直至与目标点重合
            nextResult = npc.getNextCellPosition(nextResult.newDir, nextResult.newTarget, final_target);

            if(npc.getNextCellPosition == AI_Navigation.GetFrightenPosition) return;

            path_positions.Add(nextResult);

            counter --;
        }
        
    }


    private void UpdateVisulization()
    {
        foreach ((Vector2 dir, Vector2 pos) pathPos in path_positions)
        {
            GameObject tempgo = Instantiate(visulizer, pathPos.pos, Quaternion.identity);
            tempgo.transform.parent = debug;
            path_visulizers.Add(tempgo);
           
            tempgo.GetComponent<PathArrow>().SetUp(config.npcColor, pathPos.dir);
        }
    }


    private void CleanUp()
    {
        path_positions.Clear();

        foreach (GameObject tempgo in path_visulizers)
        {
            Destroy(tempgo);
        }
        path_visulizers.Clear();
    }
}
