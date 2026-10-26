using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StarterAssets;

public class GameSettings : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public GameObject settingsPanel;

    [Header("Fade")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.5f;

    [Header("Player Control")]
    public StarterAssetsInputs playerInput;
    public ThirdPersonController thirdPersonController;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);

        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
    }
    public void GoToHome()
    {
        StartCoroutine(GoToHomeWithFade());
    }

    private IEnumerator GoToHomeWithFade()
    {
        // Pause 상태에서도 Fade가 작동하도록 먼저 게임 재개
        Time.timeScale = 1f;

        float time = 0f;
        fadePanel.alpha = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 1f;

        SceneManager.LoadScene("Level");
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat(
            "MasterVolume",
            Mathf.Log10(volume) * 20f
        );

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);

        // Pause the game
        Time.timeScale = 0f;

        // Clear current player input
        if (playerInput != null)
        {
            playerInput.move = Vector2.zero;
            playerInput.look = Vector2.zero;
            playerInput.jump = false;
            playerInput.sprint = false;
            playerInput.enabled = false;
        }

        // Unlock and show the mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseSettings()
{
    settingsPanel.SetActive(false);

    // Resume the game
    Time.timeScale = 1f;

    // Enable player input
    if (playerInput != null)
    {
        playerInput.enabled = true;
        playerInput.look = Vector2.zero;
    }

    // Lock and hide the mouse cursor
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}
}