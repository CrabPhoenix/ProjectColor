using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 控制摄像机在 XY 平面中通过 WASD 进行移动。
/// </summary>
public class CameraPlaneMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;

    private bool canControl = true;

    public bool CanControl => canControl;

    /// <summary>
    /// 设置玩家是否可以手动移动摄像机。
    /// </summary>
    public void SetControlEnabled(bool enabled)
    {
        canControl = enabled;
    }

    /// <summary>
    /// 每帧读取键盘输入并移动摄像机。
    /// </summary>
    private void Update()
    {
        if(!canControl) return;
        if(!GameStageManager.IsCameraManualControlStage()) return;
        if(Keyboard.current == null) return;

        Vector2 inputDirection = Vector2.zero;
        if(Keyboard.current.wKey.isPressed) inputDirection.y += 1f;
        if(Keyboard.current.sKey.isPressed) inputDirection.y -= 1f;
        if(Keyboard.current.aKey.isPressed) inputDirection.x -= 1f;
        if(Keyboard.current.dKey.isPressed) inputDirection.x += 1f;

        if(inputDirection == Vector2.zero) return;

        Vector3 moveDirection = new Vector3(inputDirection.x, inputDirection.y, 0f).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
