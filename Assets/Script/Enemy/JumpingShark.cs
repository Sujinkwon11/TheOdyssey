using System.Collections.Generic;
using UnityEngine;

// Put this on the shark root, with a Rigidbody and CapsuleCollider.
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class JumpingShark : MonoBehaviour
{
    [Header("Route (keep these outside the moving shark)")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Timing and shape")]
    [Tooltip("Height above the straight line between the two points.")]
    [SerializeField, Min(0f)] private float jumpHeight = 1.5f;
    [SerializeField, Min(0.1f)] private float jumpDuration = 1.2f;
    [SerializeField, Min(0f)] private float pauseBetweenJumps = 1.5f;
    [SerializeField, Min(0f)] private float warningDuration = 0.6f;

    [Header("Damage")]
    [SerializeField, Min(1)] private int damage = 25;

    [Header("Optional warning")]
    [Tooltip("A separate marker over the dangerous tile. Do not assign the shark or its parent.")]
    [SerializeField] private GameObject warningMarker;

    private enum Phase { Resting, Warning, Jumping }
    private Phase phase;
    private Rigidbody body;
    private float elapsed;
    private bool goingToB;
    private Vector3 launchPosition;
    private Vector3 landingPosition;
    private readonly HashSet<SharkDemoHealth> hitThisJump = new HashSet<SharkDemoHealth>();

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        GetComponent<CapsuleCollider>().isTrigger = true;
    }

    private void OnEnable()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("JumpingShark: assign Point A and Point B in the Inspector.", this);
            enabled = false;
            return;
        }

        if (pointA.IsChildOf(transform) || pointB.IsChildOf(transform))
        {
            Debug.LogError("JumpingShark: route points must be outside the moving shark.", this);
            enabled = false;
            return;
        }

        if (warningMarker != null &&
            (transform.IsChildOf(warningMarker.transform) || warningMarker.transform.IsChildOf(transform)))
        {
            Debug.LogError("JumpingShark: use a separate warning object outside the shark hierarchy.", this);
            enabled = false;
            return;
        }

        body.position = pointA.position;
        goingToB = true;
        elapsed = 0f;
        phase = Phase.Resting;
        hitThisJump.Clear();
        SetWarning(false);
    }

    private void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;

        if (phase == Phase.Resting)
        {
            if (elapsed < pauseBetweenJumps) return;
            phase = Phase.Warning;
            elapsed = 0f;
            SetWarning(true);
            return;
        }

        if (phase == Phase.Warning)
        {
            if (elapsed < warningDuration) return;
            if (pointA == null || pointB == null)
            {
                enabled = false;
                return;
            }
            launchPosition = body.position;
            landingPosition = goingToB ? pointB.position : pointA.position;
            hitThisJump.Clear();
            phase = Phase.Jumping;
            elapsed = 0f;
            SetWarning(false);
            return;
        }

        // Leave the landing step active for one physics simulation so it can hit.
        if (elapsed > Mathf.Max(0.1f, jumpDuration) + Time.fixedDeltaTime)
        {
            goingToB = !goingToB;
            phase = Phase.Resting;
            elapsed = 0f;
            return;
        }

        float t = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, jumpDuration));
        body.MovePosition(PositionOnArc(launchPosition, landingPosition, t));

        // The placeholder's nose should point along local +Z (the blue axis).
        Vector3 direction = landingPosition - launchPosition;
        direction += Vector3.up * (4f * jumpHeight * (1f - 2f * t));
        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 up = Mathf.Abs(Vector3.Dot(direction.normalized, Vector3.up)) > 0.99f
                ? Vector3.forward : Vector3.up;
            body.MoveRotation(Quaternion.LookRotation(direction, up));
        }
    }

    private Vector3 PositionOnArc(Vector3 start, Vector3 end, float t)
    {
        // 4*t*(1-t) is zero at either end and one at the midpoint.
        return Vector3.Lerp(start, end, t) + Vector3.up * (4f * jumpHeight * t * (1f - t));
    }

    private void OnTriggerEnter(Collider other) => TryDamage(other);
    private void OnTriggerStay(Collider other) => TryDamage(other);

    private void TryDamage(Collider other)
    {
        if (!isActiveAndEnabled || phase != Phase.Jumping) return;
        // Works when the player's collider is on a child of the health object.
        SharkDemoHealth health = other.GetComponentInParent<SharkDemoHealth>();
        if (health != null && hitThisJump.Add(health))
            health.TakeDamage(damage);
    }

    private void SetWarning(bool visible)
    {
        if (warningMarker != null) warningMarker.SetActive(visible);
    }

    private void OnDisable()
    {
        // Do not toggle a misassigned parent or the shark itself.
        if (warningMarker != null && !transform.IsChildOf(warningMarker.transform)
            && !warningMarker.transform.IsChildOf(transform))
            SetWarning(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;
        Gizmos.color = Color.cyan;
        Vector3 previous = pointA.position;
        for (int i = 1; i <= 32; i++)
        {
            Vector3 next = PositionOnArc(pointA.position, pointB.position, i / 32f);
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
        Gizmos.DrawWireSphere(pointA.position, 0.2f);
        Gizmos.DrawWireSphere(pointB.position, 0.2f);
    }
}
