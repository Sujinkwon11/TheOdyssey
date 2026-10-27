
using UnityEngine;
using UnityEngine.Audio;

public class KillZone : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private AudioMixerGroup masterMixerGroup;

    [Header("Splash Particle")]
    [SerializeField] private ParticleSystem splashEffectPrefab;
    [SerializeField] private Transform waterSurface;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play splash sound
            if (splashSound != null)
            {
                GameObject soundObject =
                    new GameObject("SplashSound");

                AudioSource source =
                    soundObject.AddComponent<AudioSource>();

                source.clip = splashSound;
                source.outputAudioMixerGroup = masterMixerGroup;
                source.spatialBlend = 0f;
                source.dopplerLevel = 0f;

                source.Play();

                Destroy(soundObject, splashSound.length);
            }


            if (splashEffectPrefab != null)
            {
                Debug.Log("Splash Prefab found!");

                float waterY = waterSurface != null
                    ? waterSurface.position.y
                    : transform.position.y;

                Vector3 splashPosition = new Vector3(
                    other.transform.position.x,
                    waterY + 1.5f,
                    other.transform.position.z
                );

                ParticleSystem splash = Instantiate(
                    splashEffectPrefab,
                    splashPosition,
                    Quaternion.identity
                );

                splash.gameObject.SetActive(true);
                splash.Play(true);

                Debug.Log("Splash created at: " + splashPosition);
            }
            else
            {
                Debug.LogError("Splash Prefab is NOT assigned!");
            }


            // Respawn player
            PlayerRespawn playerRespawn =
                other.GetComponentInParent<PlayerRespawn>();

            if (playerRespawn != null)
            {
                playerRespawn.Respawn();
            }
        }
    }
}
