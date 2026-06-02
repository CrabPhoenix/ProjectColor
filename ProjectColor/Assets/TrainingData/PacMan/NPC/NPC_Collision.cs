using UnityEngine;

/// <summary>
/// 监听NPC的碰撞
/// </summary>
public class NPC_Collision : MonoBehaviour
{
    private LevelManager level;

    void Start()
    {
        level = GameObject.FindWithTag("Level").GetComponent<LevelManager>();   
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.CompareTag("Player") && GetComponent<NPC_Collision>().enabled && level.CurrentState == E_LevelState.frighten)
        {
            transform.parent.GetComponent<NPC_Controller>().isEaten = true;
        }
    }
}
