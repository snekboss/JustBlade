using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A class which designates the attached game object as a Player Agent.
/// There is only meant to be one Player Agent at any given moment, since they also control the main camera.
/// The Player Agent is controlled by the user (player).
/// A PlayerAgent also requires:
/// - <see cref="AnimationManager"/>.
/// - <see cref="EquipmentManager"/>.
/// - <see cref="LimbManager"/>.
/// </summary>
public class PlayerAgent : Agent
{
    #region Camera related fields
    // The camera is governed by the player agent, because the position has to be set AFTER player's LateUpate is run.
    // The only way to ensure that is by letting the player govern camera completely.
    // The other option is to write a custom system which updates all game objects on the scene *manually*.
    const float ThirdPersonCameraOffsetYmin = -1.0f;
    const float ThirdPersonCameraOffsetYmax = 0.5f;
    const float ThirdPersonCameraOffsetYchangeSpeed = 0.1f;

    const float ThirdPersonCameraOffsetZchangeSpeed = 0.1f;
    const float ThirdPersonCameraOffsetZmin = 0.75f;
    const float ThirdPersonCameraOffsetZmax = 2.5f;
    bool IsCameraModeOrbital;

    public Camera mainCameraPrefab;

    Transform chosenCameraTrackingPoint;
    public Transform thirdPersonViewTrackingPoint;
    public Transform firstPersonViewTrackingPoint;
    #endregion

    #region Player movement related fields
    public Transform groundednessCheckerTransform; // used for checking if the player is grounded
    bool isGrounded; // value based on CharacterController.isGrounded AND Physics.CheckSphere.

    CharacterController charCont;
    const float AgentSkinWidthMultiplier = 0.1f;
    const float GroundedDistanceMultiplier = 2.0f;
    float AgentSkinWidth { get { return CharMgr.AgentWorldRadius * AgentSkinWidthMultiplier; } }
    float GroundedDistance { get { return AgentSkinWidth * GroundedDistanceMultiplier; } }

    /// <summary>
    /// Make the player agent a NavMeshAgent, to make sure that AiAgents don't go through the player agent.
    /// </summary>
    NavMeshAgent nma;
    #endregion

    #region Foot movement fields
    // Foot movement fields
    float moveInputX;
    float moveInputY;
    Vector2 localMoveDirXZ;
    Vector3 worldVelocity;

    float jumpPower = 4.0f;
    float jumpCooldownTimer;
    float jumpCooldownTimerMax = 1.0f;
    #endregion

    #region Agent rotation fields
    // Agent rotation fields
    float mouseX;
    float mouseY;

    float cameraYaw; // left right about Y axis
    const float CameraPitchThreshold = 89.0f;

    const float TargetLookDirSlerpRate = 0.1f;
    Vector3 targetLookDir;
    #endregion

    #region Combat inputs
    // Combat inputs
    bool btnAtkPressed;
    bool btnAtkHeld;
    bool btnDefPressed;
    bool btnDefHeld;
    bool btnDefReleased;
    bool btnJumpPressed;
    bool btnShiftHeld; // toggle editing camera offset Y or Z
    bool btnRpressed; // toggle first/third person view
    bool btnTpressed; // toggle orbital camera 
    bool btnQpressed; // toggle AI command
    #endregion

    #region Fields that are determined by Combat inputs.
    // Below are not inputs, but they depend on inputs.
    bool isAtk;
    bool isDef;
    float isDefTimer;
    float isDefTimerThreshold = 0.5f;
    CombatDirection lastCombatDir;
    CombatDirection combatDir;
    #endregion


    public bool IsPlayerOrderingToHoldPosition { get; protected set; } = false;

    public delegate void PlayerOrderToggleEvent(PlayerAgent playerAgent, bool isPlayerOrderingToHoldPosition);
    public event PlayerOrderToggleEvent PlayerOrderToggle;

    public override void InitializeAgent(Weapon weaponPrefab
        , Armor headArmorPrefab
        , Armor torsoArmorPrefab
        , Armor handArmorPrefab
        , Armor legArmorPrefab
        , CharacteristicSet characteristicPrefab = null)
    {
        base.InitializeAgent(weaponPrefab
            , headArmorPrefab
            , torsoArmorPrefab
            , handArmorPrefab
            , legArmorPrefab
            , characteristicPrefab);

        InitializeCharacterController();
        SetCameraTrackingPoint();
    }

    /// <summary>
    /// Initializes the values of the character controller of the player.
    /// It sets the height, radius, position, etc.
    /// </summary>
    void InitializeCharacterController()
    {
        if (charCont == null)
        {
            charCont = gameObject.AddComponent<CharacterController>();
        }
        
        charCont.height = CharacteristicManager.DefaultAgentHeight;
        charCont.center = Vector3.up * CharacteristicManager.DefaultAgentHeight / 2;
        charCont.radius = CharacteristicManager.DefaultAgentRadius;
        charCont.minMoveDistance = 0;
        // From Unity Docs:
        // It's good practice to keep your Skin Width at least greater than 0.01 and more than 10% of the Radius.
        charCont.skinWidth = CharacteristicManager.DefaultAgentRadius * AgentSkinWidthMultiplier;
    }

    /// <summary>
    /// Initializes the <see cref="NavMeshAgent"/>, so that the other <see cref="NavMeshAgent"/>s can avoid the player.
    /// </summary>
    void InitializeNavMeshAgent()
    {
        nma = gameObject.AddComponent<NavMeshAgent>();
        nma.height = CharacteristicManager.DefaultAgentHeight;
        nma.radius = CharacteristicManager.DefaultAgentRadius;
        nma.updatePosition = false;
        nma.updateRotation = false;
        nma.nextPosition = transform.position;
        nma.avoidancePriority = 0;
    }

    /// <summary>
    /// Handles the death of the player agent.
    /// It sets values like:
    /// - mouseX, mouseY to zero.
    /// - Movement velocity related vectors to the zero vector.
    /// This is done so that the player doesn't glide around after death.
    /// </summary>
    void HandleDeath()
    {
        worldVelocity = Vector3.zero;
        localMoveDirXZ = Vector2.zero;
        charCont.SimpleMove(worldVelocity);
        mouseX = 0;
        mouseY = 0;
    }

    /// <summary>
    /// Reads the mouse and keyboard inputs which govern the player's movement, rotation and combat.
    /// </summary>
    void ReadInputs()
    {
        // Foot movement
        moveInputX = Input.GetAxis("Horizontal");
        moveInputY = Input.GetAxis("Vertical");

        // Camera rotation
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        // Button inputs
        btnAtkPressed = Input.GetMouseButtonDown(0);
        btnAtkHeld = Input.GetMouseButton(0);

        btnDefPressed = Input.GetMouseButtonDown(1);
        btnDefHeld = Input.GetMouseButton(1);
        btnDefReleased = Input.GetMouseButtonUp(1);

        btnJumpPressed = Input.GetKeyDown(KeyCode.Space);
        btnShiftHeld = Input.GetKey(KeyCode.LeftShift);

        btnRpressed = Input.GetKeyDown(KeyCode.R);
        btnTpressed = Input.GetKeyDown(KeyCode.T);
        btnQpressed = Input.GetKeyDown(KeyCode.Q);
    }

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

    /// <summary>
    /// Sets the <see cref="chosenCameraTrackingPoint"/> depending on whether or not the camera is in first or third person mode.
    /// It also sets the visibility of the helmet.
    /// </summary>
    void SetCameraTrackingPoint()
    {
        if (StaticVariables.IsCameraModeFirstPerson)
        {
            chosenCameraTrackingPoint = firstPersonViewTrackingPoint;
            EqMgr.ToggleHelmetVisibility(false);
        }
        else
        {
            chosenCameraTrackingPoint = thirdPersonViewTrackingPoint;
            EqMgr.ToggleHelmetVisibility(true);
        }
    }

    void HandleOrders()
    {
        if (btnQpressed)
        {
            IsPlayerOrderingToHoldPosition = !IsPlayerOrderingToHoldPosition;

            if (PlayerOrderToggle != null)
            {
                PlayerOrderToggle(this, IsPlayerOrderingToHoldPosition);
            }
        }
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
    /// Handles the rotation of the camera based on mouse input.
    /// </summary>
    void HandleCameraRotation()
    {
        cameraYaw += StaticVariables.PlayerCameraRotationSpeed * mouseX;

        // Subtracting, because negative angle about X axis means "up".
        LookAngleX -= StaticVariables.PlayerCameraRotationSpeed * mouseY;

        LookAngleX = Mathf.Clamp(LookAngleX, -CameraPitchThreshold, CameraPitchThreshold);

        // First, reset rotation.
        Camera.main.transform.rotation = Quaternion.identity;

        // Then, rotate with the new angles.
        Camera.main.transform.Rotate(Vector3.up, cameraYaw);
        Camera.main.transform.Rotate(Vector3.right, LookAngleX);
    }

    /// <summary>
    /// Performs a two step verification as to whether the player is grounded or not.
    /// The reason is because, when the game is unpaused by setting Time.timeScale to 0,
    /// the isGrounded property of the CharacterController becomes unreliable for a short time.
    /// This method uses Physics.CheckSphere to perform a second check.
    /// </summary>
    void HandleGroundednessCheck()
    {
        LayerMask walkableLayerMask = 1 << StaticVariables.Instance.DefaultLayer.value;
        bool isGroundedPhysics =
            Physics.CheckSphere(groundednessCheckerTransform.position, GroundedDistance, walkableLayerMask, QueryTriggerInteraction.Ignore);

        // We perform a two step verification as to whether the player is grounded.
        // This is because, when the game is unpaused by setting Time.timeScale to 0,
        // the isGrounded property of the CharacterController becomes unreliable for a short time.
        isGrounded = charCont.isGrounded || isGroundedPhysics;
    }

    /// <summary>
    /// Handles the rotation of the player agent based on the camera's rotation.
    /// <seealso cref="HandleCameraRotation"/>.
    /// </summary>
    void HandleAgentRotation()
    {
        if (IsCameraModeOrbital == false)
        {
            // Only keep track of camera's forward when we're NOT in orbital mode.
            targetLookDir = Camera.main.transform.forward;
            targetLookDir.y = 0;
        }

        Quaternion lookRot = Quaternion.LookRotation(targetLookDir);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, TargetLookDirSlerpRate);
    }

    /// <summary>
    /// Handles the player agent's foot movement based on keyboard input.
    /// This method also manages the jumping of the player.
    /// This method is called in Update, despite the fact that jumping is also done in this method.
    /// This is because jumping is done instantly, so I don't think it matters whether it is done in FixedUpdate or not.
    /// </summary>
    void HandleFootMovement()
    {
        // Save the velocity in y axis, in case the player is currently falling/jumping.
        // This is because the "isGrounded" code below might edit the y value.
        float existingY = worldVelocity.y;

        if (isGrounded)
        {
            // This part of the code is about handling non-vertical movement (ie, X and Z axes).

            // localMoveDirXZ is for animation (moveX, moveY); as well as for initializing non-vertical movement.
            localMoveDirXZ = new Vector2(moveInputX, moveInputY);

            // Notice that the y value of this vector is 0.
            // This is because we don't want the y value to take any part in the calculation of the movement speed.
            Vector3 localMoveDir3D = new Vector3(localMoveDirXZ.x, 0, localMoveDirXZ.y);

            // Transform the moveDir from local space to world space.
            Vector3 worldMoveDir3D = transform.TransformDirection(localMoveDir3D);

            // Calculate world velocity (using only the horizontal directions).
            worldVelocity = worldMoveDir3D * CharMgr.MovementSpeedLimit;

            if (worldMoveDir3D.sqrMagnitude > 1)
            {
                // This is in order to avoid the movement speed increase caused by diagonal movement.
                worldVelocity /= worldMoveDir3D.magnitude;
            }

            // Now, update the value of currentMovementSpeed by taking ONLY the horizontal directions into account.
            Vector2 worldVelocityXZ = new Vector2(worldVelocity.x, worldVelocity.z);

            // Update its value only as long as the player is grounded.
            CharMgr.CurrentMovementSpeed = worldVelocityXZ.magnitude;
        }

        // Restore worldVelocity's vertical component, in case it was changed above.
        worldVelocity.y = existingY;

        if (isGrounded && worldVelocity.y < 0)
        {
            // This is to avoid having very negative y values due to gravity.
            worldVelocity.y = 0;
        }

        // Jump related
        bool canJump = true;

        if (!isGrounded)
        {
            jumpCooldownTimer = 0;
        }

        if (jumpCooldownTimer < jumpCooldownTimerMax)
        {
            canJump = false;
            jumpCooldownTimer += Time.deltaTime;
        }

        if (btnJumpPressed && isGrounded && canJump)
        {
            jumpCooldownTimer = 0;
            AnimMgr.SetJump(true);

            worldVelocity += Vector3.up * jumpPower;
        }

        // Apply gravity, and then finally use Move with worldVelocity.
        // NOTE: I know I'm already multiplying the worldVelocity by Time.deltaTime below.
        // However, if I don't also do it onto Physics.gravity, then gravity becomes ridiculously large.
        // This worked, so I'm keeping it...
        worldVelocity += Physics.gravity * Time.deltaTime;

        charCont.Move(worldVelocity * Time.deltaTime);

        // Update NavMeshAgent's position, so that the other AiAgents know where the player is.
        if (nma != null && nma.isActiveAndEnabled)
        {
            nma.nextPosition = transform.position;
        }
    }

    /// <summary>
    /// Sets the combat related fields such as <see cref="isAtk"/>, <see cref="isDef"/> etc. based on 
    /// the inputs which were read in <see cref="ReadInputs"/>.
    /// </summary>
    void HandleCombatInputs()
    {
        // Handle attacking
        isAtk = btnAtkHeld;

        // Handle Blocking
        isDef = btnDefHeld;

        // Continue defending for a short time even after releasing the defend button.
        if (btnDefReleased)
        {
            isDefTimer = 0;
        }

        // Cancel the the above if btnAtk was pressed.
        if (btnAtkPressed)
        {
            isDefTimer = isDefTimerThreshold;
        }

        if (isDefTimer < isDefTimerThreshold && !isAtk)
        {
            isDef = true;
            isDefTimer += Time.deltaTime;
        }
    }

    /// <summary>
    /// Handles the choosing of a <see cref="Agent.CombatDirection"/> based on the mouse movement of the player.
    /// The chosen combat direction is also reported to the <see cref="AnimationManager"/> by this method.
    /// </summary>
    void HandleCombatDirection()
    {
        // Assume combatDir hasn't changed.
        combatDir = lastCombatDir;

        if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
        {
            // it has to be left or right
            if (mouseX > 0)
            {
                combatDir = CombatDirection.Right;
            }
            if (mouseX < 0)
            {
                combatDir = CombatDirection.Left;
            }
        }
        else if (Mathf.Abs(mouseX) < Mathf.Abs(mouseY))
        {
            // it has to be up or down
            if (mouseY > 0)
            {
                combatDir = CombatDirection.Up;
            }
            if (mouseY < 0)
            {
                combatDir = CombatDirection.Down;
            }
        }

        bool wantToDefend = btnDefPressed; // def has higher precedence than atk, hence fewer conditions.
        bool wantToAttack = btnAtkPressed && !isDef && !btnDefPressed && !btnDefHeld;

        if (wantToDefend || wantToAttack)
        {
            AnimMgr.UpdateCombatDirection(combatDir);
        }

        lastCombatDir = combatDir;
    }

    /// <summary>
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

    /// <summary>
    /// Unity's Awake method.
    /// In this case, it is used to initialize a few fields about the player.
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        IsFriendOfPlayer = true;
        IsPlayerAgent = true;

        isDefTimer = 2 * isDefTimerThreshold; // set it far above the threshold, so that the condition is not satisfied at the start.
    }

    /// <summary>
    /// Unity's Start method.
    /// In this case, it is used to spawn the camera (if it is null), and set the <see cref="chosenCameraTrackingPoint"/>.
    /// It is also used to initialize the movement related components of the player.
    /// </summary>
    void Start()
    {
        SpawnMainCamera();
        SetCameraTrackingPoint();

        InitializeCharacterController();
        InitializeNavMeshAgent();
    }

    /// <summary>
    /// Unity's Update method.
    /// This is where most of the other methods are invoked every frame.
    /// It is also the place where <see cref="AnimationManager.UpdateAnimations(Vector2, float, bool, bool, bool)"/> is invoked.
    /// </summary>
    void Update()
    {
        if (StaticVariables.IsGamePaused)
        {
            return;
        }

        if (IsDead)
        {
            HandleDeath();
            return;
        }

        ReadInputs();

        HandleOrders();
        HandleCameraViewMode();
        HandleCameraRotation();
        HandleAgentRotation();
        HandleGroundednessCheck();
        HandleFootMovement();

        HandleCombatInputs();
        HandleCombatDirection();

        AnimMgr.UpdateAnimations(localMoveDirXZ, CharMgr.CurrentMovementSpeed, isGrounded, isAtk, isDef);
    }

    /// <summary>
    /// Unity's LateUpdate method.
    /// It is also an override of <see cref="Agent.LateUpdate"/>.
    /// On top of what <see cref="Agent.LateUpdate"/> does, it also sets the position of the camera.
    /// See <see cref="HandleCameraPosition"/> for more information.
    /// </summary>
    protected override void LateUpdate()
    {
        base.LateUpdate(); // let the spine be rotated

        // Move the camera to the position after the spine has been rotated.
        HandleCameraPosition();
    }
}
