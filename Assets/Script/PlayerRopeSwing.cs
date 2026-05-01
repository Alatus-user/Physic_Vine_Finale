using UnityEngine;


public class PlayerRopeSwing : MonoBehaviour
{
    [Header("Swing Acceleration (F = ma)")]
    [Tooltip("Tangential acceleration applied to rope when swinging (m/s^2)")]
    public float swingAcceleration = 25f;

    public float attachMomentumBoost = 1.3f;

    public float releaseHorizontalMultiplier = 1.8f;

    public float releaseVerticalMultiplier = 1.2f;

    public float minReleaseVelocity = 10f;

    [Header("Air Control - Mid-Air Movement (F = ma)")]
    [Tooltip("Enable horizontal control while in air after release")]
    public bool enableAirControl = true;

    [Tooltip("Horizontal acceleration in air when pressing A/D (m/s^2)")]
    public float airAcceleration = 20f;

    public float maxAirSpeed = 20f;

    public float airControlDuration = 0f;

    [Header("Air Float - Carried Momentum (Air Resistance)")]
    public float floatDuration = 0.6f;

    [Range(0.1f, 1f)]
    public float floatGravityScale = 0.4f;

    [Tooltip("Air resistance coefficient during float (linear damping)")]
    [Range(0f, 1f)]
    public float airDrag = 0f;

    [Header("Rotation")]
    public bool rotateWithRope = true;
    public float rotationSmoothSpeed = 10f;

    public float reattachCooldown = 0.4f;

    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;
    private HingeJoint2D currentJoint = null;
    private Rigidbody2D nearestRope = null;
    private Rigidbody2D attachedRope = null;
    private float nearestDistance = float.MaxValue;
    private float lastDetachTime = -999f;

    private bool isFloating = false;
    private float floatTimer = 0f;
    private bool hasAirControl = false;
    private float airControlTimer = 0f;
    private float originalGravityScale;
    private float originalLinearDamping;

    public bool IsSwinging => currentJoint != null;
    public bool IsFloating => isFloating;
    public bool HasAirControl => hasAirControl;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();

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

        // Apply swing force using F = ma while attached
        if (IsSwinging && attachedRope != null)
        {
            float swingInput = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(swingInput) > 0.1f)
            {
                ApplySwingForce(swingInput);
            }

            if (rotateWithRope)
            {
                RotatePlayerWithRope();
            }
        }

        if (isFloating)
        {
            floatTimer -= Time.deltaTime;
            if (floatTimer <= 0f)
            {
                EndFloat();
            }
        }

        if (hasAirControl && airControlDuration > 0f)
        {
            airControlTimer -= Time.deltaTime;
            if (airControlTimer <= 0f)
            {
                hasAirControl = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Mid-air control after release using F = ma
        if (hasAirControl && !IsSwinging)
        {
            float airInput = Input.GetAxisRaw("Horizontal");

            if (Mathf.Abs(airInput) > 0.1f)
            {
                ApplyAirControlForce(airInput);
            }
        }
    }

    private void ApplySwingForce(float input)
    {
        // F = m * a
        // m = mass of the attached rope segment
        // a = swingAcceleration (m/s^2) in direction of input
        float mass = attachedRope.mass;
        float acceleration = swingAcceleration;
        Vector2 forceDirection = Vector2.right * input;

        Vector2 force = mass * acceleration * forceDirection; // F = ma

        attachedRope.AddForce(force, ForceMode2D.Force);
    }

    private void ApplyAirControlForce(float input)
    {
        // F = m * a
        // m = mass of player
        // a = airAcceleration (m/s^2) in direction of input
        float mass = playerRb.mass;
        float acceleration = airAcceleration;
        Vector2 forceDirection = Vector2.right * input;

        Vector2 force = mass * acceleration * forceDirection; // F = ma

        float currentSpeed = playerRb.linearVelocity.x;
        bool sameDirection = Mathf.Sign(currentSpeed) == Mathf.Sign(input);

        if (!sameDirection || Mathf.Abs(currentSpeed) < maxAirSpeed)
        {
            playerRb.AddForce(force, ForceMode2D.Force);
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
        if (isFloating) EndFloat();
        hasAirControl = false;

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

        Vector2 swingVelocity = playerRb.linearVelocity;

        Destroy(currentJoint);
        currentJoint = null;

 
        Vector2 releaseVelocity = new Vector2(
            swingVelocity.x * releaseHorizontalMultiplier,
            swingVelocity.y * releaseVerticalMultiplier
        );

        float horizontalSign = Mathf.Sign(releaseVelocity.x);
        if (Mathf.Abs(releaseVelocity.x) < minReleaseVelocity && horizontalSign != 0)
        {
            releaseVelocity.x = horizontalSign * minReleaseVelocity;
        }

        playerRb.linearVelocity = releaseVelocity;

        playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.identity;

        StartFloat();

        if (enableAirControl)
        {
            hasAirControl = true;
            airControlTimer = airControlDuration;
        }

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

        playerRb.gravityScale = originalGravityScale * floatGravityScale;
        playerRb.linearDamping = airDrag;
    }

    void EndFloat()
    {
        isFloating = false;
        floatTimer = 0f;

        playerRb.gravityScale = originalGravityScale;
        playerRb.linearDamping = originalLinearDamping;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFloating) EndFloat();
        if (hasAirControl) hasAirControl = false;
    }
}