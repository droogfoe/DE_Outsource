using UnityEngine;
using Cinemachine;


public class CM_POVExtension : CinemachineExtension {
    [SerializeField] float clampAngle = 80;
    [SerializeField] float hSpd = 10;
    [SerializeField] float vSpd = 10;

    private Utage.InputManager inputManager;
    private Vector3 startRotation;
    protected override void Awake()
    {
        inputManager = Utage.InputManager.Instance;
        base.Awake();
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (!Application.isPlaying)
            return;

        if (vcam.Follow)
        {
            if (stage == CinemachineCore.Stage.Aim)
            {
                if (startRotation == null)startRotation = transform.localRotation.eulerAngles;
                Vector2 deltaInput = inputManager.GetMouseDelta();
                startRotation.x = deltaInput.x * Time.deltaTime * vSpd;
                startRotation.y = deltaInput.y * Time.deltaTime * hSpd;
                startRotation.y = Mathf.Clamp(startRotation.y, -clampAngle, clampAngle);
                state.RawOrientation = Quaternion.Euler(startRotation.y, startRotation.x, 0);
            }
        }
    }
}
