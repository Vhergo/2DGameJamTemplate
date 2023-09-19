using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersionDisplay : MonoBehaviour
{
    private void Awake() {
        if (TryGetComponent(out TMP_Text output)) {
            output.text = Application.version;
        }
    }
}
