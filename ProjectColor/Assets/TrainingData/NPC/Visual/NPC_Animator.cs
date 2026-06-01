using UnityEngine;

/// <summary>
/// 负责怪物不同的状态与移动时的动画
/// </summary>
public class NPC_Animator : MonoBehaviour
{
    private Animator animator;
    private NPC_Controller NPC_Controller;


    void Awake()
    {
        animator = GetComponent<Animator>();
        NPC_Controller = GetComponent<NPC_Controller>();
    }

    void OnEnable()
    {
        NPC_Controller.OnDirectionChange += HandleDirectionChange;
    }

    void OnDisable()
    {
        NPC_Controller.OnDirectionChange -= HandleDirectionChange;
    }

    public void HandleDirectionChange(Vector2 direction)
    {
        animator.SetInteger("move_x", (int)direction.x);
        animator.SetInteger("move_y", (int)direction.y);
    }


    public void SetDefault(bool isDefault)
    {
        animator.SetBool("isDefault", isDefault);
    }


    public void SetEaten(bool isEaten)
    {
        animator.SetBool("isEaten", isEaten);
    }


    public void EnterFrighten()
    {
        //进入恐惧状态
        animator.SetBool("isFrighten", true);
        animator.SetTrigger("frighten");
    }

    public void ExitFrighten()
    {
        //退出恐惧状态
        animator.SetBool("isFrighten", false);
    }

    
    public void EnterFrightenTimeout()
    {
        //进入恐惧将要结束的闪烁状态
        animator.SetTrigger("frighten_timeout");
    }
}
