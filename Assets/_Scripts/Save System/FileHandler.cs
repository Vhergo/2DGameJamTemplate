using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileHandler
{
    //Write to file
    public static bool WriteToFile(string saveFileName, string fileContents)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, saveFileName);

        try {
            File.WriteAllText(fullPath, fileContents);
            return true;
            // Succsessfully wrote save data to file
        }
        catch (Exception e) {
            Debug.LogError($"Failed to write to file at {fullPath} with exception {e}");
            return false;
            // Failed to write save data to file
        }
    }

    //Read from file
    public static bool ReadFromFile(string saveFileName, out string loadedContent)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, saveFileName);

        try {
            loadedContent = File.ReadAllText(fullPath);
            return true;
            // saved data loaded correctly
        }
        catch (FileNotFoundException) {
            Debug.LogError($"File not found at {fullPath}");
            loadedContent = null;
            return false;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to load save data from file at {fullPath} with exception {e}");
            loadedContent = null; // nothing to output
            return false;
            // unable to load saved data
        }
    }
}
