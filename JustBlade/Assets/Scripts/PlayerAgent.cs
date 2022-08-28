using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAgent : Agent
{
    // TODO: Remove[SerializeField]. It is for debugging purposes only.
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
    public Transform groundednessCheckerTransform; // used for checking if the player is grounded

    CapsuleCollider playerMovementCollider;
    Rigidbody playerMovementRigidbody;

    #region Foot movement fields
    // Foot movement fields
    float moveInputX;
    float moveInputY;
    Vector2 localMoveDirXZ;
    Vector2 worldVelocityXZ;

    float jumpPower = 4.0f;
    float jumpCooldownTimer;
    float jumpCooldownTimerMax = 1.0f;
    public float groundDistance = 0.3f;
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

    NavMeshObstacle nmo;

    CombatDirection lastCombatDir;
    CombatDirection combatDir;

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

    // Below are not inputs, but they depend on inputs.
    bool isAtk;
    bool isDef;
    float isDefTimer;
    float isDefTimerThreshold = 0.1f;

    public override void Awake()
    {
        base.Awake();
        isFriendOfPlayer = true;
        IsPlayerAgent = true;

        isDefTimer = 2 * isDefTimerThreshold; // set it far above the threshold, so that the condition is not satisfied at the start.

        InitializeMovementCollider();
        InitializeMovementRigidbody();
    }

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

    void InitializeMovementCollider()
    {
        playerMovementCollider = gameObject.AddComponent<CapsuleCollider>();
        playerMovementCollider.height = AgentHeight;
        playerMovementCollider.center = Vector3.up * AgentHeight / 2;
        playerMovementCollider.radius = AgentRadius;

        nmo = gameObject.AddComponent<NavMeshObstacle>();
        nmo.height = playerMovementCollider.height;
        nmo.center = playerMovementCollider.center;
        nmo.radius = playerMovementCollider.radius;
        nmo.carving = false;
    }

    void InitializeMovementRigidbody()
    {
        playerMovementRigidbody = gameObject.AddComponent<Rigidbody>();
        playerMovementRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        playerMovementRigidbody.mass = AgentMass;
    }

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

    void SpawnMainCamera()
    {
        if (Camera.main == null)
        {
            Instantiate(mainCameraPrefab);
        }
    }

    void HandleCameraMode()
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

    void HandleCameraViewMode()
    {
        if (btnRpressed)
        {
            StaticVariables.IsCameraModeFirstPerson = !StaticVariables.IsCameraModeFirstPerson;

            HandleCameraMode();
        }

        if (btnTpressed)
        {
            IsCameraModeOrbital = !IsCameraModeOrbital;
        }
    }

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

    void HandleFootMovement()
    {
        // Movement related
        LayerMask walkableLayerMask = 1 << StaticVariables.Instance.DefaultLayer.value;
        isGrounded = Physics.CheckSphere(groundednessCheckerTransform.position, groundDistance, walkableLayerMask, QueryTriggerInteraction.Ignore);

        if (isGrounded)
        {
            localMoveDirXZ = Vector2.ClampMagnitude(new Vector2(moveInputX, moveInputY), 1.0f);

            Vector3 localMoveDir3D = new Vector3(localMoveDirXZ.x, 0, localMoveDirXZ.y);

            Vector3 worldMoveDir3D = transform.TransformDirection(localMoveDir3D);

            Vector3 worldVelocity3D = worldMoveDir3D * MovementSpeedLimit;

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
            playerMovementRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
        }
    }

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

        if (isDefTimer < isDefTimerThreshold && !isAtk)
        {
            isDef = true;
            isDefTimer += Time.deltaTime;
        }
    }

    void HandleCombatDirection()
    {
        // Assume combatDir hasn't changed.
        combatDir = lastCombatDir;

        if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
        {
            //it has to be left or right
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
            //it has to be up or down
            if (mouseY > 0)
            {
                combatDir = CombatDirection.Up;
            }
            if (mouseY < 0)
            {
                combatDir = CombatDirection.Down;
            }
        }

        bool wantToDefend = btnDefPressed; // def has higher precedence than atk
        bool wantToAttack = btnAtkPressed && !isDef && !btnDefPressed && !btnDefHeld;

        if (wantToDefend || wantToAttack)
        {
            AnimMgr.UpdateCombatDirection(combatDir);
        }

        lastCombatDir = combatDir;
    }

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

    void Start()
    {
        SpawnMainCamera();
        HandleCameraMode();
    }

    void Update()
    {
        if (IsDead)
        {
            playerMovementRigidbody.velocity = Vector3.zero;
            playerMovementRigidbody.angularVelocity = Vector3.zero;
            // playerMovementRigidbody.isKinematic = true;
            worldVelocityXZ = Vector2.zero;

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

    protected override void LateUpdate()
    {
        base.LateUpdate(); // let the spine be rotated

        // Move the camera to the position after the spine has been rotated.
        HandleCameraPosition();
    }

    void FixedUpdate()
    {
        Vector3 worldVelocity3D = new Vector3(worldVelocityXZ.x, 0, worldVelocityXZ.y);
        playerMovementRigidbody.MovePosition(playerMovementRigidbody.position + worldVelocity3D * Time.fixedDeltaTime);
    }
}
