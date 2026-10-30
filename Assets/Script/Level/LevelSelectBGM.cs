
using System.Collections;
using UnityEngine;

public class LevelSelectBGM : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;

    [SerializeField, Range(0f, 1f)]
    private float targetVolume = 0.8f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 5f;
    [SerializeField] private float fadeOutDuration = 5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();

        if (bgmSource != null)
        {
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
            bgmSource.volume = 0f;
        }
    }

    private void OnEnable()
    {
        InkSceneTransition.FadeInStarted += OnFadeIn;
        InkSceneTransition.FadeOutStarted += OnFadeOut;
    }

    private void OnDisable()
    {
        InkSceneTransition.FadeInStarted -= OnFadeIn;
        InkSceneTransition.FadeOutStarted -= OnFadeOut;
    }

    private void OnFadeIn()
    {
        if (bgmSource == null || bgmSource.clip == null)
            return;

        if (!bgmSource.isPlaying)
            bgmSource.Play();

        StartFade(targetVolume, fadeInDuration);
    }

    private void OnFadeOut()
    {
        StartFade(0f, fadeOutDuration);
    }

    private void StartFade(float target, float duration)
    {
        if (bgmSource == null)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(
            FadeVolume(target, duration)
        );
    }

    private IEnumerator FadeVolume(float target, float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth fade
            t = t * t * (3f - 2f * t);

            bgmSource.volume =
                Mathf.Lerp(startVolume, target, t);

            yield return null;
        }

        bgmSource.volume = target;

        if (target <= 0f)
            bgmSource.Stop();

        fadeRoutine = null;
    }
}
