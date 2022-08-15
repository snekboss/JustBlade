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

    public LayerMask defaultLayer;

    public CapsuleCollider playerMovementCollider;
    public Rigidbody playerMovementRigidbody;
    float playerMovementColliderRadius = 0.29f;
    float playerMovementRigidbodyMass = 70.0f;

    // Foot movement fields
    float moveX;
    float moveY;
    float jumpPower = 4.0f;
    public float movementSpeed = 5.0f;
    [SerializeField] float jumpCooldownTimer;
    [SerializeField] float jumpCooldownTimerMax = 1.0f;
    [Range(0.01f, 10.0f)] public float groundDistance = 0.3f;
    Vector3 playerMoveDir;
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

        eqMgr = GetComponent<EquipmentManager>();
        animMgr = GetComponent<AnimationManager>();

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
        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxis("Vertical");

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
        isGrounded = Physics.CheckSphere(groundednessCheckerTransform.position, groundDistance, defaultLayer, QueryTriggerInteraction.Ignore);

        if (isGrounded)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");
            playerMoveDir = Vector3.ClampMagnitude(new Vector3(moveX, 0, moveY), 1);
            playerMoveDir = transform.TransformDirection(playerMoveDir);
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
            animMgr.SetJump(true);
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
            animMgr.UpdateCombatDirection(combatDir);
        }

        lastCombatDir = combatDir;
    }

    void Update()
    {
        ReadInputs();

        HandleCameraZoom();
        HandleFootMovement();
        HandleEyeRotation();

        HandleCombatInputs();
        HandleCombatDirection();

        animMgr.UpdateAnimations(moveX, moveY, isGrounded, isAtk, isDef);
    }

    void FixedUpdate()
    {
        playerMovementRigidbody.MovePosition(playerMovementRigidbody.position + playerMoveDir * movementSpeed * Time.fixedDeltaTime);
    }
}
