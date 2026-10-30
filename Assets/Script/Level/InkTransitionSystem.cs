
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class InkSceneTransition : MonoBehaviour
{
    public static InkSceneTransition Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private GameObject loadingCover;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Ink Videos")]
    [SerializeField] private VideoClip inkFadeIn;
    [SerializeField] private VideoClip inkFadeOut;

    public static event System.Action FadeInStarted;
    public static event System.Action FadeOutStarted;
    public bool IsBusy { get; private set; } = true;

    private void Awake()
    {
        // Keep only one transition system
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.playbackSpeed = 1f;
        videoPlayer.source = VideoSource.VideoClip;

        transitionPanel.SetActive(true);

        if (loadingCover != null)
            loadingCover.SetActive(true);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Play Fade In when the game first starts
        StartCoroutine(FadeInRoutine());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Play Fade In after loading a new scene
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        IsBusy = true;

        yield return PlayInkVideo(inkFadeIn, true);

        transitionPanel.SetActive(false);
        IsBusy = false;
    }

    // Call this instead of SceneManager.LoadScene()
    public void LoadSceneWithInk(string sceneName)
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Next scene is not assigned!");
            return;
        }

        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        IsBusy = true;

        // Resume time if the game was paused
        Time.timeScale = 1f;

        // Play Fade Out before changing scenes
        yield return PlayInkVideo(inkFadeOut, false);

        // Keep screen covered during loading
        transitionPanel.SetActive(true);

        if (loadingCover != null)
            loadingCover.SetActive(true);

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("Scene could not be loaded: " + sceneName);
            transitionPanel.SetActive(false);
            IsBusy = false;
        }
    }

    private IEnumerator PlayInkVideo(VideoClip clip, bool isFadeIn)
    {
        if (isFadeIn)
        {
            transitionPanel.SetActive(true);

            if (loadingCover != null)
                loadingCover.SetActive(true);
        }
        else
        {
            transitionPanel.SetActive(false);
        }

        if (clip == null || videoPlayer == null)
        {
            Debug.LogWarning("Ink video is not assigned!");
            yield break;
        }

        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        // Wait until video is prepared
        float prepareTime = 0f;

        while (!videoPlayer.isPrepared && prepareTime < 10f)
        {
            prepareTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError("Ink video preparation failed.");
            yield break;
        }

        bool finished = false;

        VideoPlayer.EventHandler onFinished =
            vp => finished = true;

        VideoPlayer.ErrorEventHandler onError =
            (vp, message) =>
            {
                Debug.LogError("Ink video error: " + message);
                finished = true;
            };

        videoPlayer.loopPointReached += onFinished;
        videoPlayer.errorReceived += onError;


        transitionPanel.SetActive(true);
        videoPlayer.Play();

        // Start BGM Fade In / Fade Out with Ink Transition
        if (isFadeIn)
        {
            FadeInStarted?.Invoke();
        }
        else
        {
            FadeOutStarted?.Invoke();
        }

        if (loadingCover != null)
            loadingCover.SetActive(false);


        // Wait until the video finishes
        while (!finished)
            yield return null;

        videoPlayer.loopPointReached -= onFinished;
        videoPlayer.errorReceived -= onError;

        if (isFadeIn)
            videoPlayer.Stop();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}
