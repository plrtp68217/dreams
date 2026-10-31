using Unity.Cinemachine;
using UnityEngine;

// These backgrounds are one screen tall. Follow horizontally without exposing the clear color.
[DisallowMultipleComponent]
public class FastFoodCameraHorizon : CinemachineExtension
{
    [SerializeField] private float _height;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage,
        ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize) return;

        Vector3 correction = state.PositionCorrection;
        correction.y += _height - state.GetFinalPosition().y;
        state.PositionCorrection = correction;
    }
}
