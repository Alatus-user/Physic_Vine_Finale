using UnityEngine;

/// <summary>
/// Benji Bananas style camera follow.
/// Player is locked to a specific screen position (e.g. left-center)
/// while the world scrolls past them.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Range(0f, 1f)]
    public float screenX = 0.3f;

    [Range(0f, 1f)]
    public float screenY = 0.5f;

    public float smoothTimeX = 0.15f;

    public float smoothTimeY = 0.3f;

    public bool useLookAhead = true;

    public float lookAheadDistance = 2f;

    public float lookAheadSmooth = 3f;

    public float verticalDeadzone = 1.5f;

    public bool snapYOnGround = false;

    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-100, -100);
    public Vector2 maxBounds = new Vector2(100, 100);

    private Vector3 velocity = Vector3.zero;
    private Vector2 currentLookAhead = Vector2.zero;
    private Rigidbody2D targetRb;
    private Camera cam;
    private float originalZ;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("BenjiCameraFollow must be on a Camera GameObject!");
            enabled = false;
            return;
        }

        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
        }

        originalZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 screenOffset = CalculateScreenOffset();

        Vector2 lookAhead = Vector2.zero;
        if (useLookAhead && targetRb != null)
        {
            Vector2 targetLookAhead = targetRb.linearVelocity.normalized * lookAheadDistance;
            currentLookAhead = Vector2.Lerp(currentLookAhead, targetLookAhead, lookAheadSmooth * Time.deltaTime);
            lookAhead = currentLookAhead;
        }

        Vector3 targetPos = target.position - screenOffset + (Vector3)lookAhead;
        targetPos.z = originalZ;

        float newX = Mathf.SmoothDamp(transform.position.x, targetPos.x, ref velocity.x, smoothTimeX);

        float newY = transform.position.y;
        float yDistance = Mathf.Abs(target.position.y - transform.position.y - (-screenOffset.y));

        if (yDistance > verticalDeadzone || verticalDeadzone <= 0f)
        {
            newY = Mathf.SmoothDamp(transform.position.y, targetPos.y, ref velocity.y, smoothTimeY);
        }

        Vector3 finalPos = new Vector3(newX, newY, originalZ);

        if (useBounds)
        {
            finalPos.x = Mathf.Clamp(finalPos.x, minBounds.x, maxBounds.x);
            finalPos.y = Mathf.Clamp(finalPos.y, minBounds.y, maxBounds.y);
        }

        transform.position = finalPos;
    }

    private Vector3 CalculateScreenOffset()
    {
        if (cam.orthographic)
        {
            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;

            float offsetX = (screenX - 0.5f) * camWidth;
            float offsetY = (screenY - 0.5f) * camHeight;

            return new Vector3(offsetX, offsetY, 0);
        }
        else
        {
            float distance = Mathf.Abs(originalZ - target.position.z);
            float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * cam.aspect;

            float offsetX = (screenX - 0.5f) * frustumWidth;
            float offsetY = (screenY - 0.5f) * frustumHeight;

            return new Vector3(offsetX, offsetY, 0);
        }
    }

    void OnDrawGizmos()
    {
        if (target == null) return;

        Camera c = GetComponent<Camera>();
        if (c == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position, 0.3f);

        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.3f);
        Gizmos.DrawLine(transform.position, target.position);
    }
}