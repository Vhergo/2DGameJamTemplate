using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFollow : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;  // Get mouse position
        mousePos.z = 10f;  // Distance from the camera to the object
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);  // Convert to world position
        worldPosition.z = 0;  // Force z-index to stay at 0, if needed

        transform.position = worldPosition;  // Set the position of the GameObject to the cursor position
    }
}