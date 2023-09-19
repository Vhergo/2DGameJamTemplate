using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildVersionEditor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    private const string initialVerstion = "0.0";

    public void OnPreprocessBuild(BuildReport report) {
        string currentVersion = FindCurrentVersion();
        UpdateVersion(currentVersion);
    }

    private string FindCurrentVersion() {
        string[] currentVersion = PlayerSettings.bundleVersion.Split('[', ']');

        return currentVersion.Length == 1 ? initialVerstion : currentVersion[1];
    }

    private void UpdateVersion(string version) {
        if (float.TryParse(version, out float versionNumber)) {
            float newVersion = versionNumber + 0.01f;
            string date = DateTime.Now.ToString("d");

            PlayerSettings.bundleVersion = string.Format("Version [{0}] - {1}}", newVersion, date);
            UnityEngine.Debug.Log(PlayerSettings.bundleVersion);
        }
    }
}
