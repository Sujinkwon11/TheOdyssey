using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private AudioClip collectSound;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;

        isCollected = true;

        if (collectSound != null)
        {
            GameObject soundObject = new GameObject("CollectSound");

            AudioSource source = soundObject.AddComponent<AudioSource>();

            source.clip = collectSound;
            source.pitch = 1f;
            source.spatialBlend = 0f;   // 2D
            source.dopplerLevel = 0f;   // Doppler OFF

            source.Play();

            Destroy(soundObject, collectSound.length);
        }

        Destroy(gameObject);
    }
}