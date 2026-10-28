
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SandFootstepParticles : MonoBehaviour
{
    [Header("Particle")]
    [SerializeField] private ParticleSystem sandParticles;
    [SerializeField] private int particlesPerStep = 15;

    [Header("Footstep Settings")]
    [SerializeField] private float stepInterval = 0.3f;
    [SerializeField] private float minimumSpeed = 0.2f;
    [SerializeField] private float heightOffset = 0.25f;
    [SerializeField] private float footSideOffset = 0.2f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayers = ~0;

    private CharacterController controller;
    private Vector3 previousPosition;
    private float stepTimer;
    private bool leftFoot = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 movement = transform.position - previousPosition;
        previousPosition = transform.position;

        if (sandParticles == null || Time.deltaTime <= 0f)
            return;

        movement.y = 0f;
        float speed = movement.magnitude / Time.deltaTime;

        if (speed < minimumSpeed)
        {
            stepTimer = 0f;
            return;
        }

        // Detect ground beneath player
        Vector3 origin = controller.bounds.center;
        float distance = controller.bounds.extents.y + 0.4f;

        if (!Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            distance,
            groundLayers,
            QueryTriggerInteraction.Ignore))
        {
            stepTimer = 0f;
            return;
        }

        // Check Sand tag
        Transform surface = hit.collider.transform;
        bool isSand = false;

        while (surface != null)
        {
            if (surface.CompareTag("Sand"))
            {
                isSand = true;
                break;
            }

            surface = surface.parent;
        }

        if (!isSand)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer > 0f)
            return;

        // Alternate left and right feet
        float side = leftFoot ? -footSideOffset : footSideOffset;

        Vector3 spawnPosition =
            hit.point +
            Vector3.up * heightOffset +
            transform.right * side;

        sandParticles.transform.position = spawnPosition;

        if (!sandParticles.isPlaying)
            sandParticles.Play();

        sandParticles.Emit(particlesPerStep);

        leftFoot = !leftFoot;
        stepTimer = stepInterval;
    }
}
