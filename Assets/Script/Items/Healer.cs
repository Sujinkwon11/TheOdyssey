using UnityEngine;
using UnityEngine.Audio;

public class Healer : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioMixerGroup masterMixerGroup;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null) return;

        isCollected = true;

        playerHealth.AddHealth(healAmount);

        if (collectSound != null)
        {
            GameObject soundObject = new GameObject("HealthPickupSound");
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = masterMixerGroup;

            source.clip = collectSound;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;

            source.Play();

            Destroy(soundObject, collectSound.length);
        }

        Destroy(gameObject);
    }
}