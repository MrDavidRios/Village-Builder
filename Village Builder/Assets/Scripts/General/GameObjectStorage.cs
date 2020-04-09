using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectStorage : MonoBehaviour
{
    //Non-Static (can't be accessed by other scripts without a stored instance, but these variables can be set in the inspector
    [LabelOverride("Selection Cursor")]
    public GameObject nonStaticSelectionCursor;

    [LabelOverride("Tile Parent")]
    public GameObject nonStaticTileParent;

    [LabelOverride("Main Camera")]
    public Camera nonStaticMainCamera;

    //Static (can be accessed by other scripts without a stored instance)
    public static GameObject selectionCursor;
    public static GameObject tileParent;

    public static Camera mainCamera;

    private void Awake()
    {
        //Set the static variables equal to those set in the inspector
        selectionCursor = nonStaticSelectionCursor;
        tileParent = nonStaticTileParent;
        mainCamera = nonStaticMainCamera;
    }
}
