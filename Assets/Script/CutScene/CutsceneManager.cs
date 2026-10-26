
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

    [Header("Dialogue Settings")]
    [SerializeField] private float dialogueDuration = 5f;
    [SerializeField] private List<DialogueSequence> dialogues = new();

    [Header("Ink Transition")]
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private VideoPlayer transitionPlayer;
    [SerializeField] private VideoClip inkFadeIn;
    [SerializeField] private VideoClip inkFadeOut;
    [SerializeField] private float transitionDuration = 5f;

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
        if (transitionPlayer != null)
        {
            transitionPlayer.playOnAwake = false;
            transitionPlayer.isLooping = false;
        }

        if (openingPanel != null)
            openingPanel.SetActive(true);

        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        if (transitionPanel != null)
            transitionPanel.SetActive(false);

        // Opening: 5-second ink fade in
        yield return PlayInkTransition(inkFadeIn, false);

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
}
