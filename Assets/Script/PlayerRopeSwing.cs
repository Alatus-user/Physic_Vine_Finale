using UnityEngine;

public class PlayerRopeSwing : MonoBehaviour
{
    [Header("Swing Force")]
    [Tooltip("Force when swinging with A/D while attached")]
    public float swingForce = 25f;

    [Tooltip("Initial impulse on attach to keep momentum flowing (Benji feel)")]
    public float attachMomentumBoost = 1.3f;

    [Header("Release - Momentum Carry")]
    [Tooltip("Multiplier on horizontal velocity (preserves forward momentum)")]
    public float releaseHorizontalMultiplier = 1.8f;

    [Tooltip("Multiplier on vertical velocity (controls upward arc)")]
    public float releaseVerticalMultiplier = 1.2f;

    [Tooltip("Minimum forward velocity guaranteed on release")]
    public float minReleaseVelocity = 10f;

    [Header("Air Float - Carried Momentum")]
    [Tooltip("Duration of reduced gravity after releasing (seconds)")]
    public float floatDuration = 0.6f;

    [Tooltip("Gravity multiplier during float (0.3 = floaty, 1.0 = normal)")]
    [Range(0.1f, 1f)]
    public float floatGravityScale = 0.4f;

    [Tooltip("Air drag during float (0 = preserve momentum forever)")]
    [Range(0f, 1f)]
    public float airDrag = 0.05f;

    [Header("Rotation")]
    [Tooltip("Rotate player to face swing direction")]
    public bool rotateWithRope = true;
    public float rotationSmoothSpeed = 10f;

    [Header("Detection")]
    [Tooltip("Cooldown to prevent instant re-attach after release")]
    public float reattachCooldown = 0.4f;

    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;
    private HingeJoint2D currentJoint = null;
    private Rigidbody2D nearestRope = null;
    private Rigidbody2D attachedRope = null;
    private float nearestDistance = float.MaxValue;
    private float lastDetachTime = -999f;

    // Momentum carry state
    private bool isFloating = false;
    private float floatTimer = 0f;
    private float originalGravityScale;
    private float originalLinearDamping;

    public bool IsSwinging => currentJoint != null;
    public bool IsFloating => isFloating;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();

        // Remember original physics settings
        originalGravityScale = playerRb.gravityScale;
        originalLinearDamping = playerRb.linearDamping;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsSwinging)
            {
                ReleaseRope();
            }
            else if (nearestRope != null && Time.time - lastDetachTime > reattachCooldown)
            {
                AttachToRope(nearestRope);
            }
        }

        // While swinging: A/D adds swing force to the rope
        if (IsSwinging && attachedRope != null)
        {
            float swingInput = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(swingInput) > 0.1f)
            {
                attachedRope.AddForce(Vector2.right * swingInput * swingForce, ForceMode2D.Force);
            }

            if (rotateWithRope)
            {
                RotatePlayerWithRope();
            }
        }

        // Handle float state (carried momentum after release)
        if (isFloating)
        {
            floatTimer -= Time.deltaTime;

            if (floatTimer <= 0f)
            {
                EndFloat();
            }
        }
    }

    void RotatePlayerWithRope()
    {
        Vector2 ropeToPlayer = (Vector2)transform.position - attachedRope.position;
        float angle = Mathf.Atan2(ropeToPlayer.y, ropeToPlayer.x) * Mathf.Rad2Deg + 90f;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Rope") && !IsSwinging)
        {
            float dist = Vector2.Distance(transform.position, other.transform.position);
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestRope = other.attachedRigidbody;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Rope") && other.attachedRigidbody == nearestRope && !IsSwinging)
        {
            nearestRope = null;
            nearestDistance = float.MaxValue;
        }
    }

    void LateUpdate()
    {
        if (!IsSwinging)
        {
            nearestDistance = float.MaxValue;
        }
    }

    void AttachToRope(Rigidbody2D rope)
    {
        // Cancel float state if attaching while floating
        if (isFloating)
        {
            EndFloat();
        }

        if (playerMovement != null) playerMovement.CanMove = false;

        playerRb.constraints = RigidbodyConstraints2D.None;

        currentJoint = gameObject.AddComponent<HingeJoint2D>();
        currentJoint.connectedBody = rope;
        currentJoint.autoConfigureConnectedAnchor = false;
        currentJoint.anchor = Vector2.zero;
        currentJoint.connectedAnchor = Vector2.zero;

        Vector2 momentum = playerRb.linearVelocity * attachMomentumBoost;
        rope.linearVelocity = momentum;

        attachedRope = rope;
    }

    void ReleaseRope()
    {
        if (currentJoint == null) return;

        // Capture swing velocity (tangent of swing arc)
        Vector2 swingVelocity = playerRb.linearVelocity;

        Destroy(currentJoint);
        currentJoint = null;

        // Apply release velocity with separate X/Y multipliers
        // This preserves natural swing arc - no forced upward boost
        Vector2 releaseVelocity = new Vector2(
            swingVelocity.x * releaseHorizontalMultiplier,
            swingVelocity.y * releaseVerticalMultiplier
        );

        // Guarantee minimum forward velocity in swing direction
        float horizontalSign = Mathf.Sign(releaseVelocity.x);
        if (Mathf.Abs(releaseVelocity.x) < minReleaseVelocity && horizontalSign != 0)
        {
            releaseVelocity.x = horizontalSign * minReleaseVelocity;
        }

        playerRb.linearVelocity = releaseVelocity;

        // Reset rotation
        playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.identity;

        // START FLOAT - this is the "carried momentum" feel
        StartFloat();

        if (playerMovement != null) playerMovement.CanMove = true;

        lastDetachTime = Time.time;
        attachedRope = null;
        nearestRope = null;
        nearestDistance = float.MaxValue;
    }

    void StartFloat()
    {
        isFloating = true;
        floatTimer = floatDuration;

        // Reduce gravity so player floats forward longer
        playerRb.gravityScale = originalGravityScale * floatGravityScale;

        // Add slight air drag so momentum naturally decays
        playerRb.linearDamping = airDrag;
    }

    void EndFloat()
    {
        isFloating = false;
        floatTimer = 0f;

        // Restore normal physics
        playerRb.gravityScale = originalGravityScale;
        playerRb.linearDamping = originalLinearDamping;
    }

    // Cancel float when player lands on ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFloating && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            EndFloat();
        }
    }
}