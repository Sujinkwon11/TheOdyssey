using UnityEngine;
using StarterAssets;

public class Boost : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float duration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController controller =
                other.GetComponent<ThirdPersonController>();

            if (controller != null)
            {
                controller.ApplySpeedBoost(speedMultiplier, duration);
            }
        }
    }
}