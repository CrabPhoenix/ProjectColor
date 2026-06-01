using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责state需要用到的属于单独个体的临时变量
/// </summary>
public class StateContext
{
    public List<Vector3> chamberPoints;
    public int chamberNumber;
    public Vector3 currentTargetInChamber;
    public int currentTargetIndexInChamber;

    public LevelManager level;

    public bool frightenTimeoutBefore = false;

    public StateContext(LevelManager level)
    {
        this.level = level;
    }

    
}
