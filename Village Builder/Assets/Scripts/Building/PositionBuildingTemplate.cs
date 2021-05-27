using System.Collections;
using System.Collections.Generic;
using DavidRios.Building.Building_Types;
using DavidRios.Input;
using DavidRios.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DavidRios.Building
{
    public class PositionBuildingTemplate : MonoBehaviour
    {
        //Booleans
        public static bool PlacingBuilding;

        public static bool Rotating = false;

        //GameObjects
        public static GameObject TemplateBuilding;

        //Hotkeys
        public KeyCode positionLockKey;
        
        //Building Objects
        private Building _building;
        private Vector3 _cachedBuildingPos = Vector3.zero;

        //Quaternions
        private Quaternion _cachedBuildingRot;
        private CheckTiles _checkTiles;

        //Scripts
        private PlacementGrid _grid;
        
        //Input
        private PlayerController.DefaultActions _input;

        //Raycast
        private RaycastHit _hit;

        private bool _isSqaure;

        //Camera
        [SerializeField] private UnityEngine.Camera mainCamera;

        private bool _placeable, _placingBuilding, _positionLocked;
        private Ray _ray;

        private TemplateActions _templateActions;

        //Vector3s
        public List<Vector3> OccupiedTileCentres { get; private set; }
    
        private Vector3 _templatePos;

        //LayerMasks
        public LayerMask terrainLayer;

        private void Awake()
        {
            OccupiedTileCentres = new List<Vector3>();
            
            _grid = FindObjectOfType<PlacementGrid>();
            
            _templateActions = GetComponent<TemplateActions>();
            _checkTiles = GetComponent<CheckTiles>();
        }

        private void Start() => _input = InputHandler.PlayerControllerInstance.Default;

        private void Update()
        {
            if (!TemplateBuilding)
                _positionLocked = false;

            if (CanBeginPlacingBuilding())
            {
                _placingBuilding = true;

                //Only fire a Raycast if the mouse is over ground
                _ray = mainCamera.ScreenPointToRay(InputHandler.MousePosition);

                if (Physics.Raycast(_ray, out _hit, GlobalRules._maxRaycastDistance, terrainLayer))
                {
                    _building = BuildingOperations.GetBuildingScriptableObject(TemplateBuilding.transform);

                    //Is the building square? (is its width equal its length)
                    _isSqaure = _building.width == _building.length;

                    if (_isSqaure && Numbers.IsEven(_building.width))
                        _grid.centerInTiles = false;
                    else
                        _grid.centerInTiles = true;

                    var occupiedTiles =
                        _checkTiles.GetOccupiedTileCentres(_building, _cachedBuildingPos, _cachedBuildingRot);

                    _placeable = !_checkTiles.CheckIfInvalid(occupiedTiles);

                    //This has to be done precisely after the 'GetOccupiedTileCentres' function so that the list doesn't add duplicate members.
                    _cachedBuildingPos = TemplateBuilding.transform.position;
                    _cachedBuildingRot = TemplateBuilding.transform.rotation;

                    #region Non-Square buildings

                    //If the building's width is odd
                    if (!_isSqaure && Numbers.IsEven(_building.width))
                    {
                        Debug.LogError("Remember to program non-square buildings!");

                        if (_building.length % 2 != 0)
                        {
                        }
                    }
                    //If the building's width is even
                    else if (!_isSqaure)
                    {
                        Debug.LogError("Remember to program non-square buildings!");

                        if (Numbers.IsEven(_building.length))
                        {
                        }
                    }

                    #endregion

                    _templatePos = _grid.GetNearestPointOnGrid(_hit.point);

                    _templatePos += TemplateBuilding.GetComponent<BuildingTemplate>().positionOffset;

                    TemplateBuilding.transform.position = _templatePos;

                    TemplateActions.SetTemplateColor(_placeable ? Color.green : Color.red);

                    TemplateBuilding.SetActive(true);
                }
                else
                {
                    //Since the mouse isn't over empty ground, the building template will be made invisible.
                    TemplateBuilding.SetActive(false);

                    _placingBuilding = false;
                }

                if (!Rotating)
                    TransformTemplate();
            }
            else if (!_positionLocked)
            {
                _placingBuilding = false;
            }

            if (InputHandler.Pressed(_input.LockBuilding))
                _positionLocked = !_positionLocked;

            PlacingBuilding = _placingBuilding;
        }

        private bool CursorHoveringOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        private bool CanBeginPlacingBuilding()
        {
            //Return true if the template exists, the cursor isn't currently over UI, and if there are enough resources to construct the building. 
            return TemplateBuilding && !CursorHoveringOverUI() &&
                   StorageManager.EnoughResources(TemplateBuilding.GetComponent<UnderConstruction>().requiredItems);
        }

        private void TransformTemplate()
        {
            if (InputHandler.Pressed(_input.LeftClick) && _placeable)
            {
                _checkTiles.GetOccupiedTileCentres(_building, _cachedBuildingPos, _cachedBuildingRot);
                _templateActions.PlaceTemplate(_templatePos, TemplateBuilding.transform.localEulerAngles,
                    OccupiedTileCentres);
            }

            if (InputHandler.Pressed(_input.ResetRotate) && !Rotating)
                StartCoroutine(TemplateActions.RotateTemplate(90f, 0.5f));

            if (InputHandler.ScrollValue > 0)
                StartCoroutine(TemplateActions.RotateTemplate(90f, 0.5f));

            if (InputHandler.ScrollValue < 0)
                StartCoroutine(TemplateActions.RotateTemplate(-90f, 0.5f));
        }

        public static IEnumerator RemoveTemplate(Transform buildingTemplate, Button clickedButton = null)
        {
            List<Vector2> occupiedTiles = null;

            occupiedTiles = BuildingOperations.GetBuildingScript(buildingTemplate.transform)._occupiedIndices;

            Destroy(buildingTemplate.gameObject);

            //Set all occupied tiles as walkable now that the building is no longer there
            for (var i = 0; i < occupiedTiles.Count; i++)
            {
                Environment.walkable[(int) occupiedTiles[i].x, (int) occupiedTiles[i].y] = true;
                Environment.buildingPlaced[(int) occupiedTiles[i].x, (int) occupiedTiles[i].y] = false;
                Environment.ModifyWalkableTiles((int) occupiedTiles[i].x, (int) occupiedTiles[i].y);
            }

            //If this is called from a button, then remove all of its listeners.
            if (clickedButton != null)
            {
                clickedButton.onClick.RemoveAllListeners();

                //Remove villager jobs & job list elements
                var jobManager = FindObjectOfType<JobManager>();

                //Find villager who will construct building
                var villager = JobUtils.ReturnVillagerFromJobTransform(JobManager.villagers, buildingTemplate);

                if (villager == null)
                    yield break;

                //Get villager's building job and remove it along with its supporting jobs
                for (var i = 0; i < villager.jobList.Count; i++)
                    if (villager.jobList[i].objectiveTransforms[0] == buildingTemplate)
                    {
                        if (villager.performingJob)
                        {
                            if (villager.items.Count == 0)
                            {
                                //Currently withdrawing.
                                if (villager.jobList[0].jobType == "Withdraw")
                                {
                                    //If currently withdrawing, finish withdrawal, clear all jobs, and deposit.
                                    Debug.Log("-1-");

                                    jobManager.RemoveJob(villager._index, i);
                                    villager.moveJobCancelled = true;

                                    jobManager.RemoveJob(villager._index, i);

                                    jobManager.RemoveJob(villager._index, i);
                                    villager.moveJobCancelled = true;

                                    jobManager.RemoveJob(villager._index, i);

                                    //Wait until job count is equal to 0 and continue.
                                    yield return new WaitUntil(() => villager.jobList.Count == 0);

                                    var storagesToUse = JobUtils.GetNearestAvailableStorages(villager,
                                        villager.items[0].itemType, villager.items.Count);

                                    for (var j = 0; j < storagesToUse.Length; j++)
                                    {
                                        storagesToUse[j].GetComponent<Storage>()
                                            .QueueItemsForDeposit(
                                                new ItemBundle
                                                    {item = villager.items[0], amount = villager.items.Count},
                                                villager._index);

                                        villager.moveJobCancelled = false;

                                        jobManager.AssignJobGroup("Deposit", storagesToUse[j].transform.position,
                                            new Transform[1] {storagesToUse[j]}, new int[1] {villager.items.Count},
                                            villager._index);
                                    }
                                }
                                else
                                {
                                    var testJobList = villager.jobList;

                                    Debug.Log("-2-" + villager.jobList[0].jobType);

                                    //Wait until job is withdraw, and then wait until job is move. After this, cancel all jobs and then deposit.

                                    yield return new WaitUntil(() => villager.jobList[0].jobType == "Withdraw");
                                    yield return new WaitUntil(() => villager.jobList[0].jobType == "Move");

                                    jobManager.RemoveJob(villager._index);

                                    Debug.Log("here.");
                                }
                            }
                            else
                            {
                                Debug.Log("-3-" + villager.jobList[0].jobType);

                                if (villager.jobList[0].jobType == "Build")
                                    yield break;

                                var jobList = villager.jobList;
                                //Remove 'Move' Job
                                jobManager.RemoveJob(villager._index, i);
                                villager.moveJobCancelled = true;

                                //Remove 'DepositToBuildSite' Job
                                jobManager.RemoveJob(villager._index, i);

                                //Remove 'Move' Job
                                jobManager.RemoveJob(villager._index, i);
                                villager.moveJobCancelled = true;

                                //Add new jobs that deposit items that are currently in hand to storage
                                var storagesToUse = JobUtils.GetNearestAvailableStorages(villager,
                                    villager.items[0].itemType, villager.items.Count);

                                for (var j = 0; j < storagesToUse.Length; j++)
                                {
                                    storagesToUse[j].GetComponent<Storage>().QueueItemsForDeposit(
                                        new ItemBundle {item = villager.items[0], amount = villager.items.Count},
                                        villager._index);

                                    villager.moveJobCancelled = false;

                                    jobManager.AssignJobGroup("Deposit", storagesToUse[j].transform.position,
                                        new Transform[1] {storagesToUse[j]}, new int[1] {villager.items.Count},
                                        villager._index);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("-4-");
                            jobManager.RemoveJob(villager._index, i);
                            jobManager.RemoveJob(villager._index, i - 1);
                        }

                        break;
                    }
            }
        }
    }
}