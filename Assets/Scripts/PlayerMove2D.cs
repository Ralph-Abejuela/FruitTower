using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;

    [Header("Jump Settings")]
    [SerializeField] private float maxJumpForce = 12f;
    [SerializeField] private float maxChargeTime = 1f;
    [SerializeField] private Vector2 jumpAngle = new Vector2(0.5f, 1f);

    [Header("Materials")]
    [SerializeField] private PhysicsMaterial2D bouncyMaterial; // Assign your Bouncy Material here
    [SerializeField] private PhysicsMaterial2D slipperyMaterial; // Assign your Zero Bounce Material here

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // State Variables
    private Rigidbody2D rb;
    private Collider2D col; // Reference to the collider to swap materials
    private SpriteRenderer sr; // Reference to the collider to swap materials
    private Animator anim;
    private bool isGrounded;
    private bool isCharging;
    private bool isJumping;
    private float chargeTimer;
    private float inputX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>(); // Get the BoxCollider2D
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        jumpAngle.Normalize();
    }

    private void Update()
    {
        // 1. Check Ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 2. Read Input
        inputX = Input.GetAxisRaw("Horizontal");

        if (inputX > 0)
        {
            sr.flipX = true;
        }
        else if (inputX < 0)
        {
            sr.flipX = false;
        }

        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isCharging", isCharging);

        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        // LOGIC: Material Swapping
        // If we are on the ground and NOT jumping, we want to be heavy and non-bouncy.
        // If we are in the air OR jumping, we want to be bouncy for wall hits.

        if (isGrounded && !isJumping)
        {
            // 1. Switch to Slippery (No Bounce) so we don't bounce off the floor
            if (col.sharedMaterial != slipperyMaterial)
                col.sharedMaterial = slipperyMaterial;

            // 2. Movement Logic
            if (isCharging)
            {
                rb.velocity = new Vector2(0, -1f); // Stick to ground
            }
            else
            {
                rb.velocity = new Vector2(inputX * walkSpeed, -1f); // Walk and stick
            }
        }
        else
        {
            // AIR / JUMPING BEHAVIOR

            // 1. Switch to Bouncy so we bounce off walls
            if (col.sharedMaterial != bouncyMaterial)
                col.sharedMaterial = bouncyMaterial;

            // 2. No movement inputs allowed in air (pure physics)
        }
    }

    private void HandleJumpInput()
    {
        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                isCharging = true;
                chargeTimer += Time.deltaTime;
                if (chargeTimer > maxChargeTime) chargeTimer = maxChargeTime;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (isCharging)
                {
                    ExecuteJump();
                }
            }
        }
        else
        {
            // Reset if falling
            isCharging = false;
            chargeTimer = 0f;
        }
    }

    private void ExecuteJump()
    {
        float jumpPowerPercent = chargeTimer / maxChargeTime;
        float finalForce = Mathf.Max(jumpPowerPercent * maxJumpForce, 2f);

        Vector2 direction = Vector2.up;
        if (inputX > 0) direction = new Vector2(jumpAngle.x, jumpAngle.y);
        else if (inputX < 0) direction = new Vector2(-jumpAngle.x, jumpAngle.y);

        isJumping = true;

        // Apply Force
        rb.AddForce(direction * finalForce, ForceMode2D.Impulse);

        isCharging = false;
        chargeTimer = 0f;

        // Reset jump flag shortly after launch
        Invoke("ResetJumpFlag", 0.1f);
    }

    private void ResetJumpFlag()
    {
        isJumping = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

}