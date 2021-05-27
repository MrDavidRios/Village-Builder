using System;
using System.Collections;
using DavidRios.Assets.Scripts.Villager;
using DavidRios.Building;
using DavidRios.Input;
<<<<<<< Updated upstream
=======
using DavidRios.UI;
using DavidRios.Villager;
>>>>>>> Stashed changes
using Terrain;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Environment = DavidRios.Environment.Environment;

namespace TileOperations
{
    public class Select : MonoBehaviour
    {
        public static bool CanSelect;

        public static bool SelectionLocked;

        //GameObjects
        public static GameObject SelectedObject;

        private static UIManager _uiManagerScript;

        //Booleans
        public bool anythingSelected;

        //Floats
        public float selectionCursorHeight;

        //Strings
        public string[] tempMiscUINames;

        public GameObject selectionCursor;

        public GameObject terrainMesh;

        //LayerMask
        public LayerMask selectable;

        public Vector2 selectionPos;

        public Camera mainCamera;

        private int _villagerRole;

        //Building Info
        private Building _buildingInfo;

        private Vector2 _defaultSelectionCursorSideDistance;
        private DisplayJobsList _displayJobsList;

        //Raycasting
        private RaycastHit _hit;
        private JobManager _jobManager;
        private PlacementGrid _placementGrid;
        private Ray _ray;
        private SelectionCursor _selectionCursorScript;

        //Scripts
        private TerrainData _terrainData;
        private TerrainGenerator _terrainGenerator;
        
        //Input
        private PlayerController.DefaultActions _input;

        //Integers
        private Vector2Int _tileIndex;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!CanSelect)
                return;

            //The ray goes from the camera's position on the screen to the mouse cursor's position
            _ray = mainCamera.ScreenPointToRay(InputHandler.MousePosition);

            //This code won't run if the mouse is hovering over the UI or if a building is being placed
            if (!EventSystem.current.IsPointerOverGameObject() && !PositionBuildingTemplate.PlacingBuilding &&
                !SelectionLocked)
            {
                //If the raycast hit something with a collider, run the code inside this 'if' statement
                if (Physics.Raycast(_ray, out _hit, GlobalRules._maxRaycastDistance, selectable))
                {
                    //Left-Click
                    if (InputHandler.Pressed(_input.LeftClick))
                    {
                        RestoreSelectionCursorSideDistanceToDefault();

                        InitializeTempUI();

                        //Set the selected object equal to the object that was clicked on
                        SelectedObject = _hit.transform.gameObject;

                        //Check what kind of object was selected based on its layer
                        switch (SelectedObject.layer)
                        {
                            //Terrain Layer (Tiles)
                            case 8:
                            {
                                var tilePos = _placementGrid.GetNearestPointOnGrid(_hit.point);

                                //Find the x and y indexes of the tile
                                _tileIndex = GetTileIndex(tilePos);

                                //Change the scale of the selection cursor
                                ScaleSelectionCursor(new Vector3(0.5f, 0.5f, 0.5f));

                                //Is this tile a water tile?
                                if (Environment.TileType[_tileIndex.x, _tileIndex.y] == "Water")
                                    //If the tile is a water tile, move the cursor down 0.4 units because water tiles are lower than land tiles.
                                    PositionSelectionCursor(tilePos + new Vector3(0f, -0.4f, 0f), _tileIndex);
                                else
                                    PositionSelectionCursor(tilePos, _tileIndex);

                                SelectedObject = _terrainGenerator.gameObject;
                                break;
                            }

                            //Resource Layer (Trees, rocks, etc.)
                            case 9:
                            {
                                //Differentiate between different resource types
                                switch (SelectedObject.tag)
                                {
                                    case "Tree":
                                        ScaleSelectionCursor(new Vector3(0.75f, 0.75f, 0.75f));
                                        break;
                                    case "Stone":
                                        ScaleSelectionCursor(SelectedObject.transform.localScale);
                                        break;
                                    case "Iron":
                                        break;
                                    case "Bronze":
                                        break;
                                    case "Gold":
                                        break;
                                    default:
                                        Debug.Log("Invalid tag specified: " + SelectedObject.tag);
                                        break;
                                }

                                PositionSelectionCursor(SelectedObject.transform.position, true);
                                break;
                            }

                            //Buildings Layer
                            case 10:
                            {
                                PositionSelectionCursorCustom(SelectedObject.transform.position, true);

                                _buildingInfo = BuildingOperations.GetBuildingScriptableObject(SelectedObject.transform);

                                float buildingWidth = _buildingInfo.width;

                                ScaleSelectionCursor(new Vector3(buildingWidth / 2f, buildingWidth / 2f,
                                    buildingWidth / 2f));
                                ModifySelectionCursorSideDistance(1.25f, 1.5f);
                                break;
                            }

                            //Villagers Layer
                            case 11:
                            {
                                StartCoroutine(FollowSelected(SelectedObject));

                                ScaleSelectionCursor(new Vector3(0.5f, 0.5f, 0.5f));
                                break;
                            }

                            //Piles Layer
                            case 12:
                            {
                                //Differentiate between different resource types
                                switch (SelectedObject.tag)
                                {
                                    case "Log":
                                        ScaleSelectionCursor(new Vector3(0.3f, 0.3f, 0.3f));
                                        break;
                                    default:
                                        Debug.Log("Invalid tag specified: " + SelectedObject.tag);
                                        break;
                                }

                                PositionSelectionCursor(SelectedObject.transform.position, true);
                                break;
                            }

                            default:
                            {
                                Debug.LogError("Error: Invalid Layer ID given: " + _hit.transform.gameObject.layer);
                                break;
                            }
                        }

                        if (!_uiManagerScript.mainPanels["SelectionPanel"].activeInHierarchy)
                            _uiManagerScript.mainPanels["SelectionPanel"].SetActive(true);
                    }
                }
                else
                {
                    //De-Select everything if the player clicked away
                    if (InputHandler.Pressed(_input.LeftClick))
                        DeselectAll();
                }
            }

            if (PositionBuildingTemplate.PlacingBuilding)
                DeselectAll();

            //If nothing is selected
            if (SelectedObject == null)
            {
                anythingSelected = false;

                //Deactivate the selection cursor
                selectionCursor.SetActive(false);
            }
            else
            {
                anythingSelected = true;
            }

            if (SelectionLocked && SelectedObject == null)
                LockSelection(false);

            //Update the UI (User Interface)
            UpdateUI();

            //Go through with keypress actions based on what's selected
            if (anythingSelected)
                Actions();
        }

        private void Initialize()
        {
            //Cache necessary scripts
            _terrainGenerator = FindObjectOfType<TerrainGenerator>();
            _terrainData = FindObjectOfType<TerrainData>();
            _placementGrid = FindObjectOfType<PlacementGrid>();
            _uiManagerScript = FindObjectOfType<UIManager>();
            _selectionCursorScript = FindObjectOfType<SelectionCursor>();
            _jobManager = FindObjectOfType<JobManager>();
            _displayJobsList = FindObjectOfType<DisplayJobsList>();

            selectionCursor = _selectionCursorScript.gameObject;

            _defaultSelectionCursorSideDistance =
                new Vector2(_selectionCursorScript.sideDist, _selectionCursorScript.sideDist * 1.25f);

            selectionCursor.SetActive(false);

            //Initialize Terrain Collider
            var terrainCollider = terrainMesh.GetComponent<BoxCollider>();
            terrainCollider.center =
                new Vector3(_terrainGenerator.worldSize / 2, -0.75f, _terrainGenerator.worldSize / 2);
            terrainCollider.size = new Vector3(_terrainGenerator.worldSize, 1.5f, _terrainGenerator.worldSize);

            _input = InputHandler.PlayerControllerInstance.Default;
            
            //Set necessary variables
            CanSelect = true;
        }

        private void UpdateUI()
        {
            HideSubtitles(3);

            if (!anythingSelected)
            {
                //Close the selection panel
                _uiManagerScript.ClosePanel("SelectionPanel");
                return;
            }

            switch (SelectedObject.layer)
            {
                //Terrain
                case 8:
                {
                    //Cached data
                    var tileType = Environment.TileType[_tileIndex.x, _tileIndex.y];
                    var fertility = Environment.Fertility[_tileIndex.x, _tileIndex.y];

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
                    _uiManagerScript.ChangeText("SelectedTitle", tileType);

                    //If the tile is a water/shore tile, then find the depth. Once that's found, set the selection panel's subtitle to the depth of the water tile.
                    if (tileType == "Water" || tileType == "Shore")
                    {
                        //UV basically represents the color of the tile and is represented by x and y values.
                        var uvY = Environment.Uvs[_tileIndex.x, _tileIndex.y].y;
                        var waterDepth = "Shallow";

                        //Depending on the darkness of the water (uvY) the depth of the water can be found.
                        if (uvY < 0.8f && uvY >= 0.5f)
                            waterDepth = "Medium Depth";
                        else if (uvY < 0.5f && uvY >= 0.4f)
                            waterDepth = "Deep";
                        else if (uvY < 0.4f)
                            waterDepth = "Deeper";

                        //Change the selection panel's subtitle to the depth of the selected water tile
                        _uiManagerScript.ChangeText("Subtitle1", waterDepth);
                    }

                    //If the tile is grass or sand display the fertility of the tile
                    if (tileType == "Grass" || tileType == "Sand")
                        _uiManagerScript.ChangeText("Subtitle1", fertilityLevel);

                    //If the tile is a fishing tile, display the fact that it has fish
                    if (Environment.FishingTile[_tileIndex.x, _tileIndex.y])
                        _uiManagerScript.ChangeText("Subtitle1", "Good fishing spot");

                    //Show the first subtitle
                    ShowSubtitles(1);

                    break;
                }

                //Resource
                case 9:
                {
                    var resourceType = SelectedObject.tag;

                    _uiManagerScript.ChangeText("SelectedTitle", resourceType);

                    switch (resourceType)
                    {
                        case "Tree":
                        {
                            if (!SelectedObject.GetComponent<Resource>().beingHarvested)
                                _uiManagerScript.ShowMiscUI("ChopTreeButton");
                            else
                                _uiManagerScript.HideMiscUI("ChopTreeButton");

                            if (SelectedObject.transform.localScale.x > 1.0f)
                                _uiManagerScript.ChangeText("Subtitle1", "Medium Tree");
                            else
                                _uiManagerScript.ChangeText("Subtitle1", "Small Tree");

                            if (SelectedObject.transform.localScale.x > 1.2f)
                                _uiManagerScript.ChangeText("Subtitle1", "Large Tree");

                            _uiManagerScript.ChangeText("Subtitle2",
                                "Wood amount: " + SelectedObject.GetComponent<Resource>().resourceAmount);
                            break;
                        }
                        case "Stone":
                        {
                            if (SelectedObject.transform.localScale.x > 0.6f)
                                _uiManagerScript.ChangeText("Subtitle1", "Medium Rock");
                            else
                                _uiManagerScript.ChangeText("Subtitle1", "Small Rock");

                            if (SelectedObject.transform.localScale.x > 0.7f)
                                _uiManagerScript.ChangeText("Subtitle1", "Large Rock");

                            _uiManagerScript.ChangeText("Subtitle2",
                                "Stone amount: " + SelectedObject.GetComponent<Resource>().resourceAmount);
                            break;
                        }
                        case "Iron":
                        {
                            break;
                        }
                        case "Bronze":
                        {
                            break;
                        }
                        case "Gold":
                        {
                            break;
                        }
                    }

                    ShowSubtitles(2);
                    break;
                }

                //Building
                case 10:
                {
                    _uiManagerScript.ChangeText("SelectedTitle", SelectedObject.name);

                    if (SelectedObject.GetComponent<UnderConstruction>() == null)
                    {
                        var buildingType = BuildingOperations.GetBuildingScriptableObject(SelectedObject.transform)
                            .buildingType;
                        string subtitle1Text;

                        switch (buildingType)
                        {
                            case "Storage":
                                subtitle1Text = "Good for storing things";
                                break;
                            case "House":
                                subtitle1Text = "Good for villagers to live in";
                                break;
                            case "Road":
                                subtitle1Text = "Helps villagers walk faster.";
                                break;
                            default:
                                Debug.LogError("Invalid building type: " + buildingType);
                                subtitle1Text = "?";
                                break;
                        }

                        _uiManagerScript.ChangeText("Subtitle1", subtitle1Text);

                        _uiManagerScript.HideMiscUI("TemplateClearButton");
                    }
                    else
                    {
                        var underConstructionScript = SelectedObject.GetComponent<UnderConstruction>();

                        //If the building's current stage is 0, that means that it is still a template.
                        if (underConstructionScript.currentStage == 0 && !underConstructionScript.grassCleared &&
                            !underConstructionScript.beingDeposited && underConstructionScript.laborAmount == 0 &&
                            !_uiManagerScript.miscUIElements["TemplateClearButton"].activeInHierarchy)
                        {
                            _uiManagerScript.miscUIElements["TemplateClearButton"].GetComponent<Button>().onClick
                                .AddListener(() =>
                                    StartCoroutine(PositionBuildingTemplate.RemoveTemplate(SelectedObject.transform,
                                        _uiManagerScript.miscUIElements["TemplateClearButton"].GetComponent<Button>())));
                            _uiManagerScript.ShowMiscUI("TemplateClearButton");
                        }
                        //If the current stage isn't 0 or the building's labor amount is greater than 0, then hide the template clear button.
                        else if (underConstructionScript.currentStage != 0 || underConstructionScript.laborAmount > 0 ||
                                 underConstructionScript.grassCleared || underConstructionScript.beingDeposited)
                        {
                            _uiManagerScript.HideMiscUI("TemplateClearButton");
                        }

                        _uiManagerScript.ChangeText("Subtitle1", "Under Construction");
                    }

                    ShowSubtitles(1);
                    break;
                }

                //Villager
                case 11:
                {
                    var villager = SelectedObject.GetComponent<VillagerLogic>();

                    _uiManagerScript.ChangeText("SelectedTitle", villager.name);

                    _uiManagerScript.ChangeText("Subtitle1", "Gender: " + villager.sex);

                    if (villager.customJobDescription == "")
                        _uiManagerScript.ChangeText("Subtitle2",
                            "Currently " + VillagerPropertiesGenerator.CurrentJobDescription(villager));
                    else
                        _uiManagerScript.ChangeText("Subtitle2", villager.customJobDescription);

                    //Only change dropdown value if the villager's role changes. Check later if this is necessary.
                    if ((int) Enum.Parse(typeof(VillagerRoles), villager.role) != _villagerRole)
                    {
                        _villagerRole = (int) Enum.Parse(typeof(VillagerRoles), villager.role);

                        _uiManagerScript.UpdateDropdown("RoleSelector",
                            _villagerRole - 1 /*VillagerRoles Enum begins at 1*/);
                    }

                    ShowSubtitles(2);

                    _uiManagerScript.ShowMiscUI("JobsViewButton");
                    break;
                }

                //Piles Layer
                case 12:
                {
                    //Differentiate between different resource types
                    switch (SelectedObject.tag)
                    {
                        case "Log":
                            _uiManagerScript.ChangeText("SelectedTitle", "Pile of Logs");

                            _uiManagerScript.ChangeText("Subtitle1", "It's a pile of logs.");
                            _uiManagerScript.ChangeText("Subtitle2",
                                "Number of logs: " + SelectedObject.GetComponent<ItemPile>().amountOfItems);
                            break;
                        default:
                            Debug.Log("Invalid tag specified: " + SelectedObject.tag);
                            break;
                    }

                    var itemPileScript = SelectedObject.GetComponent<ItemPile>();

                    //Add a 0 before the minutes/seconds if they are less than 10 so the output is always xx:yy.
                    var formattedMinutes = itemPileScript.despawnMinutes < 10
                        ? "0" + itemPileScript.despawnMinutes
                        : itemPileScript.despawnMinutes.ToString();
                    var formattedSeconds = itemPileScript.despawnSeconds < 10
                        ? "0" + itemPileScript.despawnSeconds
                        : itemPileScript.despawnSeconds.ToString();

                    _uiManagerScript.ChangeText("ExtraDisplay",
                        "Despawns in: " + formattedMinutes + ":" + formattedSeconds);

                    if (itemPileScript.beingPickedUp)
                        _uiManagerScript.ChangeText("ExtraDisplay", "Being picked up.");

                    ShowSubtitles(3);
                    break;
                }
            }

            _uiManagerScript.OpenPanel("SelectionPanel");
        }

        private void Actions()
        {
            switch (SelectedObject.layer)
            {
                //Terrain
                case 8:
                {
                    break;
                }

                //Resource
                case 9:
                {
                    switch (SelectedObject.tag)
                    {
                        case "Tree":
                            if (InputHandler.Pressed(_input.ChopTree))
                                _jobManager.ChopSelectedTree();
                            break;
                        case "Stone":
                            break;
                    }

                    break;
                }

                //Building
                case 10:
                {
                    if (InputHandler.Pressed(_input.Delete))
                        if (_uiManagerScript.miscUIElements["TemplateClearButton"].activeInHierarchy)
                            _uiManagerScript.miscUIElements["TemplateClearButton"].GetComponent<Button>().onClick
                                .Invoke();
                    break;
                }

                //Villager
                case 11:
                {
                    //Open/Close Jobs panel if the 'J' key is pressed.
                    if (InputHandler.Pressed(_input.JobsMenuToggle))
                    {
                        if (!_uiManagerScript.mainPanels["JobsPanel"].activeInHierarchy)
                            StartCoroutine(_displayJobsList.DisplayVillagerJobs("SelectTile.cs"));

                        _uiManagerScript.ToggleUIObject(_uiManagerScript.mainPanels["JobsPanel"]);
                        _uiManagerScript.ToggleUIObject(_uiManagerScript.mainPanels["SelectionDescriptionPanel"]);
                    }

                    break;
                }
            }

            //Lock
            if (InputHandler.Pressed(_input.LockBuilding))
            {
                var lockToggle = !SelectionLocked;

                LockSelection(lockToggle);
            }
        }

        public void ActionInit()
        {
            if (SelectedObject.GetComponent<Resource>() != null)
                if (SelectedObject.GetComponent<Resource>().beingHarvested)
                    return;

            if (SelectedObject)
                PositionSelectionCursor(SelectedObject.transform.position, true);
        }

        private void ShowSubtitles(int subtitleAmount)
        {
            for (var i = 1; i <= subtitleAmount; i++)
                if (i > 2)
                    _uiManagerScript.ShowText("ExtraDisplay");
                else
                    _uiManagerScript.ShowText("Subtitle" + i);
        }

        private void HideSubtitles(int subtitleAmount)
        {
            for (var i = 1; i <= subtitleAmount; i++)
                if (i > 2)
                    _uiManagerScript.HideText("ExtraDisplay");
                else
                    _uiManagerScript.HideText("Subtitle" + i);
        }

        /// <summary>
        ///     Close the UI that is focused (jobs panel is only focused on villagers, chop button is focused on trees, etc.)
        /// </summary>
        private void InitializeTempUI()
        {
            _uiManagerScript.OpenPanel("SelectionDescriptionPanel");

            _uiManagerScript.ClosePanel("JobsPanel");

            for (var i = 0; i < tempMiscUINames.Length; i++) _uiManagerScript.HideMiscUI(tempMiscUINames[i]);
        }

        //Change the position of the selection cursor based on the position passed through the function, and the fact of whether the selected object is a tile or not.
        private void PositionSelectionCursor(Vector3 position, Vector2Int tileIndex = default)
        {
            //Make a variable of the future position of the selection cursor so that calculations can be done without affecting the actual position of the selection cursor.
            Vector3 selectionCursorPos;

            //Simply set the selection cursor's position to the position value passed through the function (Vector3 position).
            selectionCursorPos = position;

            _selectionCursorScript.UpdateCursorPos(selectionCursorPos);


            //Activate the selection cursor (makes it visible)
            selectionCursor.SetActive(true);
        }

        //Change the position of the selection cursor based on the position passed through the function, and the fact of whether the selected object is a tile or not.
        private void PositionSelectionCursor(Vector3 position, bool isAnimating)
        {
            //Make a variable of the future position of the selection cursor so that calculations can be done without affecting the actual position of the selection cursor.
            Vector3 selectionCursorPos;

            //Set the selection cursor's position to the nearest point on the placement grid (PlacementGrid.cs).
            selectionCursorPos = FindObjectOfType<PlacementGrid>().GetNearestPointOnGrid(position);

            if (isAnimating)
                _selectionCursorScript.UpdateCursorPos(new Vector3(selectionCursorPos.x,
                    SelectedObject.transform.position.y + selectionCursorHeight, selectionCursorPos.z));
            else
                _selectionCursorScript.UpdateCursorPos(
                    new Vector3(selectionCursorPos.x, SelectedObject.transform.position.y + selectionCursorHeight,
                        selectionCursorPos.z), false);

            //Activate the selection cursor (makes it visible)
            selectionCursor.SetActive(true);
        }

        private void PositionSelectionCursorCustom(Vector3 position, bool isAnimating)
        {
            if (isAnimating)
                _selectionCursorScript.UpdateCursorPos(new Vector3(position.x,
                    SelectedObject.transform.position.y + selectionCursorHeight, position.z));
            else
                _selectionCursorScript.UpdateCursorPosCustom(new Vector3(position.x, position.y + selectionCursorHeight,
                    position.z));

            //Activate the selection cursor (makes it visible)
            selectionCursor.SetActive(true);
        }

        //Change the scale of the selection cursor based on the scale value passed through the function (entire scale values (Vector3)).
        private void ScaleSelectionCursor(Vector3 scale)
        {
            if (selectionCursor)
            {
                var selectionCursorScript = selectionCursor.GetComponent<SelectionCursor>();

                if (selectionCursorScript.updateScale != null) StopCoroutine(selectionCursorScript.updateScale);

                selectionCursorScript.updateScale = selectionCursorScript.UpdateScale(scale);
                StartCoroutine(selectionCursorScript.updateScale);
            }
        }

        private void ModifySelectionCursorSideDistance(float minSideDist, float maxSideDist)
        {
            _selectionCursorScript.sideDistMin = minSideDist;
            _selectionCursorScript.sideDistMax = maxSideDist;
        }

        private void RestoreSelectionCursorSideDistanceToDefault()
        {
            _selectionCursorScript.sideDistMin = _defaultSelectionCursorSideDistance.x;
            _selectionCursorScript.sideDistMax = _defaultSelectionCursorSideDistance.y;
        }

        //Deselect everything by setting the 'selectedObject' to null, which basically means setting it to empty (nothing).
        public void DeselectAll()
        {
            LockSelection(false);

            SelectedObject = null;
        }

        //Get the index of the tile in the tileCentres[,] array given its position
        public Vector2Int GetTileIndex(Vector3 tilePos)
        {
            for (var y = 0; y < _terrainGenerator.worldSize; y++)
            for (var x = 0; x < _terrainGenerator.worldSize; x++)
                //Check if it's land
                if (Environment.TileCentres[x, y] == new Vector3(tilePos.x, Environment.TileCentres[x, y].y, tilePos.z))
                    return new Vector2Int(x, y);

            //If the tile can't be found, return an impossible value, signaling an error.
            return new Vector2Int(-1, -1);
        }

        public void UpdateVillagerRole(TMP_Dropdown dropDown)
        {
            SelectedObject.GetComponent<VillagerLogic>().role = Enum.GetName(typeof(VillagerRoles),
                dropDown.value + 1 /*VillagerRoles Enum begins at 1*/);
        }

        private IEnumerator FollowSelected(GameObject gameObject)
        {
            while (SelectedObject == gameObject)
            {
                PositionSelectionCursorCustom(SelectedObject.transform.position, false);

                yield return null;
            }

            yield return null;
        }

        public void LockSelection(bool lockSelection)
        {
            if (SelectionLocked != lockSelection)
            {
                _uiManagerScript.ToggleUIObject(_uiManagerScript.miscUIElements["LockButton"]);
                _uiManagerScript.ToggleUIObject(_uiManagerScript.miscUIElements["UnlockButton"]);
            }

            SelectionLocked = lockSelection;
        }

        public void DisplayVillagerJobsButton(string source)
        {
            StartCoroutine(_displayJobsList.DisplayVillagerJobs(source));
        }
    }
}