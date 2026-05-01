using UnityEngine;

public class PlayerRopeSwing : MonoBehaviour
{
    [Header("Swing Force")]
    [Tooltip("Force when swinging with A/D while attached")]
    public float swingForce = 25f;

    [Tooltip("Initial impulse on attach to keep momentum flowing (Benji feel)")]
    public float attachMomentumBoost = 1.3f;

    [Header("Release")]
    [Tooltip("Multiplier on velocity when releasing (Benji feel = 1.4 to 1.8)")]
    public float releaseMultiplier = 1.6f;

    [Tooltip("Extra upward boost on release")]
    public float releaseUpwardBoost = 4f;

    [Tooltip("Minimum forward velocity guaranteed on release")]
    public float minReleaseVelocity = 8f;

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

    public bool IsSwinging => currentJoint != null;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Press Space: attach if near rope, detach if already swinging
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
            // If not near rope, Space falls through to PlayerMovement (jump)
        }

        // While swinging: A/D adds swing force to the rope
        if (IsSwinging && attachedRope != null)
        {
            float swingInput = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(swingInput) > 0.1f)
            {
                attachedRope.AddForce(Vector2.right * swingInput * swingForce, ForceMode2D.Force);
            }

            // Rotate player to align with rope (Benji style)
            if (rotateWithRope)
            {
                RotatePlayerWithRope();
            }
        }
    }

    void RotatePlayerWithRope()
    {
        // Calculate angle from attached rope to player
        Vector2 ropeToPlayer = (Vector2)transform.position - attachedRope.position;
        float angle = Mathf.Atan2(ropeToPlayer.y, ropeToPlayer.x) * Mathf.Rad2Deg + 90f;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Find the nearest rope segment
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
        // Reset distance tracking each frame to find newest closest segment
        if (!IsSwinging)
        {
            nearestDistance = float.MaxValue;
        }
    }

    void AttachToRope(Rigidbody2D rope)
    {
        // Disable normal movement
        if (playerMovement != null) playerMovement.CanMove = false;

        // Allow rotation while swinging
        playerRb.constraints = RigidbodyConstraints2D.None;

        // Create joint connecting player to rope segment
        currentJoint = gameObject.AddComponent<HingeJoint2D>();
        currentJoint.connectedBody = rope;
        currentJoint.autoConfigureConnectedAnchor = false;
        currentJoint.anchor = Vector2.zero;
        currentJoint.connectedAnchor = Vector2.zero;

        // Transfer player momentum to rope WITH boost
        // This is the key Benji Bananas feel - rope keeps moving in direction player came from
        Vector2 momentum = playerRb.linearVelocity * attachMomentumBoost;
        rope.linearVelocity = momentum;

        attachedRope = rope;
    }

    void ReleaseRope()
    {
        if (currentJoint == null) return;

        // Capture current swing velocity (tangent to swing arc)
        Vector2 swingVelocity = playerRb.linearVelocity;

        Destroy(currentJoint);
        currentJoint = null;

        // Benji-style release:
        // 1. Multiply current velocity (preserves swing energy)
        // 2. Add upward boost (more dramatic arc)
        // 3. Guarantee minimum forward velocity
        Vector2 releaseVelocity = swingVelocity * releaseMultiplier;
        releaseVelocity.y += releaseUpwardBoost;

        // Ensure minimum horizontal speed in swing direction
        float horizontalSign = Mathf.Sign(releaseVelocity.x);
        if (Mathf.Abs(releaseVelocity.x) < minReleaseVelocity && horizontalSign != 0)
        {
            releaseVelocity.x = horizontalSign * minReleaseVelocity;
        }

        playerRb.linearVelocity = releaseVelocity;

        // Re-freeze rotation and reset
        playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.identity;

        // Restore movement
        if (playerMovement != null) playerMovement.CanMove = true;

        // Reset state
        lastDetachTime = Time.time;
        attachedRope = null;
        nearestRope = null;
        nearestDistance = float.MaxValue;
    }
}