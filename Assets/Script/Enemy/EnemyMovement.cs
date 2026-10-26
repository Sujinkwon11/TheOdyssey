using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    private Vector3 startPosition;
    private int direction = 1;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

        float distanceFromStart = transform.position.x - startPosition.x;

        if (distanceFromStart >= moveDistance)
        {
            direction = -1;
        }
        else if (distanceFromStart <= -moveDistance)
        {
            direction = 1;
        }
    }
}