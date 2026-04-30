using UnityEngine;

public class RopeGenerator : MonoBehaviour
{
    [Header("Rope Settings")]
    [Tooltip("Prefab ของ RopeSegment")]
    public GameObject segmentPrefab;

    [Tooltip("จำนวนข้อของเชือก")]
    public int segmentCount = 8;

    [Tooltip("ระยะห่างระหว่างข้อ (หน่วย Unity)")]
    public float segmentSpacing = 1f;

    [Header("Editor Visualization")]
    [Tooltip("แสดง preview ของเชือกใน Scene view")]
    public bool showGizmo = true;
    public Color gizmoColor = new Color(0.4f, 0.8f, 0.4f, 0.8f); // สีเขียว
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

    // วาด preview ใน Scene view ตอน edit mode
    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        // วาดจุดวงกลมตรงตำแหน่งจุดยึด
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoAnchorRadius);

        // วาดวงกลมรอบๆ เพื่อให้สังเกตเห็นง่าย
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, gizmoAnchorRadius * 1.5f);

        // วาดเส้นจำลองเชือกห้อยลงมา
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.6f);
        Vector3 endPos = transform.position + Vector3.down * segmentSpacing * (segmentCount - 1);
        Gizmos.DrawLine(transform.position, endPos);

        // วาดจุดเล็กๆ ตรงตำแหน่งของแต่ละ segment
        for (int i = 1; i < segmentCount; i++)
        {
            Vector3 segPos = transform.position + Vector3.down * segmentSpacing * i;
            Gizmos.DrawWireSphere(segPos, 0.08f);
        }

        // วาดจุดปลายเชือก
        Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.8f); // สีส้ม
        Gizmos.DrawSphere(endPos, 0.12f);
    }

    // วาดเฉพาะตอน selected (เน้นชัดขึ้น)
    void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Gizmos.color = Color.yellow;
        Vector3 endPos = transform.position + Vector3.down * segmentSpacing * (segmentCount - 1);
        Gizmos.DrawLine(transform.position, endPos);
    }
}