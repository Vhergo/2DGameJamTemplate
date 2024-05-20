using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveDataHandler
{
    // When combining paths, don't use directory separators ("/")
    // Save to Json file
    public static void SaveDataToJson(IEnumerable<ISaveable> allSaveableData, string saveName) 
    {
        // Iterate through all savable data (everything inherting from Isaveable)
        // Convert all data to Json format
        // Write json string to file
        string fullPath = Path.Combine(Application.dataPath, saveName);

        SaveData saveData = new SaveData();
        foreach (ISaveable saveable in allSaveableData) {
            saveable.PopulateSaveData(saveData);
        }

        // Write to file
        string jsonSaveString = JsonUtility.ToJson(saveData);
        if (FileHandler.WriteToFile(saveName, jsonSaveString)) {
            Debug.Log($"Successfully Saved Data at {fullPath}!");
        }
    }

    // Load from Json file
    // If file does not exist (main save or backup), return an empty instance of the saveData class 
    public static void LoadFromJson(IEnumerable<ISaveable> allSaveableData, string saveName)
    {
        var fullPath = Path.Combine(Application.persistentDataPath, saveName);

        // If file exists, load from file
        // If file doesn't exist, start new save instead
        SaveData saveData = new SaveData();
        if (FileHandler.ReadFromFile(saveName, out string loadedContent)) {
            JsonUtility.FromJsonOverwrite(loadedContent, saveData);
            // Debug.Log("Loaded Content: " + loadedContent);

            Debug.Log($"Successfully Loaded Data from {fullPath}!");
        }else {
            Debug.LogError("Cannon load saved data. Loading default values!");
        }

        // Apply loaded data to all relavent objects
        foreach (ISaveable saveable in allSaveableData) {
            saveable.LoadFromSaveData(saveData);
        }

    }
}
