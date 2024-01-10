using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script which contains the logic to control <see cref="Camera.main"/>.
/// In this game, it doesn't make sense to have a controllable camera when the is no <see cref="PlayerAgent"/> around.
/// The <see cref="PlayerAgent"/> controls the camera. However, to make things more pleasing to the eye,
/// the logic of the control of the camera is moved to its separate class, ie the <see cref="CameraManager"/>.
/// The controls are done via inputs, which are managed by Unity's <see cref="Input"/> system.
/// This class doesn't have its own Update method. It is updated via the <see cref="PlayerAgent"/>'s update method,
/// which invokes <see cref="UpdateCamera"/>.
/// The <see cref="PlayerAgent"/> also invokes <see cref="LateUpdateCamera"/> in its own LateUpdate.
/// The position and rotation of the camera must be handled in a LateUpdate call, to avoid "jittery" visuals
/// in the camera's movement.
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

    /// <summary>
    /// Main camera prefab, to be set in the Inspector menu.
    /// </summary>
    public Camera mainCameraPrefab;
    /// <summary>
    /// Third person view camera tracking point, to be set in the Inspector menu.
    /// </summary>
    public Transform thirdPersonViewTrackingPoint;
    /// <summary>
    /// First person view camera tracking point, to be set in the Inspector menu.
    /// </summary>
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
    /// Spawns the main camera if it is null, using <see cref="mainCameraPrefab"/>.
    /// </summary>
    void SpawnMainCamera()
    {
        if (Camera.main == null)
        {
            Instantiate(mainCameraPrefab);
        }
    }

    /// <summary>
    /// Initializes the camera for the <see cref="PlayerAgent"/> to control.
    /// </summary>
    /// <param name="playerAgent"></param>
    public void InitializeCamera(PlayerAgent playerAgent)
    {
        this.playerAgent = playerAgent;
        SpawnMainCamera();
        SetCameraTrackingPoint();
    }

    /// <summary>
    /// Read inputs from Unity's <see cref="Input"/> system.
    /// </summary>
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
    /// Sets the <see cref="chosenCameraTrackingPoint"/> depending on whether or not the camera is in first
    /// or third person mode. It also sets the visibility of the helmet.
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

    /// <summary>
    /// A method which is meant to be invoked in every Update call.
    /// Primarily used by <see cref="PlayerAgent.Update"/>.
    /// It reads inputs which will be used to control the camera.
    /// It is better to read inputs in Update, to avoid "input lag".
    /// It also handles the camera's view mode, and tracking point.
    /// </summary>
    public void UpdateCamera()
    {
        ReadInputs();

        HandleCameraViewMode();
        SetCameraTrackingPoint();
    }

    /// <summary>
    /// Handles the rotation of the camera based on mouse input.
    /// Primarily used by <see cref="LateUpdateCamera"/>.
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

        // First, reset the rotation.
        Camera.main.transform.rotation = Quaternion.identity;

        // Then, rotate the camera.
        Camera.main.transform.Rotate(Vector3.up, cameraYaw);
        Camera.main.transform.Rotate(Vector3.right, cameraPitch);
    }

    /// <summary>
    /// Handles the position of the camera based on whatever it is tracking.
    /// Primarily used by <see cref="LateUpdateCamera"/>.

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

    /// <summary>
    /// A method which is meant to be invoked in every Update call.
    /// Primarily used by <see cref="PlayerAgent.LateUpdate"/>.
    /// The camera's position and rotation must be set in a LateUpdte call (ie, after Update has been called).
    /// This is because we do not want to have "jittery" camera movement.
    /// Also, animations are done in Update. So, if the player chooses to use the first person view mode,
    /// then we want to set the camera's position in LateUpdate (ie, after animations have played out in Update).
    /// </summary>
    public void LateUpdateCamera()
    {
        // Rotate the camera first in order to avoid jittery camera.
        HandleCameraRotation();

        HandleCameraPosition();
    }
}
