using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAgent : Agent
{
    // TODO: Remove[SerializeField]. It is for debugging purposes only.

    float cameraOffsetYcur = 0.5f;
    float cameraOffsetYmin = -0.6f;
    float cameraOffsetYmax = 1.6f;
    float cameraOffsetYchangeSpeed = 3.0f;

    float cameraZoomLerpRate = 0.2f;
    float cameraZoomMultiSpeed = 3.0f;
    float cameraZoomMultiCur = 0.6f;
    float cameraZoomMultiMin = 0.5f;
    float cameraZoomMultiMax = 1.5f;

    public Camera cam;
    public Transform cameraPivotTransform; // camera will be a child of this transform
    public Transform groundednessCheckerTransform; // used for checking if the player is grounded

    CapsuleCollider playerMovementCollider;
    Rigidbody playerMovementRigidbody;

    // Foot movement fields
    float moveInputX;
    float moveInputY;
    Vector2 localMoveDirXZ;
    Vector2 worldVelocityXZ;

    float jumpPower = 4.0f;
    [SerializeField] float jumpCooldownTimer;
    [SerializeField] float jumpCooldownTimerMax = 1.0f;
    [Range(0.01f, 10.0f)] public float groundDistance = 0.3f;
    [SerializeField] bool isGrounded;

    // Rotation fields
    float mouseX;
    float mouseY;
    public static float PlayerCameraRotationSpeed = 45.0f;
    float playerAgentYaw; // yawing the player agent (left right about Y axis)
    const float EyesPitchThreshold = 89.0f;

    NavMeshObstacle nmo;

    [SerializeField] CombatDirection lastCombatDir;
    [SerializeField] CombatDirection combatDir;

    // Combat inputs
    [SerializeField] bool btnAtkPressed;
    [SerializeField] bool btnAtkHeld;
    [SerializeField] bool btnDefPressed;
    [SerializeField] bool btnDefHeld;
    [SerializeField] bool btnDefReleased;
    [SerializeField] bool btnJumpPressed;
    [SerializeField] bool btnShiftHeld;

    // Below are not inputs, but they depend on inputs.
    [SerializeField] bool isAtk;
    [SerializeField] bool isDef;
    [SerializeField] float isDefTimer;
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
    }

    void HandleCameraZoom()
    {
        if (btnShiftHeld == false)
        {
            cameraZoomMultiCur -= Input.mouseScrollDelta.y * Time.deltaTime * cameraZoomMultiSpeed;
            cameraZoomMultiCur = Mathf.Clamp(cameraZoomMultiCur, cameraZoomMultiMin, cameraZoomMultiMax);
        }
        else
        {
            cameraOffsetYcur += Input.mouseScrollDelta.y * Time.deltaTime * cameraOffsetYchangeSpeed;
            cameraOffsetYcur = Mathf.Clamp(cameraOffsetYcur, cameraOffsetYmin, cameraOffsetYmax);
        }

        // Using Vector3.zero, because the camera is supposed to be the child of cameraPivotTransform.
        // You can remove "Vector3.zero", but this is more explicit.
        Vector3 cameraOffset = new Vector3(0, cameraOffsetYcur, -1);
        Vector3 destination = Vector3.zero + cameraOffset * cameraZoomMultiCur;

        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, destination, cameraZoomLerpRate);
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

    void HandleEyeRotation()
    {
        playerAgentYaw += PlayerCameraRotationSpeed * mouseX * Time.deltaTime;
        LookAngleX -= PlayerCameraRotationSpeed * mouseY * Time.deltaTime; // Subtracting, because negative angle about X axis means "up".

        LookAngleX = Mathf.Clamp(LookAngleX, -EyesPitchThreshold, EyesPitchThreshold);

        // First, reset all rotations.
        transform.rotation = Quaternion.identity;
        cameraPivotTransform.rotation = Quaternion.identity;

        // Then, rotate with the new angles.
        transform.Rotate(Vector3.up, playerAgentYaw);
        cameraPivotTransform.Rotate(Vector3.right, LookAngleX);
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

        // 
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

    void Update()
    {
        if (IsDead)
        {
            playerMovementRigidbody.velocity = Vector3.zero;
            playerMovementRigidbody.angularVelocity = Vector3.zero;
            // playerMovementRigidbody.isKinematic = true;

            // Detach the camera from the player.
            if (cam.transform.parent != null)
            {
                cam.transform.parent = null;
            }
            return;
        }

        ReadInputs();

        HandleCameraZoom();
        HandleFootMovement();
        HandleEyeRotation();

        HandleCombatInputs();
        HandleCombatDirection();

        AnimMgr.UpdateAnimations(localMoveDirXZ, currentMovementSpeed, isGrounded, isAtk, isDef);
    }

    void FixedUpdate()
    {
        Vector3 worldVelocity3D = new Vector3(worldVelocityXZ.x, 0, worldVelocityXZ.y);
        playerMovementRigidbody.MovePosition(playerMovementRigidbody.position + worldVelocity3D * Time.fixedDeltaTime);
    }
}
