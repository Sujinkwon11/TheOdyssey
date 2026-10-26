using UnityEngine;

public class WaveTile : MonoBehaviour
{
    public float minHeight = 0.05f;
    public float maxHeight = 0.25f;

    public float minSpeed = 0.5f;
    public float maxSpeed = 1.5f;

    private float startY;
    private float waveHeight;
    private float waveSpeed;
    private float randomOffset;

    void Start()
    {
        startY = transform.position.y;

        // 육각형마다 랜덤한 움직임
        waveHeight = Random.Range(minHeight, maxHeight);
        waveSpeed = Random.Range(minSpeed, maxSpeed);
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float newY =
            startY +
            Mathf.Sin(Time.time * waveSpeed + randomOffset) * waveHeight;

        transform.position = new Vector3(
            transform.position.x,
            newY,
            transform.position.z
        );
    }
}