using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 根据回合阶段控制摄像机手动移动和自动跟随单位。
/// </summary>
public class TurnPhaseCameraController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private CameraPlaneMovement cameraPlaneMovement;
    [SerializeField] private float followSpeed = 12f;

    private Unit followTarget;
    private float cameraZ;

    /// <summary>
    /// 初始化摄像机引用和深度。
    /// </summary>
    private void Awake()
    {
        ResolveReferences();
        cameraZ = GetControlledTransform().position.z;
    }

    /// <summary>
    /// 在摄像机更新后跟随当前行动单位。
    /// </summary>
    private void LateUpdate()
    {
        if(followTarget == null) return;

        Vector3 targetPosition = GetCameraPositionAbove(followTarget.transform.position);
        Transform controlledTransform = GetControlledTransform();
        controlledTransform.position = Vector3.Lerp(controlledTransform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 设置玩家是否可以手动移动摄像机。
    /// </summary>
    public void SetManualControlEnabled(bool enabled)
    {
        ResolveReferences();
        if(cameraPlaneMovement != null)
        {
            cameraPlaneMovement.SetControlEnabled(enabled);
        }
    }

    /// <summary>
    /// 立即将摄像机放到指定单位上方。
    /// </summary>
    public void FocusOnUnit(Unit unit)
    {
        if(unit == null) return;

        followTarget = null;
        GetControlledTransform().position = GetCameraPositionAbove(unit.transform.position);
    }

    /// <summary>
    /// 跟随指定单位并关闭玩家手动控制。
    /// </summary>
    public void FollowUnit(Unit unit)
    {
        SetManualControlEnabled(false);
        followTarget = unit;
        if(unit != null)
        {
            GetControlledTransform().position = GetCameraPositionAbove(unit.transform.position);
        }
    }

    /// <summary>
    /// 停止跟随当前单位。
    /// </summary>
    public void StopFollowing()
    {
        followTarget = null;
    }

    /// <summary>
    /// 随机聚焦一个玩家单位并恢复手动控制。
    /// </summary>
    public void FocusRandomPlayerUnit()
    {
        UnitGridOccupancy.RebuildFromScene();
        List<Unit> playerUnits = UnitGridOccupancy.GetAliveUnits(UnitTeam.Player);
        StopFollowing();
        SetManualControlEnabled(true);

        if(playerUnits.Count == 0) return;

        int index = Random.Range(0, playerUnits.Count);
        FocusOnUnit(playerUnits[index]);
    }

    /// <summary>
    /// 获得保持当前 Z 深度的单位上方摄像机位置。
    /// </summary>
    private Vector3 GetCameraPositionAbove(Vector3 unitPosition)
    {
        return new Vector3(unitPosition.x, unitPosition.y, cameraZ);
    }

    /// <summary>
    /// 自动补齐摄像机和移动控制组件引用。
    /// </summary>
    private void ResolveReferences()
    {
        if(targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if(targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if(targetCamera != null && targetCamera.transform != transform)
        {
            cameraZ = targetCamera.transform.position.z;
        }

        if(cameraPlaneMovement == null)
        {
            cameraPlaneMovement = GetComponent<CameraPlaneMovement>();
        }

        if(cameraPlaneMovement == null && targetCamera != null)
        {
            cameraPlaneMovement = targetCamera.GetComponent<CameraPlaneMovement>();
        }
    }

    /// <summary>
    /// 获得实际被控制的摄像机 Transform。
    /// </summary>
    private Transform GetControlledTransform()
    {
        if(targetCamera != null)
        {
            return targetCamera.transform;
        }

        return transform;
    }
}
