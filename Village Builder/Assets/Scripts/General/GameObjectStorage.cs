﻿using UnityEngine;

public class GameObjectStorage : MonoBehaviour
{
    //Static (can be accessed by other scripts without a stored instance)
    public static GameObject SelectionCursor;
    public static GameObject TileParent;

    public static Camera MainCamera;

    //Non-Static (can't be accessed by other scripts without a stored instance, but these variables can be set in the inspector
    [LabelOverride("Selection Cursor")] public GameObject nonStaticSelectionCursor;

    [LabelOverride("Tile Parent")] public GameObject nonStaticTileParent;

    [LabelOverride("Main Camera")] public Camera nonStaticMainCamera;

    private void Awake()
    {
        //Set the static variables equal to those set in the inspector
        SelectionCursor = nonStaticSelectionCursor;
        TileParent = nonStaticTileParent;
        MainCamera = nonStaticMainCamera;
    }
}