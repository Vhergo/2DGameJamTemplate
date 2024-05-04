using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    public Texture2D cursorDefault;
    public Texture2D cursorEnemy;
    private Vector2 cursorHotspot;
    private bool isOverEnemy;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            cursorHotspot = new Vector2(cursorDefault.width / 2, cursorDefault.height / 2);
            Cursor.SetCursor(cursorDefault, cursorHotspot, CursorMode.Auto);
        } else {
            Destroy(gameObject);
        }
    }

    public void OnPointerExitUI()
    {
        SetCursor(cursorDefault);
    }

    public void OnPointerOverEnemy(bool onEnemy)
    {
        isOverEnemy = onEnemy ? true : false;
        SetCursor(onEnemy ? cursorEnemy : cursorDefault);
    }

    private void SetCursor(Texture2D cursorTexture)
    {
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }
}