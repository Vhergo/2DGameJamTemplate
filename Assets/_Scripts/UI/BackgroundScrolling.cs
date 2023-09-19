using UnityEngine;
using UnityEngine.UI;

public class BackgroundScrolling : MonoBehaviour
{
    [SerializeField] private RawImage image;
    [SerializeField] private float scrollX;
    [SerializeField] private float scrollY;
    [SerializeField] private bool stopScroll;

    private void Update() {
        if (!stopScroll) {
            Vector2 scrollValue = new Vector2(scrollX, scrollY);
            image.uvRect = new Rect(
                image.uvRect.position + scrollValue * Time.deltaTime,
                image.uvRect.size);
        }
    }

    public void ToggleScrolling() {
        stopScroll = !stopScroll;
    }
}
