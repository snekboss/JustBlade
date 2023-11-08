using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO: The camera is still controlled by <see cref="PlayerAgent"/>, because currently, it doesn't
/// make sense to move the camera while there is no <see cref="PlayerAgent"/> around.
/// </summary>
public class CameraManager : MonoBehaviour
{
    readonly float ThirdPersonCameraOffsetYmin = -1.0f;
    readonly float ThirdPersonCameraOffsetYmax = 0.5f;
    readonly float ThirdPersonCameraOffsetYchangeSpeed = 0.1f;

    readonly float ThirdPersonCameraOffsetZchangeSpeed = 0.1f;
    readonly float ThirdPersonCameraOffsetZmin = 0.7f;
    readonly float ThirdPersonCameraOffsetZmax = 3.5f; // used to be 2.5f
    readonly float CameraSmoothRotateLerpRate = 0.8f;

    // Prefabs to be set in the inspector.
    public Camera mainCameraPrefab;
    public Transform thirdPersonViewTrackingPoint;
    public Transform firstPersonViewTrackingPoint;
    Transform chosenCameraTrackingPoint;
    /// <summary>
    /// If true, the <see cref="PlayerAgent"/> will not look at where the camera is pointing at.
    /// </summary>
    public bool IsCameraModeOrbital { get; private set; }

    // Camera inputs.
    // Below are the smoothed versions of the raw mouse inputs, done by Mathf.SmoothDamp.
    float mouseXsmoothed;
    float mouseYsmoothed;
    readonly float MouseInputSmoothTime = 0.05f;

    float mouseSmoothDampVelocityX; // DO NOT MODIFY. This is passed as a ref argument to Unity's Mathf.SmoothDamp method.
    float mouseSmoothDampVelocityY; // DO NOT MODIFY. This is passed as a ref argument to Unity's Mathf.SmoothDamp method.

    float cameraYaw; // left/right about the Y axis
    float cameraPitch; // up/down about the X axis
    readonly float CameraPitchThreshold = 89.0f;

    bool btnShiftHeld; // toggle editing camera offset Y or Z
    bool btnRpressed; // toggle first/third person view
    bool btnTpressed; // toggle orbital camera 

    PlayerAgent playerAgent;

    /// <summary>
    /// Spawns the main camera, if it is null.
    /// </summary>
    void SpawnMainCamera()
    {
        if (Camera.main == null)
        {
            Instantiate(mainCameraPrefab);
        }
    }

    public void InitializeCamera(PlayerAgent playerAgent)
    {
        this.playerAgent = playerAgent;
        SpawnMainCamera();
        SetCameraTrackingPoint();
    }

    void ReadInputs()
    {
        // Camera rotation
        // Get raw mouse inputs
        float mouseXraw = Input.GetAxis("Mouse X");
        float mouseYraw = Input.GetAxis("Mouse Y");

        // Calculate smoothed mouse inputs to avoid camera jitter when "moving + rotating camera".
        mouseXsmoothed = Mathf.SmoothDamp(mouseXsmoothed, mouseXraw, ref mouseSmoothDampVelocityX, MouseInputSmoothTime);
        mouseYsmoothed = Mathf.SmoothDamp(mouseYsmoothed, mouseYraw, ref mouseSmoothDampVelocityY, MouseInputSmoothTime);

        btnShiftHeld = Input.GetKey(KeyCode.LeftShift);

        btnRpressed = Input.GetKeyDown(KeyCode.R);
        btnTpressed = Input.GetKeyDown(KeyCode.T);
    }
    /// <summary>
    /// Manages the switching between first person and third person views by calling <see cref="SetCameraTrackingPoint"/>.
    /// It also toggles between orbital camera mode based on <see cref="IsCameraModeOrbital"/>.
    /// </summary>
    void HandleCameraViewMode()
    {
        if (btnRpressed)
        {
            StaticVariables.IsCameraModeFirstPerson = !StaticVariables.IsCameraModeFirstPerson;

            SetCameraTrackingPoint();
        }

        if (btnTpressed)
        {
            IsCameraModeOrbital = !IsCameraModeOrbital;
        }
    }

    /// <summary>
    /// Sets the <see cref="chosenCameraTrackingPoint"/> depending on whether or not the camera is in first or third person mode.
    /// It also sets the visibility of the helmet.
    /// </summary>
    void SetCameraTrackingPoint()
    {
        if (StaticVariables.IsCameraModeFirstPerson)
        {
            chosenCameraTrackingPoint = firstPersonViewTrackingPoint;
            playerAgent.EqMgr.ToggleHelmetVisibility(false);
        }
        else
        {
            chosenCameraTrackingPoint = thirdPersonViewTrackingPoint;
            playerAgent.EqMgr.ToggleHelmetVisibility(true);
        }
    }

    public void UpdateCamera()
    {
        ReadInputs();

        HandleCameraViewMode();
        SetCameraTrackingPoint();
    }

    /// <summary>
    /// TODO: Explain that this should be invoked from LateUpdate.
    /// Handles the rotation of the camera based on mouse input.
    /// </summary>
    void HandleCameraRotation()
    {
        if (StaticVariables.IsGamePaused || playerAgent.IsDead)
        {
            // PlayerAgent.Update method is paused when the game is paused.
            // However, since this method is now invoked from PlayerAgent.LateUpdate,
            // we must also put this if check here.
            // Otherwise, the camera continues to rotate even while the game is paused.
            // Same thing when the player is dead.
            return;
        }

        cameraYaw += StaticVariables.PlayerCameraRotationSpeed * mouseXsmoothed;

        // We subtract here, so that the camera moves up when the mouse moves up.
        cameraPitch -= StaticVariables.PlayerCameraRotationSpeed * mouseYsmoothed;

        cameraPitch = Mathf.Clamp(cameraPitch, -CameraPitchThreshold, CameraPitchThreshold);

        // I used to have a camera jitter problem when "moving + rotating camera".

        // Below code was suggested by Microsoft's Bing Ai chat bot, based on my instructions regarding my camera jitter problem.
        // Create a target forward vector
        Vector3 targetForward = Quaternion.Euler(cameraPitch, cameraYaw, 0) * Vector3.forward;

        // Smoothly interpolate the camera's forward vector towards the target
        Camera.main.transform.forward = Vector3.Lerp(Camera.main.transform.forward, targetForward, CameraSmoothRotateLerpRate);

        // Update the camera's rotation
        Camera.main.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        // Above code was suggested by Microsoft's Bing Ai chat bot, based on my instructions regarding my camera jitter problem.
    }

    /// <summary>
    /// TODO: Explain also, that in general, it's PROBABLY better to do this in late update anyway. Dunno though. Think later. I'm hungry.
    /// Handles the position of the camera.
    /// This method is best called from <see cref="LateUpdate"/> method.
    /// This is because, if the camera is in first person view mode, then we want the spine bone to be rotated
    /// before we place the camera in the agent's eye.
    /// </summary>
    void HandleCameraPosition()
    {
        // Assume the camera is in first person view mode.
        Vector3 offset = Vector3.zero;

        if (StaticVariables.IsCameraModeFirstPerson == false)
        {
            // The camera is actually in third person view mode, so apply the zoom effects.

            if (btnShiftHeld == false)
            {
                StaticVariables.ThirdPersonCameraOffsetZcur -= Input.mouseScrollDelta.y * ThirdPersonCameraOffsetZchangeSpeed;
                StaticVariables.ThirdPersonCameraOffsetZcur
                    = Mathf.Clamp(StaticVariables.ThirdPersonCameraOffsetZcur, ThirdPersonCameraOffsetZmin, ThirdPersonCameraOffsetZmax);
            }
            else
            {
                StaticVariables.ThirdPersonCameraOffsetYcur += Input.mouseScrollDelta.y * ThirdPersonCameraOffsetYchangeSpeed;
                StaticVariables.ThirdPersonCameraOffsetYcur
                    = Mathf.Clamp(StaticVariables.ThirdPersonCameraOffsetYcur, ThirdPersonCameraOffsetYmin, ThirdPersonCameraOffsetYmax);
            }

            Vector3 offsetZ = Camera.main.transform.forward * (-StaticVariables.ThirdPersonCameraOffsetZcur);
            Vector3 offsetY = Vector3.up * StaticVariables.ThirdPersonCameraOffsetYcur;
            offset = offsetZ + offsetY;
        }

        Vector3 destination = chosenCameraTrackingPoint.position + offset;
        Camera.main.transform.position = destination;
    }

    public void LateUpdateCamera()
    {
        // Rotate the camera first in order to avoid jittery camera.
        HandleCameraRotation();

        HandleCameraPosition();
    }
}
