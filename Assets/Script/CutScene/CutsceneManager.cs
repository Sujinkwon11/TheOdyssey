
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;

[Serializable]
public class DialogueSequence
{
    public Sprite image;

    [TextArea(3, 6)]
    public string dialogue;
}

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject openingPanel;
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;

    [Header("Dialogue Settings")]
    [SerializeField] private float dialogueDuration = 5f;
    [SerializeField] private List<DialogueSequence> dialogues = new();

    [Header("Ink Transition")]
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private VideoPlayer transitionPlayer;
    [SerializeField] private VideoClip inkFadeIn;
    [SerializeField] private VideoClip inkFadeOut;
    [SerializeField] private float transitionDuration = 5f;

    [Header("Dialogue Fade")]
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private float dialogueFadeDuration = 1f;

    private Coroutine dialogueFadeRoutine;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    private enum CutsceneState
    {
        Opening,
        Dialogue,
        Closing,
        Finished
    }

    private CutsceneState state = CutsceneState.Opening;
    private int currentIndex = -1;
    private float timer;


    private IEnumerator Start()
    {
        if (dialogueCanvasGroup != null)
            dialogueCanvasGroup.alpha = 0f;

        if (transitionPlayer != null)
        {
            transitionPlayer.playOnAwake = false;
            transitionPlayer.isLooping = false;
        }

        // Hide opening panel
        if (openingPanel != null)
            openingPanel.SetActive(false);

        // Show cutscene image behind the transition
        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        // Hide dialogue box during opening transition
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (transitionPanel != null)
            transitionPanel.SetActive(false);

        // Display first cutscene image
        if (dialogues.Count > 0)
            ShowDialogue(0);

        // Play 5-second ink fade in
        yield return PlayInkTransition(inkFadeIn, false);

        // Start dialogues
        state = CutsceneState.Dialogue;
        NextSequence();
    }


    private void Update()
    {
        if (state != CutsceneState.Dialogue)
            return;

        // Enter to skip dialogue
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame))
        {
            NextSequence();
            return;
        }

        timer += Time.unscaledDeltaTime;

        if (timer >= dialogueDuration)
            NextSequence();
    }

    private void NextSequence()
    {
        timer = 0f;
        currentIndex++;

        if (currentIndex >= dialogues.Count)
        {
            state = CutsceneState.Closing;
            StartCoroutine(EndCutscene());
            return;
        }

        if (openingPanel != null)
            openingPanel.SetActive(false);

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        ShowDialogue(currentIndex);

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowDialogue(currentIndex);

        // First dialogue: Fade In
        if (currentIndex == 0)
        {
            StartDialogueFade(1f);
        }
    }

    private void ShowDialogue(int index)
    {
        DialogueSequence sequence = dialogues[index];

        if (dialogueText != null)
            dialogueText.text = sequence.dialogue;

        if (cutsceneImage != null)
        {
            cutsceneImage.sprite = sequence.image;
            cutsceneImage.enabled = sequence.image != null;

            if (sequence.image != null)
            {
                AspectRatioFitter fitter =
                    cutsceneImage.GetComponent<AspectRatioFitter>();

                if (fitter != null)
                {
                    fitter.aspectMode =
                        AspectRatioFitter.AspectMode.EnvelopeParent;

                    fitter.aspectRatio =
                        sequence.image.rect.width /
                        sequence.image.rect.height;
                }
            }
        }
    }

    private IEnumerator EndCutscene()
    {
        // Last dialogue finished: Fade Out
        StartDialogueFade(0f);

        // Closing: 5-second ink fade out
        bool loadingNextScene =
            !string.IsNullOrWhiteSpace(nextSceneName);

        yield return PlayInkTransition(
            inkFadeOut,
            loadingNextScene
        );

        state = CutsceneState.Finished;

        if (loadingNextScene)
        {
            SceneManager.LoadSceneAsync(nextSceneName);
        }
        else
        {
            Debug.Log(
                "Cutscene finished. No next scene assigned."
            );
        }
    }

    private IEnumerator PlayInkTransition(
        VideoClip clip,
        bool keepVisibleAfter
    )
    {
        float duration = Mathf.Max(0.1f, transitionDuration);

        if (clip == null ||
            transitionPlayer == null ||
            transitionPanel == null)
        {
            // If no video is assigned, preserve the timing
            yield return new WaitForSecondsRealtime(duration);
            yield break;
        }

        transitionPlayer.Stop();
        transitionPlayer.clip = clip;
        transitionPlayer.Prepare();

        // Wait for video preparation
        float prepareTimer = 0f;

        while (!transitionPlayer.isPrepared &&
               prepareTimer < 10f)
        {
            prepareTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!transitionPlayer.isPrepared)
        {
            Debug.LogWarning("Ink transition could not prepare.");
            yield return new WaitForSecondsRealtime(duration);
            yield break;
        }

        // Fit video playback duration to 5 seconds
        if (transitionPlayer.canSetPlaybackSpeed &&
            transitionPlayer.length > 0)
        {
            transitionPlayer.playbackSpeed =
                (float)(transitionPlayer.length / duration);
        }

        transitionPanel.SetActive(true);
        transitionPlayer.Play();

        yield return new WaitForSecondsRealtime(duration);

        if (keepVisibleAfter)
        {
            // Keep the final ink-covered frame while loading
            transitionPlayer.Pause();
        }
        else
        {
            transitionPlayer.Stop();
            transitionPanel.SetActive(false);
        }
    }


    private void StartDialogueFade(float targetAlpha)
    {
        if (dialogueCanvasGroup == null)
            return;

        if (dialogueFadeRoutine != null)
            StopCoroutine(dialogueFadeRoutine);

        dialogueFadeRoutine =
            StartCoroutine(FadeDialogue(targetAlpha));
    }

    private IEnumerator FadeDialogue(float targetAlpha)
    {
        float startAlpha = dialogueCanvasGroup.alpha;
        float elapsed = 0f;

        float duration = Mathf.Max(0.01f, dialogueFadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth fade
            t = t * t * (3f - 2f * t);

            dialogueCanvasGroup.alpha =
                Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        dialogueCanvasGroup.alpha = targetAlpha;
        dialogueFadeRoutine = null;
    }

}
