
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LightningFlash : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image flashImage;

    [Header("Thunder Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip thunderSound;

    [Header("Lightning Settings")]
    [SerializeField] private float flashInterval = 5f;
    [SerializeField] private float flashDuration = 0.4f;

    [SerializeField, Range(0f, 1f)]
    private float flashIntensity = 1f;

    private void Awake()
    {
        if (flashImage != null)
        {
            flashImage.raycastTarget = false;
            SetFlashAlpha(0f);
        }
    }

    private IEnumerator Start()
    {
        if (flashImage == null)
        {
            Debug.LogError("Flash Image is not assigned!");
            yield break;
        }


        while (true)
        {
            // Random interval between 4 and 8 seconds
            yield return new WaitForSeconds(Random.Range(4f, 8f));

            yield return StartCoroutine(PlayLightning());
        }

    }

    private IEnumerator PlayLightning()
    {
        // Instantly flash the screen
        SetFlashAlpha(flashIntensity);

        // Brief bright flash
        yield return new WaitForSeconds(0.08f);

        // Play thunder sound
        if (audioSource != null && thunderSound != null)
        {
            audioSource.PlayOneShot(thunderSound);
        }

        // Gradually fade out the white screen
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, flashDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            SetFlashAlpha(
                Mathf.Lerp(flashIntensity, 0f, t)
            );

            yield return null;
        }

        SetFlashAlpha(0f);
    }

    private void SetFlashAlpha(float alpha)
    {
        if (flashImage == null)
            return;

        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
    }

    private void OnDisable()
    {
        SetFlashAlpha(0f);
    }
}
