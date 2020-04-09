using System.Collections;
using System.Collections.Generic;
using TerrainGeneration;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceBuilding : MonoBehaviour
{
    //Hotkeys
    public KeyCode positionLockKey;

    //Booleans
    public static bool _placingBuilding;
    private bool placingBuilding;

    private bool isSqaure;
    private bool positionLocked = false;

    private bool placeable = false;

    private bool rotating = false;

    //Integers
    private int tileLayer;

    //Float
    public float rotSpeed;

    //Strings
    public string tileLayerName;

    //Vector3s
    private Vector3 templatePos;
    private Vector3 cachedBuildingPos = Vector2.zero;

    private List<Vector3> occupiedTileCentres = new List<Vector3>();

    //Quaternions
    private Quaternion cachedBuildingRot;

    //Materials
    private Material[] templateBuildingMaterials;
    private Material[] newTemplateBuildingMaterials;

    public Material placeableBuildingMaterial;
    public Material nonPlaceableBuildingMaterial;

    //GameObjects
    public GameObject templateBuilding;

    //Camera
    private Camera mainCamera; 

    //Building Objects
    private Building building;

    //Scripts
    private PlacementGrid grid;
    private TerrainGenerator terrainGenerator;

    //Raycast
    RaycastHit hit;
    Ray ray;

    private void Awake()
    {
        grid = FindObjectOfType<PlacementGrid>();

        tileLayer = LayerMask.NameToLayer(tileLayerName);

        terrainGenerator = FindObjectOfType<TerrainGenerator>();

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        bool templateBuildingExists = templateBuilding;

        if (!templateBuildingExists)
            positionLocked = false;

        bool cursorHoveringOverUI = EventSystem.current.IsPointerOverGameObject(); 
       
        if (templateBuildingExists && !cursorHoveringOverUI && !positionLocked)
        {
            placingBuilding = true;

            //Only fire a Raycast if the mouse is over empty ground
            if (Physics.Raycast(ray, out hit, GlobalRules._maxRaycastDistance, 1 << tileLayer))
            {
                #region Init
                switch (templateBuilding.tag)
                {
                    case "Infrastructure":
                        building = templateBuilding.GetComponent<InfrastructureBuilding>().building;
                        break;
                    case "Residential":
                        building = templateBuilding.GetComponent<ResidentialBuilding>().building;
                        break;
                    case "Industrial":
                        building = templateBuilding.GetComponent<IndustrialBuilding>().building;
                        break;
                    default:
                        Debug.LogWarning("Template Building Tag not found: " + templateBuilding.tag);
                        break;
                }

                //Is the building square? (is its width equal its length)
                isSqaure = building.width == building.length;

                //If the building's width is odd
                if (isSqaure && building.width % 2 != 0)
                {
                    grid.centerInTiles = true;
                }
                //If the building's width is even
                else
                {
                    grid.centerInTiles = false;
                }

                var occupiedTiles = GetOccupiedTileCentres();

                bool buildingPlacementInvalid = CheckIfInvalid(occupiedTiles);
                placeable = !buildingPlacementInvalid;

                //This has to be done precisely after the 'GetOccupiedTileCentres' function so that the list doesn't add duplicate members.
                cachedBuildingPos = templateBuilding.transform.position;
                cachedBuildingRot = templateBuilding.transform.rotation;

                #region Non-Square buildings
                //If the building's width is odd
                if (!isSqaure && building.width % 2 != 0)
                {
                    if (building.length % 2 != 0)
                    { }
                    else
                    { }
                }
                //If the building's width is even
                else
                {
                    if (building.length % 2 != 0)
                    { }
                    else
                    { }
                }
                #endregion
                #endregion

                templatePos = grid.GetNearestPointOnGrid(hit.point);

                templatePos += templateBuilding.GetComponent<BuildingTemplate>().positionOffset;

                templateBuilding.transform.position = templatePos;

                if (placeable) { SetTemplateColor(Color.green); } else { SetTemplateColor(Color.red); }

                templateBuilding.SetActive(true);
            }
            else
            {
                //Since the mouse isn't over empty ground, the building template will be made invisible.
                templateBuilding.SetActive(false);

                placingBuilding = false;
            }

            if (!rotating)
            {
                if (Input.GetKeyDown(KeyCode.R) && !rotating)
                {
                    StartCoroutine(RotateTemplate(90f, 0.5f));
                }

                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    StartCoroutine(RotateTemplate(90f, 0.5f));
                }

                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    StartCoroutine(RotateTemplate(-90f, 0.5f));
                }
            }
        }
        else if (!positionLocked)
            placingBuilding = false;

        if (Input.GetKeyDown(positionLockKey))
            positionLocked = !positionLocked;

        _placingBuilding = placingBuilding;
    }

    private void FinalizePlacement(Vector3 newBuildingPos)
    {

    }

    //Get the positions of the tiles that the building will occupy when placed
    private List<Vector3> GetOccupiedTileCentres()
    {
        templatePos = templateBuilding.transform.position;

        //Prevent duplicate members; If the building's position and orientation is the same as before, the tiles it occupies aren't going to be different.
        if (cachedBuildingPos == templatePos && cachedBuildingRot == templateBuilding.transform.rotation && occupiedTileCentres.Count > 0)
            return occupiedTileCentres;
        else
            occupiedTileCentres.Clear();

        if (isSqaure)
        {
            //If building's dimensions are odd
            if (building.width % 2 != 0)
                occupiedTileCentres.Add(templatePos);

            switch (building.width)
            {
                //If the building is 1x1
                #region 1x1
                case 1:
                    return occupiedTileCentres;
                #endregion
                //If the building is 2x2
                #region 2x2
                case 2:
                    occupiedTileCentres.AddMany(
                        new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z - 0.5f),
                        new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z + 0.5f),
                        new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z - 0.5f),
                        new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z + 0.5f)
                    );
                    return occupiedTileCentres;
                #endregion
                //If the building is 3x3
                #region 3x3
                case 3:
                    occupiedTileCentres.AddMany(
                        new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z - 1f),
                        new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z),
                        new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z + 1f),
                        new Vector3(templatePos.x, templatePos.y, templatePos.z - 1f),
                        new Vector3(templatePos.x, templatePos.y, templatePos.z + 1f),
                        new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z - 1f),
                        new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z),
                        new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z + 1f)
                    );
                    return occupiedTileCentres;
                #endregion
                //If the building is 4x4
                #region 4x4
                case 4:
                    occupiedTileCentres.AddMany(
                        //Middle 2x2
                        new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z - 0.5f),
                        new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z + 0.5f),
                        new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z - 0.5f),
                        new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z + 0.5f),

                        //Outer Corners
                        new Vector3(templatePos.x - 1.5f, templatePos.y, templatePos.z - 1.5f),
                        new Vector3(templatePos.x - 1.5f, templatePos.y, templatePos.z + 1.5f),
                        new Vector3(templatePos.x + 1.5f, templatePos.y, templatePos.z - 1.5f),
                        new Vector3(templatePos.x + 1.5f, templatePos.y, templatePos.z + 1.5f),

                        //Outer Sides (Horizontal)
                        new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z - 1.5f),
                        new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z - 1.5f),
                        new Vector3(templatePos.x - 0.5f, templatePos.y, templatePos.z + 1.5f),
                        new Vector3(templatePos.x + 0.5f, templatePos.y, templatePos.z + 1.5f),

                        //Outer sides (Vertical)
                        new Vector3(templatePos.x - 1.5f, templatePos.y, templatePos.z - 0.5f),
                        new Vector3(templatePos.x - 1.5f, templatePos.y, templatePos.z + 0.5f),
                        new Vector3(templatePos.x + 1.5f, templatePos.y, templatePos.z - 0.5f),
                        new Vector3(templatePos.x + 1.5f, templatePos.y, templatePos.z + 0.5f)
                    );
                    return occupiedTileCentres;
                #endregion
                //If the building is 5x5
                #region 5x5
                case 5:
                    occupiedTileCentres.AddMany(
                        //Get the center x and z axis tiles

                        //x-axis
                        new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z),
                        new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z),
                        new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z),
                        new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z),

                        //z-axis
                        new Vector3(templatePos.x, templatePos.y, templatePos.z + 1f),
                        new Vector3(templatePos.x, templatePos.y, templatePos.z + 2f),
                        new Vector3(templatePos.x, templatePos.y, templatePos.z - 1f),
                        new Vector3(templatePos.x, templatePos.y, templatePos.z - 2f),

                        //Get the 4 2x2 quadrants that are left

                        //Bottom left
                        new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z - 1f),
                        new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z - 2f),
                        new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z - 1f),
                        new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z - 2f),

                        //Top left
                        new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z + 1f),
                        new Vector3(templatePos.x - 1f, templatePos.y, templatePos.z + 2f),
                        new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z + 1f),
                        new Vector3(templatePos.x - 2f, templatePos.y, templatePos.z + 2f),

                        //Bottom right
                        new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z - 1f),
                        new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z - 2f),
                        new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z - 1f),
                        new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z - 2f),

                        //Top right
                        new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z + 1f),
                        new Vector3(templatePos.x + 1f, templatePos.y, templatePos.z + 2f),
                        new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z + 1f),
                        new Vector3(templatePos.x + 2f, templatePos.y, templatePos.z + 2f)
                    );

                    return occupiedTileCentres;
                #endregion
                default:
                    Debug.LogWarning("Invalid building width specified:" + building.width);
                    return null;
            }
        }
        else
        {
            return null;
        }
    }

    //Check if the building is going to be placed over any obstructions. If so, return true. If it can be placed, return false.
    private bool CheckIfInvalid(List<Vector3> occupiedTiles, bool canBePlacedOnWater = false)
    {
        Vector3 maxWorldBounds = new Vector3(terrainGenerator.worldSize - 0.5f, 0f, terrainGenerator.worldSize - 0.5f);

        for (int i = 0; i < occupiedTiles.Count; i++)
        {
            occupiedTiles[i] = new Vector3(occupiedTiles[i].x, 0f, occupiedTiles[i].z);

            //Debug.Log(occupiedTiles[i]);

            //Check if any tiles are out of world bounds
            if (occupiedTiles[i].x < 0.5f || occupiedTiles[i].z < 0.5f)
                return true;

            if (occupiedTiles[i].x > maxWorldBounds.x || occupiedTiles[i].z > maxWorldBounds.z)
                return true;

            var tileCentresMap = Environment.tileCentresMap;

            int tileX = Environment.tileCentresMap.Forward[occupiedTiles[i]].x;
            int tileY = Environment.tileCentresMap.Forward[occupiedTiles[i]].y;

            //Check if any tiles are over unbuildable/occupied tiles (e.g. water, resource, another building)
            if (!canBePlacedOnWater)
            {
                if (Environment.tileType[tileX, tileY] == "Water")
                    return true;
            }

            if (!Environment.walkable[tileX, tileY])
                return true;
        }
        return false;
    }

    private void SetTemplateColor(Color color)
    {
        templateBuildingMaterials = templateBuilding.GetComponent<Renderer>().sharedMaterials;

        newTemplateBuildingMaterials = templateBuildingMaterials;

        for (int i = 0; i < templateBuildingMaterials.Length; i++)
        {
            newTemplateBuildingMaterials[i].SetColor("_TintColor", color);
        }

        templateBuilding.GetComponent<Renderer>().sharedMaterials = newTemplateBuildingMaterials;
    }

    private void RestoreTemplateColor()
    {
        if (templateBuildingMaterials.Length > 0)
            templateBuilding.GetComponent<Renderer>().sharedMaterials = templateBuildingMaterials;
    }

    public void SetTemplate(GameObject _templateBuilding)
    {
        ClearTemplate();

        templateBuilding = Instantiate(_templateBuilding);

        //Removing the "(Clone)" part for added cleanliness and replacing it with "_Template"
        templateBuilding.name = templateBuilding.name.Remove(templateBuilding.name.Length - 7);
        templateBuilding.name = templateBuilding.name + "_Template";
    }

    public void ClearTemplate()
    {
        Destroy(templateBuilding);

        templateBuilding = null;
    }

    IEnumerator RotateTemplate(float _angle, float rotTime)
    {
        rotating = true;

        var angle = Vector3.up * _angle;

        var fromAngle = templateBuilding.transform.rotation;
        var toAngle = Quaternion.Euler(templateBuilding.transform.eulerAngles + angle);

        for (var t = 0f; t < 1; t += Time.deltaTime / rotTime)
        {
            templateBuilding.transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }

        templateBuilding.transform.rotation = toAngle;
        rotating = false;
    }
}
