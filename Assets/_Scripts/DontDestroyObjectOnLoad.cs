using UnityEngine;

public class DontDestroyObjectOnLoad : MonoBehaviour
{
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
