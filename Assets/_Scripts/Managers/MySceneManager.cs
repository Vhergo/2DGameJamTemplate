using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private float loadingScreenDuration = 2f;
    private bool isPaused;

    [Header("Scene Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.O;
    [SerializeField] private KeyCode restartKey = KeyCode.P;

    public SceneEnum currentScene = SceneEnum.MainMenuScene;
    public GameState gameState = GameState.Play;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(loadingScreen);
        } else {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        HandleSceneInput();
    }

    public void SwitchScene(SceneEnum scene, bool withLoadingScreen = false)
    {
        currentScene = scene;
        if (withLoadingScreen) {
            StartCoroutine(LoadSceneWithLoadingScreen(scene));
        } else {
            SceneManager.LoadScene(scene.ToString());
        }
    }

    private IEnumerator LoadSceneWithLoadingScreen(SceneEnum scene)
    {
        Time.timeScale = 0;
        gameState = GameState.Pause;
        loadingScreen.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.ToString());
        yield return new WaitForSecondsRealtime(loadingScreenDuration);

        while (!asyncLoad.isDone) {
            yield return null;
        }

        loadingScreen.SetActive(false);
        Time.timeScale = 1;
        gameState = GameState.Play;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGameToggle()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") return;

        if (isPaused) {
            UnpauseGame();
        } else {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameUIManager.Instance.TurnOnSettings();
        isPaused = true;
        gameState = GameState.Pause;
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        GameUIManager.Instance.TurnOffSettings();
        isPaused = false;
        gameState = GameState.Play;
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HandleSceneInput()
    {
        if (Input.GetKeyDown(restartKey)) {
            RestartScene();
        }

        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(KeyCode.Escape)) {
            PauseGameToggle();
        }
    }

    public bool LoadingScreenIsNotActive()
    {
        return !loadingScreen.activeSelf;
    }
}

public enum SceneEnum
{
    MainMenuScene,
    GameScene
}

public enum GameState
{
    Play,
    Pause
}
