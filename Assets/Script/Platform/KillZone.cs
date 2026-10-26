using UnityEngine;
using UnityEngine.Audio;

public class KillZone : MonoBehaviour
{
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private AudioMixerGroup masterMixerGroup;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play splash sound
            if (splashSound != null)
            {
                GameObject soundObject = new GameObject("SplashSound");
                AudioSource source = soundObject.AddComponent<AudioSource>();

                source.clip = splashSound;
                source.outputAudioMixerGroup = masterMixerGroup;
                source.spatialBlend = 0f;
                source.dopplerLevel = 0f;

                source.Play();
                Destroy(soundObject, splashSound.length);
            }

            // Damage + Respawn
            PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();

            if (playerRespawn != null)
            {
                playerRespawn.Respawn();
            }
        }
    }
}