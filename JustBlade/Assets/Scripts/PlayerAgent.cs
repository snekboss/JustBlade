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
/// - <see cref="AgentAudioManager"/>
/// - <see cref="CharacteristicManager"/>
/// - <see cref="CameraManager"/>.
/// - <see cref="NavMeshAgent"/>. In particular, make sure this component is disabled
/// in the Inspector menu. Enable this component via code using <see cref="Agent.InitializePosition(Vector3)"/>.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerAgent : Agent
{
    CameraManager CamMgr
    {
        get
        {
            if (camMgr == null)
            {
                camMgr = GetComponent<CameraManager>();
            }

            return camMgr;
        }
    }
    CameraManager camMgr;

    #region Player movement related fields
    public Transform groundednessCheckerTransform; // used for checking if the player is grounded
    bool isGrounded; // value based on CharacterController.isGrounded AND Physics.CheckSphere.

    bool isFalling;
    float isFallingTimer;
    readonly float IsFallingThreshold = 0.375f; // player is considered to be falling beyond this time

    CharacterController charCont;
    readonly float AgentSkinWidthMultiplier = 0.1f;
    readonly float GroundedDistanceMultiplier = 2.0f;
    float AgentSkinWidth { get { return CharMgr.AgentWorldRadius * AgentSkinWidthMultiplier; } }
    float GroundedDistance { get { return AgentSkinWidth * GroundedDistanceMultiplier; } }
    #endregion

    #region Foot movement fields
    // Foot movement fields
    float moveInputXraw;
    float moveInputYraw;

    float moveInputXsmoothed;
    float moveInputYsmoothed;

    float moveInputSmoothDampVelocityX; // DO NOT MODIFY. This is passed as a ref argument to Unity's Mathf.SmoothDamp method.
    float moveInputSmoothDampVelocityY; // DO NOT MODIFY. This is passed as a ref argument to Unity's Mathf.SmoothDamp method.
    readonly float MoveInputSmoothTime = 0.1f;

    Vector2 localMoveDirXZ;
    Vector3 worldVelocity;

    float jumpPower = 4.0f;
    float jumpCooldownTimer;
    float jumpCooldownTimerMax = 1.0f;
    #endregion

    #region Agent rotation fields
    // Agent rotation fields
    Vector3 targetLookDir;
    readonly float TargetLookDirSlerpRate = 0.1f;
    #endregion

    #region Combat inputs
    // Combat inputs
    // Mouse inputs to choose combat direction
    float mouseXraw;
    float mouseYraw;
    // Button inputs
    bool btnAtkPressed;
    bool btnAtkHeld;
    bool btnDefPressed;
    bool btnDefHeld;
    bool btnDefReleased;
    bool btnJumpPressed;
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

        CamMgr.InitializeCamera(this);

        InitializeCharacterController();
        //InitializeNavMeshAgent();
    }

    /// <summary>
    /// TODO: Explain this nonsense. All this to supress some warnings in a single frame...
    /// </summary>
    /// <param name="worldPos"></param>
    public override void InitializePosition(Vector3 worldPos)
    {
        // To avoid Unity's complaining about the NavMeshAgent not being close
        // enough to a NavMesh, we first set our transform position, and then enable
        // the NavMeshAgent component (by calling the below method).
        transform.position = worldPos;
        InitializeNavMeshAgent();
    }

    public override bool IsGrounded() { return isGrounded; }

    public override bool IsFalling() { return isFalling; }

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
        nma = gameObject.GetComponent<NavMeshAgent>();
        nma.height = CharacteristicManager.DefaultAgentHeight;
        nma.radius = CharacteristicManager.DefaultAgentRadius;
        nma.updatePosition = false;
        nma.updateRotation = false;
        nma.nextPosition = transform.position;
        nma.avoidancePriority = 0;

        // Make sure that the NavMeshComponent is disabled in the Inspector menu.
        nma.enabled = true; 
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
        mouseXraw = 0;
        mouseYraw = 0;
    }

    /// <summary>
    /// Reads the mouse and keyboard inputs which govern the player's movement, rotation and combat.
    /// </summary>
    void ReadInputs()
    {
        // Foot movement
        // Get raw movement inputs.
        moveInputXraw = Input.GetAxis("Horizontal");
        moveInputYraw = Input.GetAxis("Vertical");

        // Calculate smoothed move inputs to avoid sharp changes in the movement (and also in the animations).
        moveInputXsmoothed = Mathf.SmoothDamp(moveInputXsmoothed, moveInputXraw, ref moveInputSmoothDampVelocityX, MoveInputSmoothTime);
        moveInputYsmoothed = Mathf.SmoothDamp(moveInputYsmoothed, moveInputYraw, ref moveInputSmoothDampVelocityY, MoveInputSmoothTime);

        // Use raw mouse inputs to choose combat direction.
        mouseXraw = Input.GetAxis("Mouse X");
        mouseYraw = Input.GetAxis("Mouse Y");

        // Button inputs
        btnAtkPressed = Input.GetMouseButtonDown(0);
        btnAtkHeld = Input.GetMouseButton(0);

        btnDefPressed = Input.GetMouseButtonDown(1);
        btnDefHeld = Input.GetMouseButton(1);
        btnDefReleased = Input.GetMouseButtonUp(1);

        btnJumpPressed = Input.GetKeyDown(KeyCode.Space);

        btnQpressed = Input.GetKeyDown(KeyCode.Q);
    }

    void SetLookAngleX()
    {
        LookAngleX = Camera.main.transform.rotation.eulerAngles.x;
        // Adjust the angle to the range [-180, 180]
        if (LookAngleX > 180)
        {
            LookAngleX -= 360;
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
    /// Performs a two step verification as to whether the player is grounded or not.
    /// The reason is because, when the game is unpaused by setting Time.timeScale to 0,
    /// the isGrounded property of the CharacterController becomes unreliable for a short time.
    /// This method uses Physics.CheckSphere to perform a second check.
    /// </summary>
    void HandleGroundednessCheck()
    {
        LayerMask walkableLayerMask = 1 << StaticVariables.DefaultLayer.value;
        bool isGroundedPhysics =
            Physics.CheckSphere(groundednessCheckerTransform.position, GroundedDistance, walkableLayerMask, QueryTriggerInteraction.Ignore);

        // We perform a two step verification as to whether the player is grounded.
        // This is because, when the game is unpaused by setting Time.timeScale to 0,
        // the isGrounded property of the CharacterController becomes unreliable for a short time.
        isGrounded = charCont.isGrounded || isGroundedPhysics;
    }

    /// <summary>
    /// TODO: Write summary.
    /// It's better to invoke this after calling <see cref="HandleGroundednessCheck"/>,
    /// so that we have groundedness information.
    /// </summary>
    void HandleIsFalling()
    {
        if (isGrounded == false)
        {
            isFallingTimer += Time.deltaTime;
            if (isFallingTimer >= IsFallingThreshold)
            {
                isFalling = true;
            }
        }
        else
        {
            isFallingTimer = 0f;
            isFalling = false;
        }
    }

    /// <summary>
    /// Handles the rotation of the player agent based on camera's forward.
    /// See <see cref="HandleCameraRotation"/> for handling of the camera's rotation.
    /// </summary>
    void HandleAgentRotation()
    {
        if (CamMgr.IsCameraModeOrbital == false)
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

        if (isFalling == false)
        {
            // This part of the code is about handling non-vertical movement (ie, X and Z axes).
            // This part of the code will run as long as isFalling is false, which is based on isFallingTimer.
            // This means that this code will run even if isGrounded is false, as they're separate things.

            // localMoveDirXZ is for animation (moveX, moveY); as well as for initializing non-vertical movement.
            localMoveDirXZ = new Vector2(moveInputXsmoothed, moveInputYsmoothed);

            // Notice that the y value of this vector is 0.
            // This is because we don't want the y value to take any part in the calculation of the movement speed.
            Vector3 localMoveDir3D = new Vector3(localMoveDirXZ.x, 0, localMoveDirXZ.y);

            // Transform the moveDir from local space to world space.
            Vector3 worldMoveDir3D = transform.TransformDirection(localMoveDir3D);

            // Calculate world velocity (using only the horizontal directions).
            worldVelocity = worldMoveDir3D * CharMgr.MovementSpeedLimit;

            // If moving backwards, apply speed penalty multiplier.
            // This penalty is done only to the player (because player is intelligent).
            // The AiAgents' speeds are governed by Unity's NavMeshAgent (also, they're stupid).
            if (IsMovingBackwards(localMoveDir3D) && CharMgr.IsOverEncumbered == false)
            {
                worldVelocity *= CharacteristicManager.PlayerMovingBackwardsSpeedPenaltyMultiplier;
            }

            if (worldMoveDir3D.sqrMagnitude > 1)
            {
                // This is in order to avoid the movement speed increase caused by diagonal movement.
                worldVelocity /= worldMoveDir3D.magnitude;
            }

            // Now, update the value of currentMovementSpeed by taking ONLY the horizontal directions into account.
            Vector2 worldVelocityXZ = new Vector2(worldVelocity.x, worldVelocity.z);

            // Update its value only as long as the player is not falling (ie, isFalling == false).
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
        // Jumping is based on "isFalling" rather than "isGrounded".
        bool canJump = true;

        if (isFalling)
        {
            jumpCooldownTimer = 0;
        }

        if (jumpCooldownTimer < jumpCooldownTimerMax)
        {
            canJump = false;
            jumpCooldownTimer += Time.deltaTime;
        }

        if (btnJumpPressed && (isFalling == false) && canJump)
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
        
        // Continue defending for a short time even after the defend button is not held anymore.
        if (isDef)
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

        if (Mathf.Abs(mouseXraw) > Mathf.Abs(mouseYraw))
        {
            // it has to be left or right
            if (mouseXraw > 0)
            {
                combatDir = CombatDirection.Right;
            }
            if (mouseXraw < 0)
            {
                combatDir = CombatDirection.Left;
            }
        }
        else if (Mathf.Abs(mouseXraw) < Mathf.Abs(mouseYraw))
        {
            // it has to be up or down
            if (mouseYraw > 0)
            {
                combatDir = CombatDirection.Up;
            }
            if (mouseYraw < 0)
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
        CamMgr.UpdateCamera();
        SetLookAngleX();

        HandleOrders();
        HandleAgentRotation();
        
        HandleGroundednessCheck();
        HandleIsFalling();
        HandleFootMovement();

        HandleCombatInputs();
        HandleCombatDirection();

        AnimMgr.UpdateAnimations(localMoveDirXZ, CharMgr.CurrentMovementSpeed, IsFalling(), isAtk, isDef);
        AudioMgr.UpdateAudioManager();
    }

    /// <summary>
    /// TODO: Explain that the rotation is also done in LateUpdate, to avoid jittery camera.
    /// Unity's LateUpdate method.
    /// It is also an override of <see cref="Agent.LateUpdate"/>.
    /// On top of what <see cref="Agent.LateUpdate"/> does, it also sets the position of the camera.
    /// See <see cref="HandleCameraPosition"/> for more information.
    /// </summary>
    protected override void LateUpdate()
    {
        base.LateUpdate(); // Let the spine be rotated.

        // Update the camera after the spine has been rotated.
        CamMgr.LateUpdateCamera();
    }
}
