using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    #region VARIABLES
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;

    [Header("SOUNDS")]
    [Header("Background Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip inGameMusic;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.8f, 1.2f);

    private Slider masterSlider;
    private Slider musicSlider;
    private Slider effectsSlider;

    private GameObject settings;
    private GameObject sliders;
    private GameObject toggles;

    private Button toggleMusicButton;
    private Button toggleEffectsButton;

    private float masterSliderValue;
    private float musicSliderValue;
    private float effectsSliderValue;
    #endregion

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoadMusicUpdate;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initial Load Setup
        GetUIObjectReferences(); // Needs to be called before anything else;

        SetDefaultVolumeLevels();
        SubscribeToListeners();
        SubscribeAudioToggleButtons();
        TurnOffSettings();

        SceneManager.sceneLoaded += HandleLoadAudioUILogic;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadMusicUpdate;
        SceneManager.sceneLoaded -= HandleLoadAudioUILogic;
    }

    #region AUDIO HANDLING
    public void PlaySound(AudioClip clip, bool variablePitch = false)
    {
        if (clip == null) return;

        effectsSource.pitch = variablePitch ? Random.Range(pitchRange.x, pitchRange.y) : 1f;
        effectsSource.PlayOneShot(clip);
    }

    private void ChangeMasterVolume(float value)
    {
        AudioListener.volume = value;
        masterSliderValue = value;
    }

    private void ChangeMusicVolume(float value)
    {
        musicSource.volume = value;
        musicSliderValue = value;
    }

    private void ChangeEffectsVolume(float value)
    {
        effectsSource.volume = value;
        effectsSliderValue = value;
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleEffects()
    {
        effectsSource.mute = !effectsSource.mute;
    }

    // Waits until the loading screen is finished before playing music
    public IEnumerator SetMusic(AudioClip music)
    {
        musicSource.clip = music;

        yield return new WaitUntil(MySceneManager.Instance.LoadingScreenIsNotActive);
        musicSource.Play();
    }

    // Method to change the music based off of the current scene
    // Simple switch statement can be expanded to handle more scenes
    private void OnSceneLoadMusicUpdate(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name) {
            case nameof(SceneEnum.MainMenuScene):
                StartCoroutine(SetMusic(mainMenuMusic));
                break;
            case nameof(SceneEnum.GameScene):
                StartCoroutine(SetMusic(inGameMusic));
                break;
        }
    }
    #endregion

    #region UI HANDLING
    // Slider UI

    // Subscribed to the 'SceneManager.sceneLoaded' built-in Unity event;
    // Invoked on any scene change after the intial load
    private void HandleLoadAudioUILogic(Scene scene, LoadSceneMode mode)
    {
        GetUIObjectReferences();

        SubscribeToListeners();

        SetSliderUIValues();
        SubscribeAudioToggleButtons();
        TurnOffSettings();
    }

    // Subscribed to the 'SceneManager.sceneLoaded' built-in Unity event;
    // Get a reference to the current scene's needed UI elements
    private void GetUIObjectReferences()
    {
        sliders = GameObject.FindGameObjectWithTag("Sliders");
        if (sliders != null) {
            masterSlider = sliders.transform.GetChild(0).GetComponent<Slider>();
            musicSlider = sliders.transform.GetChild(1).GetComponent<Slider>();
            effectsSlider = sliders.transform.GetChild(2).GetComponent<Slider>();
        } else {
            LogError("Sliders not properly referenced!");
        }

        settings = GameObject.FindGameObjectWithTag("Settings");
        if (settings == null)
            LogError("Settings not properly referenced!");

        toggles = GameObject.FindGameObjectWithTag("Audio Toggles");
        if (toggles != null) {
            toggleMusicButton = toggles.transform.GetChild(0).GetComponent<Button>();
            toggleEffectsButton = toggles.transform.GetChild(1).GetComponent<Button>();
        } else {
            LogError("Toggles not properly referenced!");
        }
    }

    // Set the default volume levels on scene load
    private void SetDefaultVolumeLevels()
    {
        ChangeMasterVolume(masterSlider.value);
        ChangeMusicVolume(musicSlider.value);
        ChangeEffectsVolume(effectsSlider.value);
    }

    // Update audio levels based of slider values changes
    private void SubscribeToListeners()
    {
        masterSlider.onValueChanged.AddListener(val => ChangeMasterVolume(val));
        musicSlider.onValueChanged.AddListener(val => ChangeMusicVolume(val));
        effectsSlider.onValueChanged.AddListener(val => ChangeEffectsVolume(val));
    }

    // Update UI Sliders with the persistent volume data
    // Used to update a new scenes slider values to match the volume data that's carried over
    private void SetSliderUIValues()
    {
        if (sliders != null) {
            sliders.transform.GetChild(0).GetComponent<Slider>().value = masterSliderValue;
            sliders.transform.GetChild(1).GetComponent<Slider>().value = musicSliderValue;
            sliders.transform.GetChild(2).GetComponent<Slider>().value = effectsSliderValue;
        } else {
            LogError("Sliders not properly referenced!");
        }
    }

    // Reference and subscribe audio toggle buttons to proper listeners
    private void SubscribeAudioToggleButtons()
    {
        toggleMusicButton.onClick.AddListener(ToggleMusic);
        toggleEffectsButton.onClick.AddListener(ToggleEffects);
    }

    // Turn off the settings panel
    // Used to turn off settings panel AFTER all references have been properly aquired
    private void TurnOffSettings()
    {
        settings.SetActive(false);
    }
    #endregion

    #region HELPER FUNCTIONS
    private void LogError(string message)
    {
        Debug.LogError(message);
    }
    #endregion
}
