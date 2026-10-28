
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum StartDirection
    {
        Left,
        Right
    }

    [Header("Patrol Settings")]
    [SerializeField] private float moveDistance = 3f;

    [Header("Random Speed")]
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 3f;

    [Header("Starting Direction")]
    [SerializeField] private StartDirection startDirection = StartDirection.Right;

    private Vector3 startPosition;
    private Vector3 segmentStart;
    private Vector3 segmentEnd;

    private float currentSpeed;
    private float elapsedTime;
    private float travelDuration;
    private int direction;

    void Start()
    {
        startPosition = transform.position;

        currentSpeed = Random.Range(
            Mathf.Max(0.01f, Mathf.Min(minSpeed, maxSpeed)),
            Mathf.Max(0.01f, Mathf.Max(minSpeed, maxSpeed))
        );

        direction = startDirection == StartDirection.Right ? 1 : -1;

        segmentStart = startPosition;
        SetNextTarget();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        float t = Mathf.Clamp01(elapsedTime / travelDuration);

        // Ease In & Out
        float easedT = Mathf.SmoothStep(0f, 1f, t);

        transform.position = Vector3.Lerp(
            segmentStart,
            segmentEnd,
            easedT
        );

        if (t >= 1f)
        {
            segmentStart = segmentEnd;
            direction *= -1;
            SetNextTarget();
        }
    }

    private void SetNextTarget()
    {
        segmentEnd = startPosition +
            Vector3.right * direction * Mathf.Max(0.01f, moveDistance);

        travelDuration = Vector3.Distance(
            segmentStart,
            segmentEnd
        ) / currentSpeed;

        elapsedTime = 0f;
    }
}
