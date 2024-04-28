using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;
using System.IO;

public class Grid2DPlacerEditor : EditorWindow
{
    #region VARIABLES
    public List<PrefabGroup> prefabGroups = new List<PrefabGroup>();
    public int selectedIndex = 0;
    public int prefabLoadIndex = 0;
    public GameObject tilePrefab;
    public Transform gridOrigin;
    public float gridTileSize = 1f;
    public Vector2Int gridSize = new Vector2Int(10, 10);
    public bool canPlace = false;
    public bool showGrid = true;
    public bool allowDrag = true;
    public bool showPrefabNameFieldCreate = false;
    public bool showPrefabNameFieldLoad = false;

    private Vector3 lastPlacedTilePosition = Vector3.positiveInfinity;
    private Vector2 scrollPosition;
    private Vector2 prefabGroupScrollPosition;
    private string prefabName;
    private GameObject selectedPrefab;
    private GameObject loadedPrefab;
    private bool updateHeader = true;

    private GUIStyle headerStyle;
    private GUIStyle horizontalScrollbarStyle;
    private GUIStyle verticalScrollbarNone;
    #endregion


    [MenuItem("Tools/2D Grid Placer")]
    public static void ShowWindow()
    {
        GetWindow<Grid2DPlacerEditor>("2D Grid Placer");
    }

    private void OnFocus()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        updateHeader = true;
    }

    #region GUI FUNCTIONALITY
    void OnGUI()
    {
        InitializeHeaderStyle();
        InitializeScrollbarStyles();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        Undo.RecordObject(this, "Grid Settings Change");
        EditorGUI.BeginChangeCheck();

        #region GROUP SETTINGS
        EditorGUILayout.BeginVertical("Box");
        // Add and remove Prefab Groups
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Prefab Group", GUILayout.ExpandWidth(true))) {
            PrefabGroup newPrefabGroup = new PrefabGroup { name = "New Group", prefab = null, parent = null };
            prefabGroups.Add(newPrefabGroup);
            selectedIndex = prefabGroups.IndexOf(newPrefabGroup);
            tilePrefab = prefabGroups[selectedIndex].prefab;
            gridOrigin = prefabGroups[selectedIndex].parent;
        }

        if (GUILayout.Button("Delete Selected Group", GUILayout.ExpandWidth(true)) && prefabGroups.Count > 0 && selectedIndex >= 0) {
            prefabGroups.RemoveAt(selectedIndex);
            selectedIndex = selectedIndex - 1;
            if (selectedIndex < 0) selectedIndex = 0; // Ensure the index is not negative
            if (prefabGroups.Count > 0) {
                tilePrefab = prefabGroups[selectedIndex].prefab;
                gridOrigin = prefabGroups[selectedIndex].parent;
            }
        }
        EditorGUILayout.EndHorizontal();


        int buttonsPerRow = Mathf.Max(1, (int)(EditorGUIUtility.currentViewWidth / 110));  // assuming each button is about 110 pixels wide
        EditorGUILayout.LabelField("Prefab Groups", headerStyle);
        int count = 0;
        GUILayout.BeginHorizontal();
        foreach (var group in prefabGroups) {
            GUI.backgroundColor = (selectedIndex == prefabGroups.IndexOf(group)) ? Color.cyan : Color.white;
            if (GUILayout.Button(group.name, GUILayout.Width(100), GUILayout.Height(25))) {
                selectedIndex = prefabGroups.IndexOf(group);
                tilePrefab = group.prefab;
                gridOrigin = group.parent;
            }
            if (++count % buttonsPerRow == 0) {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        #endregion

        #region GRID SETTINGS
        DrawCustomHeader("Grid Settings");

        GUILayout.BeginVertical("Box");
        if (selectedIndex >= 0 && selectedIndex < prefabGroups.Count) {
            PrefabGroup selectedGroup = prefabGroups[selectedIndex];
            selectedGroup.name = EditorGUILayout.TextField("Group Name", selectedGroup.name);
            selectedGroup.prefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", selectedGroup.prefab, typeof(GameObject), false);
            selectedGroup.parent = (Transform)EditorGUILayout.ObjectField("Grid Origin", selectedGroup.parent, typeof(Transform), true);
        }

        gridTileSize = EditorGUILayout.FloatField("Grid Tile Size", gridTileSize);
        gridSize = EditorGUILayout.Vector2IntField("Grid Size", gridSize);
        GUILayout.EndVertical();
        #endregion

        #region OTHER SETTINGS
        DrawCustomHeader("Function Toggles");
        ShowToggleButtons();

        ShowPrefabSaveButtons();
        #endregion

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(this, "Grid Settings Change");
            EditorUtility.SetDirty(this);
        }

        EditorGUILayout.EndScrollView();

        InitializeHeaderStyle();
    }

    private void ShowPrefabSaveButtons()
    {
        DrawCustomHeader("Prefab Manager");
        GUILayout.BeginVertical("Box");
        #region CREATE PREFAB

        GUI.backgroundColor = Color.cyan;
        if (!showPrefabNameFieldCreate) {
            if (GUILayout.Button("Create Grid Prefab")) { // Create Grid Prefab Button
                showPrefabNameFieldCreate = true;  // Show the text field
                prefabName = "New Level Prefab"; // Set default name to "NewLevelPrefab
            }
        } else {
            EditorGUILayout.BeginHorizontal("box");
            // prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
            EditorGUILayout.LabelField("Prefab Name", GUILayout.Width(80));
            prefabName = EditorGUILayout.TextField(prefabName, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Confirm")) {
                if (gridOrigin.childCount > 0 && CheckIfValidNameWasInput(prefabName)) {
                    CreateGridPrefab(prefabName);
                    showPrefabNameFieldCreate = false; // Hide the text field again after creation
                } else {
                    Debug.LogError("No grid tiles to save to prefab.");
                }
            }
            if (GUILayout.Button("X")) {
                showPrefabNameFieldCreate = false;  // Hide the text field again without creation
            }
            EditorGUILayout.EndHorizontal();
        }
        GUI.backgroundColor = Color.white;
        #endregion

        #region LOAD PREFAB
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Load Grid Prefab")) {
            LoadGridPrefab();
        }
        selectedPrefab = EditorGUILayout.ObjectField(selectedPrefab, typeof(GameObject), false) as GameObject;
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        #endregion
        GUILayout.EndVertical();

        DrawCustomHeader("Save Manager");
        GUILayout.BeginVertical("Box");
        #region SAVE/LOAD GRID DATA TO/FROM FILE

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Save Grid To File")) {
            SaveGrid();
        }
        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Load Grid From File")) {
            LoadTiles();
        }
        GUI.backgroundColor = Color.white;
        #endregion
        GUILayout.EndVertical();
    }

    private void ShowToggleButtons()
    {
        GUILayout.BeginVertical("Box");
        GUI.backgroundColor = showGrid ? Color.cyan : Color.red;
        if (GUILayout.Button("Toggle Grid Visibility")) {
            showGrid = !showGrid;
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = canPlace ? Color.cyan : new Color(235, 0, 0, 0.35f); // light red rgba(235, 150, 100, 0.5)
        if (GUILayout.Button("Toggle Placement Mode")) {
            canPlace = !canPlace;
        }
        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = allowDrag ? Color.cyan : Color.red;
        if (GUILayout.Button("Toggle Dragging")) {
            allowDrag = !allowDrag;
        }
        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Clear All Tiles")) {
            ClearAllTiles();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();
    }
    #endregion

    #region MAIN SCENE GUI FUNCTIONALITY
    void OnSceneGUI(SceneView sceneView)
    {
        DrawGrid();

        Event e = Event.current;
        Vector2 mousePos = GetMousePosition(e, sceneView);
        Vector3 worldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
        Vector3 alignedPosition = AlignToGrid(worldPos);

        // Handle tile placement or removal based on mouse action
        if (canPlace && ((e.type == EventType.MouseDrag && allowDrag) || e.type == EventType.MouseDown)) {
            if (e.button == 0) {  // Left-click for placing tiles
                if (lastPlacedTilePosition != alignedPosition) {  // Check if it's a new cell
                    PlaceTileIfEmpty(alignedPosition);
                    lastPlacedTilePosition = alignedPosition;  // Update last placed position
                }
            } else if (e.button == 1) {  // Right-click for removing tiles
                if (lastPlacedTilePosition != alignedPosition || e.type == EventType.MouseDown) {
                    RemoveTileIfPresent(alignedPosition);
                    lastPlacedTilePosition = alignedPosition;  // Update last placed position
                }
            }
            e.Use();  // Consume the event to prevent other interactions
        }

        if (e.type == EventType.MouseUp) {
            lastPlacedTilePosition = Vector3.positiveInfinity;  // Reset on mouse release
        }
    }


    private void PlaceTileIfEmpty(Vector3 alignedPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(alignedPosition, Vector2.zero, 0.1f);
        if (hit.collider == null) {  // Only place a tile if there is no collider hit
            GameObject tile = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;
            Undo.RegisterCreatedObjectUndo(tile, "Place Tile");
            tile.transform.position = alignedPosition;
            tile.transform.localScale = new Vector3(gridTileSize, gridTileSize, 1);  // Scale the tile
            tile.transform.parent = gridOrigin;
        }
    }

    private void RemoveTileIfPresent(Vector3 alignedPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(alignedPosition, Vector2.zero, 0.1f);
        if (hit.collider != null) {  // Check if there is a tile to remove
            GameObject tile = hit.collider.gameObject;
            Undo.DestroyObjectImmediate(tile);  // Register undo for tile removal
        }
    }

    // Clear all placed tiles under the currently designated parent transform
    private void ClearAllTiles()
    {
        while (gridOrigin.childCount > 0) {
            Undo.DestroyObjectImmediate(gridOrigin.GetChild(0).gameObject);
        }
    }

    private Vector3 AlignToGrid(Vector3 position)
    {
        // Calculate grid position based on the tile size
        float x = Mathf.Floor(position.x / gridTileSize) * gridTileSize + gridTileSize / 2;
        float y = Mathf.Floor(position.y / gridTileSize) * gridTileSize + gridTileSize / 2;
        return new Vector3(x, y, 0);
    }



    private Vector2 GetMousePosition(Event e, SceneView sceneView)
    {
        Vector2 mousePos = new Vector2(e.mousePosition.x, e.mousePosition.y);
        mousePos.y = sceneView.camera.pixelHeight - mousePos.y * EditorGUIUtility.pixelsPerPoint;
        mousePos.x *= EditorGUIUtility.pixelsPerPoint;
        return mousePos;
    }

    private void DrawGrid()
    {
        if (!showGrid) return;
        Vector3 gridPos = (gridOrigin != null) ? gridOrigin.position : Vector3.zero;

        Handles.color = Color.Lerp(Color.black, Color.cyan, 0.75f);
        Vector3 startPosition = gridPos - new Vector3(gridTileSize * gridSize.x * 0.5f, gridTileSize * gridSize.y * 0.5f, 0);

        for (int x = 0; x <= gridSize.x; x++) {
            Vector3 startLine = startPosition + new Vector3(x * gridTileSize, 0, 0);
            Vector3 endLine = startLine + new Vector3(0, gridSize.y * gridTileSize, 0);
            Handles.DrawLine(startLine, endLine);
        }

        for (int y = 0; y <= gridSize.y; y++) {
            Vector3 startLine = startPosition + new Vector3(0, y * gridTileSize, 0);
            Vector3 endLine = startLine + new Vector3(gridSize.x * gridTileSize, 0, 0);
            Handles.DrawLine(startLine, endLine);
        }
    }
    #endregion

    #region HELPER FUNCTIONS
    private void SimpleDivider(float height = 2f)
    {
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(height) });
    }

    private bool CheckIfValidNameWasInput(string prefabName)
    {
        if (prefabName.Length > 0) {
            return true;
        } else {
            Debug.LogError("No prefab name was input.");
            return false;
        }
    }


    #endregion

    #region PREFAB HANDLING FUNCTIONALITY
    private void CreateGridPrefab(string prefabName)
    {
        GameObject prefabRoot = new GameObject(prefabName);
        foreach (Transform child in gridOrigin) {
            // Ensure the child is a prefab instance by checking if it has a prefab link
            if (PrefabUtility.GetPrefabInstanceStatus(child.gameObject) == PrefabInstanceStatus.Connected) {
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) as GameObject;
                GameObject clone = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
                clone.transform.position = child.position;
                clone.transform.rotation = child.rotation;
                clone.transform.localScale = child.localScale;
                clone.transform.SetParent(prefabRoot.transform, false);
            } else {
                // If it's not a prefab, instantiate it normally
                GameObject clone = Instantiate(child.gameObject);
                clone.transform.SetParent(prefabRoot.transform, false);
            }
        }

        string localPath = Path.Combine("Assets", "Prefabs", "Resources", "Levels", prefabName + ".prefab");
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        Debug.Log("Generated Path: " + localPath);

        if (!string.IsNullOrEmpty(localPath)) {
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, localPath);
            Debug.Log("Prefab Created: " + localPath);
        } else {
            Debug.LogError("Failed to create valid path for prefab.");
        }

        DestroyImmediate(prefabRoot);
    }


    private void LoadGridPrefab()
    {
        ClearAllTiles(); // Clear existing tiles before loading

        if (selectedPrefab != null) {
            foreach (Transform child in selectedPrefab.transform) {
                // Get the prefab asset linked to the child GameObject
                GameObject originalTilePrefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) as GameObject;
                if (originalTilePrefab != null) {
                    // Instantiate a new instance of the prefab type
                    GameObject loadedTile = PrefabUtility.InstantiatePrefab(originalTilePrefab) as GameObject;
                    if (loadedTile != null) {
                        loadedTile.transform.SetParent(gridOrigin, false); // Reparent
                        loadedTile.transform.localPosition = child.localPosition; // Adjust local transform settings
                        loadedTile.transform.localRotation = child.localRotation;
                        loadedTile.transform.localScale = child.localScale;
                    } else {
                        Debug.LogError("Failed to instantiate a tile prefab of type: " + child.name);
                    }
                } else {
                    Debug.LogWarning("The original tile prefab was not found for: " + child.name);
                }
            }
        } else {
            Debug.LogError("No grid prefab was selected");
        }
    }



    #endregion

    #region SAVE AND LOAD FUNCTIONALITY
    // SAVE AND LOAD FUNCTIONALITY
    private void SaveGrid()
    {
        string path = EditorUtility.SaveFilePanel("Save Grid Data", "", "GridSaveData", "json");
        if (!string.IsNullOrEmpty(path)) {
            GridSaveData saveData = new GridSaveData();
            foreach (Transform child in gridOrigin) {
                TileData tileData = new TileData {
                    prefabName = child.name,
                    position = child.position,
                    parentPath = GetHierarchyPath(child.parent)
                };
                saveData.tiles.Add(tileData);
            }
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh(); // Ensure Unity recognizes the new file
            Debug.Log("Grid data saved to: " + path);
        }
    }


    private void LoadTiles()
    {
        string path = EditorUtility.OpenFilePanel("Load Grid Data", "", "json");
        if (System.IO.File.Exists(path)) {
            string json = System.IO.File.ReadAllText(path);
            GridSaveData saveData = JsonUtility.FromJson<GridSaveData>(json);

            ClearAllTiles(); // Clear existing tiles before loading

            foreach (TileData tileData in saveData.tiles) {
                GameObject prefab = Resources.Load<GameObject>(tileData.prefabName); // Make sure the prefab exists in a Resources folder
                if (prefab) {
                    Debug.Log("Found Prefab");
                    GameObject tile = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    if (tile) {
                        Debug.Log("Instantiated Prefab");
                        tile.transform.position = tileData.position;
                        tile.transform.localScale = new Vector3(gridTileSize, gridTileSize, 1);
                        Transform parentTransform = FindTransformByPath(tileData.parentPath);
                        if (parentTransform != null)
                            tile.transform.parent = parentTransform;
                        Debug.Log("Loaded Tile");
                    }
                }
            }

            Debug.Log("Loaded Tiles from " + path);
        } else {
            Debug.LogError("No save file found at " + path);
        }
    }

    private string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null) {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    private Transform FindTransformByPath(string path)
    {
        GameObject obj = GameObject.Find(path);
        return obj != null ? obj.transform : null;
    }

    #endregion

    #region CUSTOM STYLING
    private void InitializeHeaderStyle()
    {
        if (headerStyle == null || updateHeader) {
            // Create a new GUIStyle
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16; // Set the font size
            headerStyle.fontStyle = FontStyle.Bold; // Make the text bold
            headerStyle.normal.textColor = Color.white; // Set the text color
            headerStyle.alignment = TextAnchor.MiddleCenter; // Center the text

            // Set a background texture color
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0.1f, 0.15f, 0.25f));

            backgroundTexture.Apply();

            headerStyle.normal.background = backgroundTexture;
            updateHeader = false;
        }
    }

    private void DrawCustomHeader(string headerTitle, int headerSpacing = 10)
    {
        GUILayout.Space(headerSpacing);
        GUILayout.Label(headerTitle, headerStyle, GUILayout.ExpandWidth(true));
    }

    private void InitializeScrollbarStyles()
    {
        if (verticalScrollbarNone == null) {
            verticalScrollbarNone = new GUIStyle(GUI.skin.verticalScrollbar);
            verticalScrollbarNone.normal.background = null;  // No background
            verticalScrollbarNone.fixedWidth = 0;            // Zero width
        }
    }

    #endregion
}

#region OTHER CLASSES
[Serializable]
public class PrefabGroup
{
    public string name;
    public GameObject prefab;
    public Transform parent;
}

[Serializable]
public class TileData
{
    public string prefabName;
    public Vector3 position;
    public string parentPath;
}

[Serializable]
public class GridSaveData
{
    public List<TileData> tiles = new List<TileData>();
}
#endregion