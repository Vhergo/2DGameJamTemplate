using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TrailRenderer))]
public class Player2DPlatformerMovement : MonoBehaviour
{
    [SerializeField] private bool isDead;
    public bool IsDead { get; set; }

    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;

    [SerializeField] private PlayerControls playerControls;
    private InputAction move;
    private InputAction jump;
    private InputAction dash;

    #region VARIABLES
    [Header("Available Controls")]
    [Tooltip("This is a test description for myVariable.")][SerializeField] private bool toggleJumpOff;
    [SerializeField] private bool toggleCrouchOff;
    [SerializeField] private bool toggleDoubleJumpOff;
    [SerializeField] private bool toggleDashOff;
    [SerializeField] private bool toggleWallGrabOff;
    [SerializeField] private bool toggleWallSlideOff;
    [SerializeField] private bool toggleWallClimbOff;
    [SerializeField] private bool toggleWallJumpOff;

    [Header("Components")]
    [SerializeField] public Rigidbody2D rb; // Set Public for Knockback
    [SerializeField] private TrailRenderer tr;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    // [SerializeField] private float groundDetectionRadius = 0.2f;
    [SerializeField] private Vector2 groundDetectionSize = new Vector2(1f, 0.2f);

    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallDetectionRadius = 0.2f;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float decceleration = 5f;
    [SerializeField] private bool conserveMomentum;
    private float originalMaxSpeed;
    private float accelForce;
    private float deccelForce;

    // In-air speed control
    [Range(0.01f, 1)][SerializeField] private float accelInAir = 1f;
    [Range(0.01f, 1)][SerializeField] private float deccelInAir = 1f;
    [SerializeField] private float airTime = 0.1f;
    private float airTimeCounter;

    // Facing direction
    [SerializeField] public bool isFacingRight = false;
    [SerializeField] public bool isMovingRight = false; // public for the weapon rotation
    private Vector2 moveDirection;

    public Vector2 MoveDirection { get; set; }

    [Header("Jump")]
    [SerializeField] private float jumpForce = 48f;
    [SerializeField] private float jumpHeight = 6f;
    [SerializeField] private float jumpTimeToApex = 0.25f;

    [Range(0, 1)][SerializeField] private float lowJumpMult = 0.55f; // percentage decrease
    [Range(0, 1)][SerializeField] private float fallGravityMult = 0.2f; // percentage increase
    [SerializeField] private bool isJumping;

    [SerializeField] private float gravityStrength;
    [SerializeField] private float gravityScale;
    [SerializeField] private float maxFallSpeed = 50f;

    [SerializeField] private int jumpLimit = 1;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float tempJumpLimit;
    private float jumpCounter;
    private float coyoteTimeCounter = 0;
    private float jumpBufferCounter = 0;
    private bool jumpInput;
    private bool isFalling;

    [Header("Crouch")]
    [SerializeField] private float crouchSpeed = 7f;
    [Range(0, 1)][SerializeField] private float crouchScaleAmount = 0.6f;
    private Vector3 originalCrouchScale;
    private Vector3 newCrouchScale;
    private bool crouchInput;
    public bool isCrouching = false;

    [Header("Dash")]
    [SerializeField] private float dashForce = 4f;
    [SerializeField] private float dashTime = 0.08f;
    [SerializeField] private float dashCooldown = 0.5f;

    [SerializeField] private Vector2 dashDir;
    [SerializeField] private bool canDash = true;
    [SerializeField] private bool isDashing = false;
    private float originalGravityScale;
    private bool dashInput;

    [Header("Wall Grab/Slide/Climb/Jump")]
    //Wall Grab
    [SerializeField] private float wallGrabTime = 4f;
    [SerializeField] private bool wallGrabInput = false;
    [SerializeField] private bool isWallGrabing = false;
    private float wallGrabCounter;
    private float tempWallGrabTime;

    //Wall Slide
    [SerializeField] private float wallSlideSpeed = 6f;
    [SerializeField] private bool wallSlideInput;
    [SerializeField] private bool isWallSliding = false;

    //Wall Climb
    [SerializeField] private float wallClimbSpeed = 6f;
    [SerializeField] private float wallClimbStaminaDrain = 1f;
    [SerializeField] private bool wallClimbInput = false;
    [SerializeField] private bool isWallClimbing = false;
    public float wallClimbDirection;

    //Wall Jump
    [SerializeField] private float wallJumpTime = 0.2f;
    [SerializeField] private float wallJumpDuration = 0.1f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(16, 20);
    private float wallJumpCounter;
    private float wallJumpDirection;
    private bool wallJumpInput = false;

    #endregion

    private void Awake()
    {
        playerControls = new PlayerControls();
        ValidateAndInitialize();
    }

    private void OnEnable()
    {
        move = playerControls.Player.Movement;
        jump = playerControls.Player.Jump;
        dash = playerControls.Player.Dash;

        move.Enable();
        jump.Enable();
        dash.Enable();

        jump.performed += JumpInput;
        dash.performed += DashInput;
    }

    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        dash.Disable();
    }

    void Start()
    {
        originalGravityScale = rb.gravityScale;

        originalCrouchScale = transform.localScale;
        originalMaxSpeed = maxSpeed;

        tempJumpLimit = jumpLimit;
        if (toggleDoubleJumpOff) jumpLimit = 1;
        jumpCounter = jumpLimit;

        tempWallGrabTime = wallGrabTime;
        if (toggleWallGrabOff) wallGrabTime = 0;
        wallGrabCounter = wallGrabTime;
    }

    void Update()
    {
        ProccessInput();
        Timers();
        Turn();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        if (!IsDead) Move();
        ControlToggles();
    }

    void JumpInput(InputAction.CallbackContext context) => jumpInput = true;
    void DashInput(InputAction.CallbackContext context) => dashInput = true;

    void ProccessInput()
    {
        //float horizontal = Input.GetAxisRaw("Horizontal");
        //float vertical = Input.GetAxisRaw("Vertical");
        //moveDirection = new Vector2(horizontal, vertical).normalized;
        moveDirection = move.ReadValue<Vector2>().normalized;

        wallClimbDirection = moveDirection.y;

        crouchInput = (Input.GetKey(KeyCode.LeftControl)) ? true : false;

        WallInteractions(moveDirection.x, moveDirection.y);

        isFalling = rb.velocity.y < 0 ? true : false;
        if (IsGrounded()) {
            if (isJumping) {
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(landSound);
            }

            isFalling = isJumping = false;
            jumpCounter = tempJumpLimit;
            airTimeCounter = airTime;
        }

        CoyoteTime();
        JumpBuffer();
        // CheckFacingDirection();

        if (Input.GetButtonUp("Jump")) LowJump();
        if (rb.velocity.y < 0) FallMultiplier();
    }

    void Move()
    {
        if (isCrouching) Mathf.Clamp(maxSpeed, 0, crouchSpeed);
        maxSpeed = isCrouching ? crouchSpeed : originalMaxSpeed;
        float targetSpeed = moveDirection.x * maxSpeed;

        float accelRate;
        if (airTimeCounter > 0) {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? accelForce : deccelForce;
        } else {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? accelForce * accelInAir : deccelForce * deccelInAir;
        }

        ConserveMomentum(targetSpeed, accelRate);

        float speedDif = targetSpeed - rb.velocity.x;
        float movement = speedDif * accelRate;

        rb.AddForce(Vector2.right * movement);
    }

    void Jump()
    {
        if ((jumpBufferCounter >= 0 && coyoteTimeCounter >= 0 && !isJumping) || (!IsGrounded() && isJumping && jumpCounter > 0)) {
            jumpInput = false;
            isJumping = true;
            jumpBufferCounter = 0;
            jumpCounter--;

            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(jumpSound);

            if (isCrouching) CrouchUp();
        }
    }

    void CrouchDown()
    {
        isCrouching = true;
        transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y * crouchScaleAmount);
    }

    void CrouchUp()
    {
        isCrouching = false;
        transform.localScale = new Vector2(transform.localScale.x, originalCrouchScale.y);
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        SetGravityScale(0);

        dashDir = moveDirection;
        if (dashDir == Vector2.zero) dashDir = new Vector2(-transform.localScale.x, 0);
        rb.velocity = new Vector2(dashDir.x * maxSpeed * dashForce, 0);
        tr.emitting = true;

        yield return new WaitForSeconds(dashTime);
        tr.emitting = false;
        SetGravityScale(originalGravityScale);
        isDashing = false;
        dashInput = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void WallGrab()
    {
        SetGravityScale(0);
        if (isWallClimbing && !toggleWallClimbOff) {
            WallClimb();
        } else {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }

    void WallSlide()
    {
        SetGravityScale(0);
        rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
    }

    // Wall climb duration is linked to all climb duration. Climbing takes double stamina
    void WallClimb()
    {
        SetGravityScale(0);
        if (wallClimbDirection != 0) {
            rb.velocity = new Vector2(rb.velocity.x, wallClimbDirection * wallClimbSpeed);
        }
        wallGrabCounter -= wallClimbStaminaDrain * Time.deltaTime;
    }

    void ProcessWallJump()
    {
        if (isWallSliding) {
            if (wallJumpDuration < 0) wallJumpInput = false;
            wallJumpDirection = transform.localScale.x; // gameObject direction seems to be flipped by default WHY?
            wallJumpCounter = wallJumpTime;
        } else {
            wallJumpCounter -= Time.deltaTime;
        }

        if (jumpInput && wallJumpCounter > 0) {
            wallJumpInput = true;
            jumpInput = false;
        }
    }

    private IEnumerator WallJump()
    {
        rb.velocity = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);
        wallJumpCounter = 0;

        yield return new WaitForSeconds(wallJumpDuration);
        wallJumpInput = false;
    }


    #region HELPER FUNCTIONS

    void ControlToggles()
    {
        if (jumpInput && !toggleJumpOff) Jump();
        if (crouchInput && !toggleCrouchOff && !isCrouching && !isJumping) {
            CrouchDown();
        } else if (!crouchInput && !toggleCrouchOff && isCrouching) {
            CrouchUp();
        }
        if (dashInput && canDash && !toggleDashOff) StartCoroutine(Dash());
        if (isWallGrabing && !toggleWallGrabOff) WallGrab();
        if (isWallSliding && !toggleWallSlideOff) WallSlide();
        // if (isWallClimbing && !toggleWallClimbOff) WallClimb();
        if (wallJumpInput && !toggleWallJumpOff) StartCoroutine(WallJump());
    }

    void ConserveMomentum(float targetSpeed, float accelRate)
    {
        bool fasterThanTargetSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed);
        bool checkDirection = Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed);
        bool targetSpeedAboveZero = Mathf.Abs(targetSpeed) > 0.01f;
        if (conserveMomentum && fasterThanTargetSpeed && checkDirection && targetSpeedAboveZero)
            accelRate = 0;
    }

    void WallInteractions(float horizontal, float vertical)
    {
        wallGrabInput = horizontal != 0 ? true : false;
        wallSlideInput = (wallGrabInput && toggleWallGrabOff) ? true : false;
        if (IsWalled() && !IsGrounded()) {
            if (wallGrabInput && wallGrabCounter > 0) {
                wallGrabCounter -= Time.deltaTime;
                isWallGrabing = true;
                isWallSliding = wallSlideInput ? true : false;
            } else {
                isWallSliding = true;
                isWallGrabing = false;
                if (!isDashing) SetGravityScale(originalGravityScale);
            }
        } else if (!IsWalled() && !IsGrounded()) {
            isWallSliding = false;
            isWallGrabing = false;
            if (!isDashing) SetGravityScale(originalGravityScale);
        } else {
            isWallSliding = false;
            isWallGrabing = false;
            wallGrabCounter = tempWallGrabTime;
            if (!isDashing) SetGravityScale(originalGravityScale);
        }
        wallClimbInput = vertical != 0 ? true : false;
        isWallClimbing = (IsWalled() && !IsGrounded() && wallClimbInput && wallGrabInput) ? true : false;
        ProcessWallJump();
    }

    void LowJump()
    {
        if (rb.velocity.y > 0) rb.AddForce(Vector2.down * rb.velocity.y * lowJumpMult, ForceMode2D.Impulse);
    }

    void FallMultiplier()
    {
        rb.AddForce(Vector2.up * rb.velocity.y * fallGravityMult / 10, ForceMode2D.Impulse);
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
    }

    void CoyoteTime()
    {
        if (IsGrounded()) {
            coyoteTimeCounter = coyoteTime;
        } else {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void JumpBuffer()
    {
        if (Input.GetButtonDown("Jump")) {
            jumpBufferCounter = jumpBufferTime;
        } else {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    void SetGravityScale(float gravityScale)
    {
        rb.gravityScale = gravityScale;
    }

    private bool IsGrounded()
    {
        // Circle detection has a hard time detecting the ground if the player is standing on the edge of a platform
        // return Physics2D.OverlapCircle(groundCheck.position, groundDetectionRadius, groundLayer);
        return Physics2D.OverlapBox(groundCheck.position, groundDetectionSize, 0f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, wallDetectionRadius, wallLayer);
    }

    void CheckFacingDirection()
    {
        // isMovingRight is not updated when stationary (no '==' case); 
        // shouldn't be true when stationary (NEED TO FIX)
        // isMovingRight = moveDirection.x > 0 ? true : moveDirection.x < 0 ? false : isMovingRight;
        isMovingRight = moveDirection.x > 0 ? true : false;
    }

    void Turn()
    {
        if (isDashing) return;
        if (MySceneManager.Instance != null && MySceneManager.Instance.gameState == GameState.Pause) return;

        if (moveDirection.x > 0 && !isFacingRight) {
            isFacingRight = true;
            Flip();
        } else if (moveDirection.x < 0 && isFacingRight) {
            isFacingRight = false;
            Flip();
        }

        // Needs player sprite to be the first child of this player object
        //if (moveDirection.x > 0) {
        //    gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;
        //} else if (moveDirection.x < 0) {
        //    gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;
        //}
    }

    private void Flip()
    {
        Vector3 playerScale = transform.localScale;
        playerScale.x *= -1;
        transform.localScale = playerScale;
    }

    void Timers()
    {
        airTimeCounter -= Time.deltaTime;
    }

    private void ValidateAndInitialize()
    {
        if (rb == null) {
            rb = GetComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Freeze Z rotation
        }

        accelForce = (50 * acceleration) / maxSpeed;
        deccelForce = (50 * decceleration) / maxSpeed;

        acceleration = Mathf.Clamp(acceleration, 0.01f, maxSpeed);
        decceleration = Mathf.Clamp(decceleration, 0.01f, maxSpeed);

        // Calculate The RigidBody2D Gravity Scale
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
        gravityScale = gravityStrength / Physics2D.gravity.y;
        rb.gravityScale = gravityScale;

        jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * rb.gravityScale));

        tempJumpLimit = toggleDoubleJumpOff ? 1 : jumpLimit;
        jumpCounter = tempJumpLimit;

        tempWallGrabTime = toggleWallGrabOff ? 0 : wallGrabTime;
        wallGrabCounter = tempWallGrabTime;
    }

    #endregion

    // -----------------------------------------------------

    void OnValidate()
    {
        ValidateAndInitialize();
    }
}