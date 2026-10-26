
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[Serializable]
public class LevelSelectData
{
    public string levelName;
    public Sprite artwork;
    public string sceneName;
}

public class LevelSelectManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image levelArtwork;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("UI Sounds")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip navigateSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip lockedSound;

    private void PlayUISound(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null)
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }

    [Header("Debug Controls")]
    [SerializeField, TextArea(4, 6)]
    private string debugNotes =
        "LEFT / RIGHT - Navigate Levels\n" +
        "ENTER - Select Level\n" +
        "P - Unlock Selected Level\n" +
        "Q - Reset All Progress\n\n" +
        "Debug controls only work in Editor / Development Build.";


    [Header("Levels")]
    [SerializeField] private List<LevelSelectData> levels = new();

    private int currentIndex = 0;
    private bool isLoading = false;

    private void Start()
    {
        // Connect UI buttons automatically
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousLevel);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextLevel);

        UpdateLevelDisplay();
    }

    private void Update()
    {
        // Disable input during Ink Transition
        if (InkSceneTransition.Instance != null &&
            InkSceneTransition.Instance.IsBusy)
        {
            return;
        }

        if (isLoading || levels.Count == 0 ||
            Keyboard.current == null)
            return;

        // Navigate using arrow keys
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            PreviousLevel();
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            NextLevel();
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD

        // DEBUG: Press P to unlock the selected level
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            int levelNumber = currentIndex + 1;

            if (!LevelProgress.IsUnlocked(levelNumber))
            {
                LevelProgress.CompleteLevel(currentIndex);
                UpdateLevelDisplay();

                Debug.Log("[DEBUG] Level " + levelNumber + " unlocked!");
            }

            return;
        }

        // DEBUG: Press Q to reset all level progress
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            LevelProgress.ResetProgress();

            // Return to Level 1
            currentIndex = 0;

            // Refresh lock states and UI
            UpdateLevelDisplay();
            ClearButtonSelection();

            Debug.Log("[DEBUG] All levels reset!");

            return;
        }

#endif



        // Press Enter to select a level
        else if (Keyboard.current.enterKey.wasPressedThisFrame ||
                 Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            EnterLevel();
        }
    }


    private void PreviousLevel()
    {
        if (currentIndex > 0)
        {
            currentIndex--;

            PlayUISound(navigateSound);

            UpdateLevelDisplay();
        }

        ClearButtonSelection();
    }

    private void NextLevel()
    {
        if (currentIndex < levels.Count - 1)
        {
            currentIndex++;

            PlayUISound(navigateSound);

            UpdateLevelDisplay();
        }

        ClearButtonSelection();
    }

    private void ClearButtonSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void UpdateLevelDisplay()
    {
        if (levels.Count == 0)
        {
            Debug.LogWarning("No levels assigned!");
            return;
        }

        LevelSelectData selectedLevel = levels[currentIndex];

        // Change level artwork
        if (levelArtwork != null)
            levelArtwork.sprite = selectedLevel.artwork;

        // Check whether the level is unlocked
        int levelNumber = currentIndex + 1;
        bool unlocked = currentIndex == 0 ||
                LevelProgress.IsUnlocked(levelNumber);

        // Show / hide locked overlay
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!unlocked);

        // Disable navigation at the first and last levels
        if (previousButton != null)
            previousButton.interactable = currentIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentIndex < levels.Count - 1;
    }


    private void EnterLevel()
    {
        int levelNumber = currentIndex + 1;

        // Check if level is locked
        bool unlocked = currentIndex == 0 ||
                        LevelProgress.IsUnlocked(levelNumber);

        if (!unlocked)
        {
            PlayUISound(lockedSound);
            Debug.Log("Level is locked!");
            return;
        }

        string sceneName = levels[currentIndex].sceneName;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Scene name is not assigned!");
            return;
        }

        isLoading = true;

        PlayUISound(selectSound);

        StartCoroutine(LoadLevelWithSound(sceneName));
    }


    private IEnumerator LoadLevelWithSound(string sceneName)
    {
        // Wait for the select sound
        if (selectSound != null && uiAudioSource != null)
        {
            yield return new WaitForSecondsRealtime(selectSound.length);
        }

        // Wait if the opening transition is still playing
        if (InkSceneTransition.Instance != null)
        {
            while (InkSceneTransition.Instance.IsBusy)
                yield return null;

            // Fade Out before loading the level
            InkSceneTransition.Instance.LoadSceneWithInk(sceneName);
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }



    private void OnDestroy()
    {
        if (previousButton != null)
            previousButton.onClick.RemoveListener(PreviousLevel);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(NextLevel);
    }

}
