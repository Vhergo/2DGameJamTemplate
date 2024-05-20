using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private string saveName = "saveData.json";

    public void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load on start();
        // Make sure all relevant objects are loaded beforehand
        LoadGameSave();
    }

    private void Update()
    {
        // Key to test save/load
        if (Input.GetKeyDown(KeyCode.Alpha1)) SaveGame();
        if (Input.GetKeyDown(KeyCode.Alpha2)) LoadGameSave();
    }

    public void SaveGame()
    {
        List<ISaveable> saveables = new List<ISaveable>();
        // find all MonoBehaviors that also inherit from Isaveable
        // add all those objects to the list
        saveables.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>());

        SaveDataHandler.SaveDataToJson(saveables, saveName);
    }

    public void LoadGameSave()
    {
        List<ISaveable> saveables = new List<ISaveable>();

        saveables.AddRange(FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>());

        SaveDataHandler.LoadFromJson(saveables, saveName);
    }
}
