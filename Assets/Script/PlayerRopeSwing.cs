using UnityEngine;

public class PlayerRopeSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    [Tooltip("แรงแกว่งซ้ายขวาตอนเกาะเชือก")]
    public float swingForce = 15f;

    [Tooltip("แรงพุ่งขึ้นตอนปล่อยเชือก")]
    public float jumpOffForce = 8f;

    [Tooltip("ตัวคูณ momentum ตอนปล่อย (1 = ใช้ momentum เต็ม)")]
    public float momentumMultiplier = 1.2f;

    [Header("Detection")]
    [Tooltip("เผื่อ delay กันการเกาะแล้วปล่อยทันที")]
    public float reattachCooldown = 0.3f;

    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;
    private HingeJoint2D currentJoint = null;
    private Rigidbody2D nearestRope = null;
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

        if (IsSwinging)
        {
            float swingInput = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(swingInput) > 0.1f && nearestRope != null)
            {
                nearestRope.AddForce(Vector2.right * swingInput * swingForce, ForceMode2D.Force);
            }
        }
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
        if (playerMovement != null) playerMovement.CanMove = false;

        currentJoint = gameObject.AddComponent<HingeJoint2D>();
        currentJoint.connectedBody = rope;
        currentJoint.autoConfigureConnectedAnchor = false;
        currentJoint.anchor = Vector2.zero;
        currentJoint.connectedAnchor = Vector2.zero;

        rope.linearVelocity = playerRb.linearVelocity * 0.5f;

        nearestRope = rope;
    }

    void ReleaseRope()
    {
        if (currentJoint == null) return;

        Vector2 swingVelocity = playerRb.linearVelocity;

        Destroy(currentJoint);
        currentJoint = null;

        playerRb.linearVelocity = swingVelocity * momentumMultiplier + Vector2.up * jumpOffForce;

        if (playerMovement != null) playerMovement.CanMove = true;

        lastDetachTime = Time.time;
        nearestRope = null;
        nearestDistance = float.MaxValue;
    }
}