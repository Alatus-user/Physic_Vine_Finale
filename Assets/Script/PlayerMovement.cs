using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    public LayerMask groundLayer;

    public LayerMask startZoneLayer;

    public bool gameOverOnGroundTouch = true;

    public Vector3 respawnPosition = new Vector3(-8.5f, 3f, 0f);

    private Rigidbody2D rb;
    private bool isOnGround;      // Touching dangerous ground
    private bool isOnStartZone;   // Touching safe start zone
    private float horizontalInput;
    private bool isGameOver = false;

    public bool CanMove { get; set; } = true;
    public bool IsGameOver => isGameOver;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!CanMove || isGameOver) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (groundCheck != null)
        {
            bool wasOnGround = isOnGround;

            isOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            isOnStartZone = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, startZoneLayer);

            if (gameOverOnGroundTouch && isOnGround && !wasOnGround)
            {
                TriggerGameOver();
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isOnStartZone)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (horizontalInput > 0.1f)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (horizontalInput < -0.1f)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    void FixedUpdate()
    {
        if (!CanMove || isGameOver) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        CanMove = false;
        rb.linearVelocity = Vector2.zero;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOver();
        }

        Time.timeScale = 0f;
    }

    public void Respawn()
    {
        Time.timeScale = 1f;

        transform.position = respawnPosition;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;

        isGameOver = false;
        CanMove = true;
        isOnGround = false;
        isOnStartZone = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;

            if (Application.isPlaying)
            {
                if (isOnStartZone) Gizmos.color = Color.cyan;     // Safe zone
                else if (isOnGround) Gizmos.color = Color.red;    // Danger
            }

            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(respawnPosition, 0.5f);
        Gizmos.DrawLine(respawnPosition + Vector3.up * 0.5f, respawnPosition + Vector3.down * 0.5f);
        Gizmos.DrawLine(respawnPosition + Vector3.left * 0.5f, respawnPosition + Vector3.right * 0.5f);
    }
}