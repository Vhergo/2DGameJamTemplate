using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grid2DPlacerData
{
    public List<PrefabGroupSave> prefabGroups;
    public int selectedIndex;
    public int prefabLoadIndex;
    public string tilePrefabName;
    public string gridOriginHierarchyPath;
    public float gridTileSize;
    public Vector2Int gridSize;
    public bool canPlace;
    public bool showGrid;
    public bool allowDrag;
    public bool showPrefabNameFieldCreate;
    public bool showPrefabNameFieldLoad;
}

[System.Serializable]
public class PrefabGroupSave
{
    public string name;
    public string prefabName;
    public string parentHierarchyPath;
}
