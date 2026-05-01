using UnityEngine;

public class RopeGenerator : MonoBehaviour
{
    public GameObject segmentPrefab;

    public int segmentCount = 8;

    public float segmentSpacing = 1f;

    public bool showGizmo = true;
    public Color gizmoColor = new Color(0.4f, 0.8f, 0.4f, 0.8f); 
    public float gizmoAnchorRadius = 0.2f;

    void Start()
    {
        GenerateRope();
    }

    void GenerateRope()
    {
        Rigidbody2D previousSegment = null;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + Vector2.down * segmentSpacing * i;
            GameObject segment = Instantiate(segmentPrefab, spawnPos, Quaternion.identity, transform);
            segment.name = $"RopeSegment_{i}";

            Rigidbody2D rb = segment.GetComponent<Rigidbody2D>();
            HingeJoint2D joint = segment.GetComponent<HingeJoint2D>();

            if (i == 0)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                if (joint != null) Destroy(joint);
            }
            else
            {
                joint.connectedBody = previousSegment;
            }

            previousSegment = rb;
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoAnchorRadius);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, gizmoAnchorRadius * 1.5f);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.6f);
        Vector3 endPos = transform.position + Vector3.down * segmentSpacing * (segmentCount - 1);
        Gizmos.DrawLine(transform.position, endPos);

        for (int i = 1; i < segmentCount; i++)
        {
            Vector3 segPos = transform.position + Vector3.down * segmentSpacing * i;
            Gizmos.DrawWireSphere(segPos, 0.08f);
        }

        Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.8f); 
        Gizmos.DrawSphere(endPos, 0.12f);
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Gizmos.color = Color.yellow;
        Vector3 endPos = transform.position + Vector3.down * segmentSpacing * (segmentCount - 1);
        Gizmos.DrawLine(transform.position, endPos);
    }
}