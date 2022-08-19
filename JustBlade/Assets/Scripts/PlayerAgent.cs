using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgent : Agent
{
    public Transform headBone;
    public Transform rootBone;
    // TODO: Remove[SerializeField]. It is for debugging purposes only.

    Vector3 cameraOffsetFromPivotDirection = new Vector3(0, 0.3f, -1.3f);
    float cameraZoomLerpRate = 0.2f;
    float cameraZoomMultiSpeed = 3.0f;
    float cameraZoomMultiCur = 1.0f;
    float cameraZoomMultiMin = 0.8f;
    float cameraZoomMultiMax = 1.5f;

    public Camera cam;
    public Transform cameraPivotTransform; // camera will be a child of this transform
    public Transform groundednessCheckerTransform; // used for checking if the player is grounded

    CapsuleCollider playerMovementCollider;
    Rigidbody playerMovementRigidbody;
    float playerMovementColliderRadius = 0.29f;
    float playerMovementRigidbodyMass = 70.0f;

    // Foot movement fields
    float moveInputX;
    float moveInputY;
    Vector2 localMoveDirXZ;
    Vector2 worldVelocityXZ;
    public override float CurrentMovementSpeed { get; protected set; }

    float jumpPower = 4.0f;
    [SerializeField] float jumpCooldownTimer;
    [SerializeField] float jumpCooldownTimerMax = 1.0f;
    [Range(0.01f, 10.0f)] public float groundDistance = 0.3f;
    [SerializeField] bool isGrounded;

    // Rotation fields
    float mouseX;
    float mouseY;
    public static float playerCameraRotationSpeed = 45.0f;
    float playerAgentYaw; // yawing the player agent (left right about Y axis)
    const float EyesPitchThreshold = 89.0f;

    [SerializeField] CombatDirection lastCombatDir;
    [SerializeField] CombatDirection combatDir;

    // Combat inputs
    [SerializeField] bool btnAtkPressed;
    [SerializeField] bool btnAtkHeld;
    [SerializeField] bool btnDefPressed;
    [SerializeField] bool btnDefHeld;
    [SerializeField] bool btnDefReleased;
    [SerializeField] bool btnJumpPressed;
    [SerializeField] bool btnKickPressed;

    // Below are not inputs, but they depend on inputs.
    [SerializeField] bool isAtk;
    [SerializeField] bool isDef;
    [SerializeField] float isDefTimer;
    float isDefTimerThreshold = 0.5f;

    void Awake()
    {
        gameObject.layer = StaticVariables.Instance.AgentLayer;

        isDefTimer = 2 * isDefTimerThreshold; // set it far above the threshold, so that the condition is not satisfied at the start.

        InitializeMovementCollider();
        InitializeMovementRigidbody();
    }

    void InitializeMovementCollider()
    {
        float height = headBone.transform.position.y - rootBone.transform.position.y;
        playerMovementCollider = gameObject.AddComponent<CapsuleCollider>();
        playerMovementCollider.height = height;
        Vector3 rootBoneLocalPos = transform.InverseTransformPoint(rootBone.transform.position);
        playerMovementCollider.center = rootBoneLocalPos + Vector3.up * height / 2;
        playerMovementCollider.radius = playerMovementColliderRadius;
    }

    void InitializeMovementRigidbody()
    {
        playerMovementRigidbody = gameObject.AddComponent<Rigidbody>();
        playerMovementRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        playerMovementRigidbody.mass = playerMovementRigidbodyMass;
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
        btnKickPressed = Input.GetKeyDown(KeyCode.E);
    }

    void HandleCameraZoom()
    {
        cameraZoomMultiCur -= Input.mouseScrollDelta.y * Time.deltaTime * cameraZoomMultiSpeed;
        cameraZoomMultiCur = Mathf.Clamp(cameraZoomMultiCur, cameraZoomMultiMin, cameraZoomMultiMax);

        // Using Vector3.zero, because the camera is supposed to be the child of cameraPivotTransform.
        // You can remove "Vector3.zero", but this is more explicit.
        Vector3 destination = Vector3.zero + cameraOffsetFromPivotDirection * cameraZoomMultiCur;

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

            CurrentMovementSpeed = worldVelocityXZ.magnitude;
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
        playerAgentYaw += playerCameraRotationSpeed * mouseX * Time.deltaTime;
        lookAngleX -= playerCameraRotationSpeed * mouseY * Time.deltaTime; // Subtracting, because negative angle about X axis means "up".

        lookAngleX = Mathf.Clamp(lookAngleX, -EyesPitchThreshold, EyesPitchThreshold);

        // First, reset all rotations.
        transform.rotation = Quaternion.identity;
        cameraPivotTransform.rotation = Quaternion.identity;

        // Then, rotate with the new angles.
        transform.Rotate(Vector3.up, playerAgentYaw);
        cameraPivotTransform.Rotate(Vector3.right, lookAngleX);
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

        AnimMgr.UpdateAnimations(localMoveDirXZ, isGrounded, isAtk, isDef);
    }

    void FixedUpdate()
    {
        Vector3 worldVelocity3D = new Vector3(worldVelocityXZ.x, 0, worldVelocityXZ.y);
        playerMovementRigidbody.MovePosition(playerMovementRigidbody.position + worldVelocity3D * Time.fixedDeltaTime);
    }
}
