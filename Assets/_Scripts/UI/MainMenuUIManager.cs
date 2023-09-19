using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance;

    private void Awake() {
        if(Instance == null) {
            Instance = this;
        }else {
            Destroy(gameObject);
        }
    }

    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject settingsPanel;

    private void Start() {
        playButton.onClick.AddListener(OnPlayButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);
        settingsButton.onClick.AddListener(OnSettingsButtonClick);
    }

    private void OnPlayButtonClick() {
        MySceneManager.Instance.SwitchScene(SceneEnum.GameScene, true);
    }

    private void OnQuitButtonClick() {
        MySceneManager.Instance.QuitGame();
    }

    public void OnSettingsButtonClick() {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void CloseSettingsPanel() {
        settingsPanel.SetActive(false);
    }

}
