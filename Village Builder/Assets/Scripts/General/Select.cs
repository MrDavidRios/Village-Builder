using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerrainGeneration;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TileOperations
{
    public class Select : MonoBehaviour
    {
        //Booleans
        public bool anythingSelected;

        public static bool canSelect;

        public static bool selectionLocked;

        //Integers
        private Vector2Int tileIndex;

        private int _villagerRole;

        //Floats
        public float selectionCursorHeight;

        private Vector2 defaultSelectionCursorSideDistance;

        //GameObjects
        public static GameObject selectedObject;

        public GameObject selectionCursor;

        public GameObject terrainMesh;

        //LayerMask
        public LayerMask selectable;

        //Raycasting
        private RaycastHit hit;
        private Ray ray;

        public Vector2 selectionPos;

        public Camera mainCamera;

        //Scripts
        private TerrainData terrainData;
        private TerrainGenerator terrainGenerator;
        private PlacementGrid placementGrid;
        private static UIManager UIManagerScript;
        private SelectionCursor selectionCursorScript;
        private JobManager jobManager;
        private DisplayJobsList displayJobsList;

        //Building Info
        private Building buildingInfo;

        private void Start() => Initialize();

        private void Initialize()
        {
            //Cache necessary scripts
            terrainGenerator = FindObjectOfType<TerrainGenerator>();
            terrainData = FindObjectOfType<TerrainData>();
            placementGrid = FindObjectOfType<PlacementGrid>();
            UIManagerScript = FindObjectOfType<UIManager>();
            selectionCursorScript = FindObjectOfType<SelectionCursor>();
            jobManager = FindObjectOfType<JobManager>();
            displayJobsList = FindObjectOfType<DisplayJobsList>();

            selectionCursor = selectionCursorScript.gameObject;

            defaultSelectionCursorSideDistance = new Vector2(selectionCursorScript.sideDist, selectionCursorScript.sideDist * 1.25f);

            selectionCursor.SetActive(false);

            //Initialize Terrain Collider
            var terrainCollider = terrainMesh.GetComponent<BoxCollider>();
            terrainCollider.center = new Vector3(terrainGenerator.worldSize / 2, -0.75f, terrainGenerator.worldSize / 2);
            terrainCollider.size = new Vector3(terrainGenerator.worldSize, 1.5f, terrainGenerator.worldSize);

            //Set necessary variables
            canSelect = true;
        }

        private void Update()
        {
            if (!canSelect)
                return;

            //The ray goes from the camera's position on the screen to the mouse cursor's position
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            //This code won't run if the mouse is hovering over the UI or if a building is being placed
            if (!EventSystem.current.IsPointerOverGameObject() && !PlaceBuilding._placingBuilding && !selectionLocked)
            {
                //If the raycast hit something with a collider, run the code inside this 'if' statement
                if (Physics.Raycast(ray, out hit, GlobalRules._maxRaycastDistance, selectable))
                {
                    //Left-Click
                    if (Input.GetMouseButtonDown(0))
                    {
                        RestoreSelectionCursorSideDistanceToDefault();

                        CloseOpenedTempUI();

                        //Set the selected object equal to the object that was clicked on
                        selectedObject = hit.transform.gameObject;

                        //Check what kind of object was selected based on its layer
                        switch (selectedObject.layer)
                        {
                            //Terrain Layer (Tiles)
                            case 8:
                                {
                                    var tilePos = placementGrid.GetNearestPointOnGrid(hit.point);

                                    //Find the x and y indexes of the tile
                                    tileIndex = GetTileIndex(tilePos);

                                    //Change the scale of the selection cursor
                                    ScaleSelectionCursor(new Vector3(0.5f, 0.5f, 0.5f));

                                    //Is this tile a water tile?
                                    if (Environment.tileType[tileIndex.x, tileIndex.y] == "Water")
                                    {
                                        //If the tile is a water tile, move the cursor down 0.4 units because water tiles are lower than land tiles.
                                        PositionSelectionCursor(tilePos + new Vector3(0f, -0.4f, 0f), tileIndex);
                                    }
                                    else
                                        PositionSelectionCursor(tilePos, tileIndex);

                                    selectedObject = terrainGenerator.gameObject;
                                    break;
                                }

                            //Resource Layer (Trees, rocks, etc.)
                            case 9:
                                {
                                    //Differentiate between different resource types
                                    switch (selectedObject.tag)
                                    {
                                        case "Tree":
                                            ScaleSelectionCursor(new Vector3(0.75f, 0.75f, 0.75f));
                                            break;
                                        case "Stone":
                                            ScaleSelectionCursor(selectedObject.transform.localScale);
                                            break;
                                        case "Iron":
                                            break;
                                        case "Bronze":
                                            break;
                                        case "Gold":
                                            break;
                                        default:
                                            Debug.Log("Invalid tag specified: " + selectedObject.tag);
                                            break;
                                    }

                                    PositionSelectionCursor(selectedObject.transform.position, true);
                                    break;
                                }

                            //Buildings Layer
                            case 10:
                                {
                                    PositionSelectionCursorCustom(selectedObject.transform.position, true);

                                    switch (selectedObject.tag)
                                    {
                                        case "Infrastructure":
                                            buildingInfo = selectedObject.GetComponent<InfrastructureBuilding>().building;
                                            break;
                                        case "Residential":
                                            buildingInfo = selectedObject.GetComponent<ResidentialBuilding>().building;
                                            break;
                                        case "Industrial":
                                            buildingInfo = selectedObject.GetComponent<IndustrialBuilding>().building;
                                            break;
                                        default:
                                            Debug.LogWarning("Building Tag not found: " + tag);
                                            break;
                                    }

                                    float buildingWidth = buildingInfo.width;

                                    ScaleSelectionCursor(new Vector3(buildingWidth / 2f, buildingWidth / 2f, buildingWidth / 2f));
                                    ModifySelectionCursorSideDistance(1.25f, 1.5f);
                                    break;
                                }

                            //Villagers Layer
                            case 11:
                                {
                                    StartCoroutine(FollowSelected(selectedObject));

                                    ScaleSelectionCursor(new Vector3(0.5f, 0.5f, 0.5f));
                                    break;
                                }

                            //Piles Layer
                            case 12:
                                {
                                    //Differentiate between different resource types
                                    switch (selectedObject.tag)
                                    {
                                        case "Log":
                                            ScaleSelectionCursor(new Vector3(0.3f, 0.3f, 0.3f));
                                            break;
                                        default:
                                            Debug.Log("Invalid tag specified: " + selectedObject.tag);
                                            break;
                                    }

                                    PositionSelectionCursor(selectedObject.transform.position, true);
                                    break;
                                }

                            default:
                                {
                                    Debug.LogError("Error: Invalid Layer ID given: " + hit.transform.gameObject.layer);
                                    break;
                                }
                        }

                        if (!UIManagerScript.mainPanels["SelectionPanel"].activeInHierarchy)
                            UIManagerScript.mainPanels["SelectionPanel"].SetActive(true);
                    }
                }
                else
                {
                    //De-Select everything if the player clicked away
                    if (Input.GetMouseButtonDown(0))
                        DeselectAll();
                }
            }

            if (PlaceBuilding._placingBuilding)
                DeselectAll();

            //If nothing is selected
            if (selectedObject == null)
            {
                anythingSelected = false;

                //Deactivate the selection cursor
                selectionCursor.SetActive(false);
            }
            else
            {
                anythingSelected = true;
            }

            if (selectionLocked && selectedObject == null)
                LockSelection(false);

            //Update the UI (User Interface)
            UpdateUI();

            //Go through with keypress actions based on what's selected
            if (anythingSelected)
                Actions();
        }

        void UpdateUI()
        {
            HideSubtitles(3);

            if (!anythingSelected)
            {
                //Close the selection panel
                UIManagerScript.ClosePanel("SelectionPanel");
                return;
            }

            switch (selectedObject.layer)
            {
                //Terrain
                case 8:
                    {
                        //Cached data
                        string tileType = Environment.tileType[tileIndex.x, tileIndex.y];
                        int fertility = Environment.fertility[tileIndex.x, tileIndex.y];

                        string fertilityLevel;

                        //Change the display of the tile's fertility based on the tile's actual fertility value
                        switch (fertility)
                        {
                            case 0:
                            default:
                                fertilityLevel = "Infertile";
                                break;
                            case 1:
                                fertilityLevel = "Low Fertility";
                                break;
                            case 2:
                                fertilityLevel = "Fertile";
                                break;
                            case 3:
                                fertilityLevel = "Very Fertile";
                                break;
                        }

                        //Change the title of the selection panel to the type of tile that's selected.
                        UIManagerScript.ChangeText("SelectedTitle", tileType);

                        //If the tile is a water/shore tile, then find the depth. Once that's found, set the selection panel's subtitle to the depth of the water tile.
                        if (tileType == "Water" || tileType == "Shore")
                        {
                            //UV basically represents the color of the tile and is represented by x and y values.
                            float uvY = Environment.uvs[tileIndex.x, tileIndex.y].y;
                            string waterDepth = "Shallow";

                            //Depending on the darkness of the water (uvY) the depth of the water can be found.
                            if (uvY < 0.8f && uvY >= 0.5f)
                                waterDepth = "Medium Depth";
                            else if (uvY < 0.5f && uvY >= 0.4f)
                                waterDepth = "Deep";
                            else if (uvY < 0.4f)
                                waterDepth = "Deeper";

                            //Change the selection panel's subtitle to the depth of the selected water tile
                            UIManagerScript.ChangeText("Subtitle1", waterDepth);
                        }

                        //If the tile is grass or sand display the fertility of the tile
                        if (tileType == "Grass" || tileType == "Sand")
                            UIManagerScript.ChangeText("Subtitle1", fertilityLevel);

                        //If the tile is a fishing tile, display the fact that it has fish
                        if (Environment.fishingTile[tileIndex.x, tileIndex.y])
                            UIManagerScript.ChangeText("Subtitle1", "Good fishing spot");

                        //Show the first subtitle
                        ShowSubtitles(1);

                        break;
                    }

                //Resource
                case 9:
                    {
                        string resourceType = selectedObject.tag;

                        UIManagerScript.ChangeText("SelectedTitle", resourceType);

                        switch (resourceType)
                        {
                            case "Tree":
                                UIManagerScript.ShowMiscUI("ChopTreeButton");

                                if (selectedObject.transform.localScale.x > 1.0f)
                                    UIManagerScript.ChangeText("Subtitle1", "Medium Tree");
                                else
                                    UIManagerScript.ChangeText("Subtitle1", "Small Tree");

                                if (selectedObject.transform.localScale.x > 1.2f)
                                    UIManagerScript.ChangeText("Subtitle1", "Large Tree");

                                UIManagerScript.ChangeText("Subtitle2", "Wood amount: " + selectedObject.GetComponent<Resource>().resourceAmount);
                                break;
                            case "Stone":
                                if (selectedObject.transform.localScale.x > 0.6f)
                                    UIManagerScript.ChangeText("Subtitle1", "Medium Rock");
                                else
                                    UIManagerScript.ChangeText("Subtitle1", "Small Rock");

                                if (selectedObject.transform.localScale.x > 0.7f)
                                    UIManagerScript.ChangeText("Subtitle1", "Large Rock");

                                UIManagerScript.ChangeText("Subtitle2", "Stone amount: " + selectedObject.GetComponent<Resource>().resourceAmount);
                                break;
                            case "Iron":
                                break;
                            case "Bronze":
                                break;
                            case "Gold":
                                break;
                        }

                        ShowSubtitles(2);
                        break;
                    }

                //Building
                case 10:
                    {
                        UIManagerScript.ChangeText("SelectedTitle", selectedObject.name);

                        if (selectedObject.GetComponent<UnderConstruction>() == null)
                            UIManagerScript.ChangeText("Subtitle1", "Good for storing things");
                        else
                            UIManagerScript.ChangeText("Subtitle1", "Under Construction");

                        ShowSubtitles(1);
                        break;
                    }

                //Villager
                case 11:
                    {
                        var villager = selectedObject.GetComponent<Villager>();

                        UIManagerScript.ChangeText("SelectedTitle", villager._name);

                        UIManagerScript.ChangeText("Subtitle1", "Gender: " + villager._sex);
                        UIManagerScript.ChangeText("Subtitle2", "Currently " + VillagerPropertiesGenerator.CurrentJobDescription(villager));

                        //Only change dropdown value if the villager's role changes. Check later if this is necessary.
                        if ((int)System.Enum.Parse(typeof(VillagerRoles), villager._role) != _villagerRole)
                        {
                            _villagerRole = (int)System.Enum.Parse(typeof(VillagerRoles), villager._role);

                            UIManagerScript.UpdateDropdown("RoleSelector", _villagerRole - 1 /*VillagerRoles Enum begins at 1*/);
                        }

                        ShowSubtitles(2);

                        UIManagerScript.ShowMiscUI("JobsViewButton");
                        break;
                    }

                //Piles Layer
                case 12:
                    {
                        //Differentiate between different resource types
                        switch (selectedObject.tag)
                        {
                            case "Log":
                                UIManagerScript.ChangeText("SelectedTitle", "Pile of Logs");

                                UIManagerScript.ChangeText("Subtitle1", "It's a pile of logs.");
                                UIManagerScript.ChangeText("Subtitle2", "Number of logs: " + selectedObject.GetComponent<ItemPile>().amountOfItems);
                                break;
                            default:
                                Debug.Log("Invalid tag specified: " + selectedObject.tag);
                                break;
                        }

                        ItemPile itemPileScript = selectedObject.GetComponent<ItemPile>();

                        //Add a 0 before the minutes/seconds if they are less than 10 so the output is always xx:yy.
                        string formattedMinutes = itemPileScript.despawnMinutes < 10 ? "0" + itemPileScript.despawnMinutes.ToString() : itemPileScript.despawnMinutes.ToString();
                        string formattedSeconds = itemPileScript.despawnSeconds < 10 ? "0" + itemPileScript.despawnSeconds.ToString() : itemPileScript.despawnSeconds.ToString();

                        UIManagerScript.ChangeText("ExtraDisplay", "Despawns in: " + formattedMinutes + ":" + formattedSeconds);

                        if (itemPileScript.beingPickedUp)
                            UIManagerScript.ChangeText("ExtraDisplay", "Being picked up.");

                        ShowSubtitles(3);
                        break;
                    }
            }

            UIManagerScript.OpenPanel("SelectionPanel");
        }

        void Actions()
        {
            switch (selectedObject.layer)
            {
                //Terrain
                case 8:
                    {
                        break;
                    }

                //Resource
                case 9:
                    {
                        switch (selectedObject.tag)
                        {
                            case "Tree":
                                if (Input.GetKeyDown(KeyCode.C))
                                    jobManager.ChopSelectedTree();
                                break;
                            case "Stone":
                                break;
                        }

                        break;
                    }

                //Building
                case 10:
                    {
                        break;
                    }

                //Villager
                case 11:
                    {
                        //Open/Close Jobs panel if the 'J' key is pressed.
                        if (Input.GetKeyDown(KeyCode.J))
                        {
                            if (!UIManagerScript.mainPanels["JobsPanel"].activeInHierarchy)
                                StartCoroutine(displayJobsList.DisplayVillagerJobs("SelectTile.cs"));

                            UIManagerScript.ToggleUIObject(UIManagerScript.mainPanels["JobsPanel"]);
                            UIManagerScript.ToggleUIObject(UIManagerScript.mainPanels["SelectionDescriptionPanel"]);
                        }
                        break;
                    }
            }

            //Lock
            if (Input.GetKeyDown(KeyCode.L))
            {
                bool lockToggle = !selectionLocked;

                LockSelection(lockToggle);
            }
        }

        public void ActionInit()
        {
            if (selectedObject.GetComponent<Resource>() != null)
            {
                if (selectedObject.GetComponent<Resource>().beingHarvested)
                    return;
            }

            if (selectedObject)
                PositionSelectionCursor(selectedObject.transform.position, true);
        }

        void ShowSubtitles(int subtitleAmount)
        {
            for (int i = 1; i <= subtitleAmount; i++)
            {
                if (subtitleAmount > 2)
                    UIManagerScript.ShowText("ExtraDisplay");
                else
                    UIManagerScript.ShowText("Subtitle" + i);

            }
        }

        void HideSubtitles(int subtitleAmount)
        {
            for (int i = 1; i < subtitleAmount; i++)
            {
                if (subtitleAmount > 2)
                    UIManagerScript.HideText("ExtraDisplay");
                else
                    UIManagerScript.HideText("Subtitle" + i);
            }
        }

        void CloseOpenedTempUI()
        {
            UIManagerScript.OpenPanel("SelectionDescriptionPanel");

            UIManagerScript.ClosePanel("JobsPanel");
            UIManagerScript.HideMiscUI("JobsViewButton");
            UIManagerScript.HideMiscUI("ChopTreeButton");
        }

        //Change the position of the selection cursor based on the position passed through the function, and the fact of whether the selected object is a tile or not.
        void PositionSelectionCursor(Vector3 position, Vector2Int tileIndex = default)
        {
            //Make a variable of the future position of the selection cursor so that calculations can be done without affecting the actual position of the selection cursor.
            Vector3 selectionCursorPos;

            //Simply set the selection cursor's position to the position value passed through the function (Vector3 position).
            selectionCursorPos = position;

            selectionCursorScript.UpdateCursorPos(selectionCursorPos);


            //Activate the selection cursor (makes it visible)
            selectionCursor.SetActive(true);
        }

        //Change the position of the selection cursor based on the position passed through the function, and the fact of whether the selected object is a tile or not.
        void PositionSelectionCursor(Vector3 position, bool isAnimating)
        {
            //Make a variable of the future position of the selection cursor so that calculations can be done without affecting the actual position of the selection cursor.
            Vector3 selectionCursorPos;

            //Set the selection cursor's position to the nearest point on the placement grid (PlacementGrid.cs).
            selectionCursorPos = FindObjectOfType<PlacementGrid>().GetNearestPointOnGrid(position);

            if (isAnimating)
                selectionCursorScript.UpdateCursorPos(new Vector3(selectionCursorPos.x, selectedObject.transform.position.y + selectionCursorHeight, selectionCursorPos.z));
            else
                selectionCursorScript.UpdateCursorPos(new Vector3(selectionCursorPos.x, selectedObject.transform.position.y + selectionCursorHeight, selectionCursorPos.z), false);

            //Activate the selection cursor (makes it visible)
            selectionCursor.SetActive(true);
        }

        void PositionSelectionCursorCustom(Vector3 position, bool isAnimating)
        {
            if (isAnimating)
                selectionCursorScript.UpdateCursorPos(new Vector3(position.x, selectedObject.transform.position.y + selectionCursorHeight, position.z));
            else
                selectionCursorScript.UpdateCursorPosCustom(new Vector3(position.x, position.y + selectionCursorHeight, position.z));

            //Activate the selection cursor (makes it visible)
            selectionCursor.SetActive(true);
        }

        //Change the scale of the selection cursor based on the scale value passed through the function (entire scale values (Vector3)).
        void ScaleSelectionCursor(Vector3 scale)
        {
            if (selectionCursor)
            {
                SelectionCursor selectionCursorScript = selectionCursor.GetComponent<SelectionCursor>();

                if (selectionCursorScript.updateScale != null)
                {
                    StopCoroutine(selectionCursorScript.updateScale);
                }

                selectionCursorScript.updateScale = selectionCursorScript.UpdateScale(scale);
                StartCoroutine(selectionCursorScript.updateScale);
            }
        }

        void ModifySelectionCursorSideDistance(float minSideDist, float maxSideDist)
        {
            selectionCursorScript.sideDistMin = minSideDist;
            selectionCursorScript.sideDistMax = maxSideDist;
        }

        void RestoreSelectionCursorSideDistanceToDefault()
        {
            selectionCursorScript.sideDistMin = defaultSelectionCursorSideDistance.x;
            selectionCursorScript.sideDistMax = defaultSelectionCursorSideDistance.y;
        }

        //Deselect everything by setting the 'selectedObject' to null, which basically means setting it to empty (nothing).
        public void DeselectAll()
        {
            LockSelection(false);

            selectedObject = null;
        }

        //Get the index of the tile in the tileCentres[,] array given its position
        public Vector2Int GetTileIndex(Vector3 tilePos)
        {
            for (int y = 0; y < terrainGenerator.worldSize; y++)
            {
                for (int x = 0; x < terrainGenerator.worldSize; x++)
                {
                    //Check if it's land
                    if (Environment.tileCentres[x, y] == new Vector3(tilePos.x, Environment.tileCentres[x, y].y, tilePos.z))
                        return new Vector2Int(x, y);
                }
            }

            //If the tile can't be found, return an impossible value, signaling an error.
            return new Vector2Int(-1, -1);
        }

        public void UpdateVillagerRole(TMP_Dropdown dropDown) => selectedObject.GetComponent<Villager>()._role = System.Enum.GetName(typeof(VillagerRoles), dropDown.value + 1 /*VillagerRoles Enum begins at 1*/);

        IEnumerator FollowSelected(GameObject gameObject)
        {
            while (selectedObject == gameObject)
            {
                PositionSelectionCursorCustom(selectedObject.transform.position, false);

                yield return null;
            }

            yield return null;
        }

        public void LockSelection(bool lockSelection)
        {
            if (selectionLocked != lockSelection)
            {
                UIManagerScript.ToggleUIObject(UIManagerScript.miscUIElements["LockButton"]);
                UIManagerScript.ToggleUIObject(UIManagerScript.miscUIElements["UnlockButton"]);
            }

            selectionLocked = lockSelection;
        }

        public void DisplayVillagerJobsButton(string source) => StartCoroutine(displayJobsList.DisplayVillagerJobs(source));
    }
}
