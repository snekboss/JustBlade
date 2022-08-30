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
    const float ThirdPersonCameraOffsetYchangeSpeed = 3.0f;

    const float ThirdPersonCameraOffsetZchangeSpeed = 3.0f;
    const float ThirdPersonCameraOffsetZmin = 0.75f;
    const float ThirdPersonCameraOffsetZmax = 2.5f;
    bool IsCameraModeOrbital;

    public Camera mainCameraPrefab;

    Transform chosenCameraTrackingPoint;
    public Transform thirdPersonViewTrackingPoint;
    public Transform firstPersonViewTrackingPoint; 
    #endregion

    public Transform groundednessCheckerTransform; // used for checking if the player is grounded

    #region Player collision related fields
    CapsuleCollider movementCollider;
    Rigidbody movementRbody;

    /// <summary>
    /// Make the player agent an obstance, to make sure that AiAgents don't go through the player agent.
    /// This makes them try to avoid the player as they come closer, but it is not too conspicous,
    /// since they just stop when they come close enough.
    /// The alternative to this is to make the player agent a NavMeshAgent, but this snaps the player
    /// on to the NavMesh, which is not good at all.
    /// </summary>
    NavMeshObstacle nmo; 
    #endregion

    #region Foot movement fields
    // Foot movement fields
    float moveInputX;
    float moveInputY;
    Vector2 localMoveDirXZ;
    Vector2 worldVelocityXZ;

    float jumpPower = 4.0f;
    float jumpCooldownTimer;
    float jumpCooldownTimerMax = 1.0f;
    static readonly float GroundDistance = 0.3f;
    bool isGrounded;
    #endregion

    #region Agent rotation fields
    // Agent rotation fields
    float mouseX;
    float mouseY;

    float cameraYaw; // left right about Y axis
    const float CameraPitchThreshold = 89.0f;

    static readonly float TargetLookDirSlerpRate = 0.1f;
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

    public override void RequestEquipmentSet(out Weapon weaponPrefab
        , out Armor headArmorPrefab
        , out Armor torsoArmorPrefab
        , out Armor handArmorPrefab
        , out Armor legArmorPrefab)
    {
        weaponPrefab = PrefabManager.Weapons[TournamentVariables.PlayerChosenWeaponIndex];

        headArmorPrefab = PrefabManager.HeadArmors[TournamentVariables.PlayerChosenHeadArmorIndex];
        torsoArmorPrefab = PrefabManager.TorsoArmors[TournamentVariables.PlayerChosenTorsoArmorIndex];
        handArmorPrefab = PrefabManager.HandArmors[TournamentVariables.PlayerChosenHandArmorIndex];
        legArmorPrefab = PrefabManager.LegArmors[TournamentVariables.PlayerChosenLegArmorIndex];
    }

    /// <summary>
    /// Initializes the values of the movement collider of the player.
    /// It sets the height, radius, position, etc.
    /// It also initializes the <see cref="NavMeshObstacle"/>.
    /// <seealso cref="nmo"/>.
    /// </summary>
    void InitializeMovementCollider()
    {
        movementCollider = gameObject.AddComponent<CapsuleCollider>();
        movementCollider.height = AgentHeight;
        movementCollider.center = Vector3.up * AgentHeight / 2;
        movementCollider.radius = AgentRadius;

        nmo = gameObject.AddComponent<NavMeshObstacle>();
        nmo.height = movementCollider.height;
        nmo.center = movementCollider.center;
        nmo.radius = movementCollider.radius;
        nmo.carving = false;
    }

    /// <summary>
    /// Initializes the values of the movement rigidbody of the player.
    /// </summary>
    void InitializeMovementRigidbody()
    {
        movementRbody = gameObject.AddComponent<Rigidbody>();
        movementRbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        movementRbody.mass = AgentMass;
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
        movementRbody.velocity = Vector3.zero;
        movementRbody.angularVelocity = Vector3.zero;
        // playerMovementRigidbody.isKinematic = true;
        worldVelocityXZ = Vector2.zero;
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
        cameraYaw += StaticVariables.PlayerCameraRotationSpeed * mouseX * Time.deltaTime;

        // Subtracting, because negative angle about X axis means "up".
        LookAngleX -= StaticVariables.PlayerCameraRotationSpeed * mouseY * Time.deltaTime; 

        LookAngleX = Mathf.Clamp(LookAngleX, -CameraPitchThreshold, CameraPitchThreshold);

        // First, reset rotation.
        Camera.main.transform.rotation = Quaternion.identity;

        // Then, rotate with the new angles.
        Camera.main.transform.Rotate(Vector3.up, cameraYaw);
        Camera.main.transform.Rotate(Vector3.right, LookAngleX);
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
        // Movement related
        LayerMask walkableLayerMask = 1 << StaticVariables.Instance.DefaultLayer.value;
        isGrounded = Physics.CheckSphere(groundednessCheckerTransform.position, GroundDistance, walkableLayerMask, QueryTriggerInteraction.Ignore);

        if (isGrounded)
        {
            localMoveDirXZ = new Vector2(moveInputX, moveInputY);

            Vector3 localMoveDir3D = new Vector3(localMoveDirXZ.x, 0, localMoveDirXZ.y);

            Vector3 worldMoveDir3D = transform.TransformDirection(localMoveDir3D);

            Vector3 worldVelocity3D = worldMoveDir3D * MovementSpeedLimit;

            if (worldMoveDir3D.sqrMagnitude > 1)
            {
                // This is in order to avoid the movement speed increase caused by diagonal movement.
                worldVelocity3D /= worldMoveDir3D.magnitude;
            }

            worldVelocityXZ = new Vector2(worldVelocity3D.x, worldVelocity3D.z);

            currentMovementSpeed = worldVelocityXZ.magnitude;
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
            movementRbody.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
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
                StaticVariables.ThirdPersonCameraOffsetZcur -= Input.mouseScrollDelta.y * Time.deltaTime * ThirdPersonCameraOffsetZchangeSpeed;
                StaticVariables.ThirdPersonCameraOffsetZcur 
                    = Mathf.Clamp(StaticVariables.ThirdPersonCameraOffsetZcur, ThirdPersonCameraOffsetZmin, ThirdPersonCameraOffsetZmax);
            }
            else
            {
                StaticVariables.ThirdPersonCameraOffsetYcur += Input.mouseScrollDelta.y * Time.deltaTime * ThirdPersonCameraOffsetYchangeSpeed;
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
    /// It is also used to initialize the movement collider and rigidbody of the player.
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        isFriendOfPlayer = true;
        IsPlayerAgent = true;

        isDefTimer = 2 * isDefTimerThreshold; // set it far above the threshold, so that the condition is not satisfied at the start.

        InitializeMovementCollider();
        InitializeMovementRigidbody();
    }

    /// <summary>
    /// Unity's Start method.
    /// In this case, it is used to spawn the camera (if it is null), and set the <see cref="chosenCameraTrackingPoint"/>.
    /// </summary>
    void Start()
    {
        SpawnMainCamera();
        SetCameraTrackingPoint();
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

        HandleCameraViewMode();
        HandleCameraRotation();
        HandleAgentRotation();
        HandleFootMovement();

        HandleCombatInputs();
        HandleCombatDirection();

        AnimMgr.UpdateAnimations(localMoveDirXZ, currentMovementSpeed, isGrounded, isAtk, isDef);
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

    /// <summary>
    /// Unity's FixedUpdate method.
    /// In this case, since the player agent is moved via a <see cref="Rigidbody"/>, the movement has to be done in FixedUpdate.
    /// The movement is based on <see cref="worldVelocityXZ"/>, which is calculated during <see cref="Update"/>.
    /// </summary>
    void FixedUpdate()
    {
        if (StaticVariables.IsGamePaused)
        {
            return;
        }

        Vector3 worldVelocity3D = new Vector3(worldVelocityXZ.x, 0, worldVelocityXZ.y);
        movementRbody.MovePosition(movementRbody.position + worldVelocity3D * Time.fixedDeltaTime);
    }
}
