using UnityEngine;
using UnityEngine.UI;

public class LoadingCircleRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    private void Update() {
        transform.Rotate(0, 0, -rotationSpeed * 10 * Time.unscaledDeltaTime);
    }
}
