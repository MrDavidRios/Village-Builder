using System.Collections;
using System.Collections.Generic;
using DavidRios.Building.Building_Types;
using UnityEngine;

namespace DavidRios.Building
{
    public class UnderConstruction : MonoBehaviour
    {
        public float laborAmount;
        public float laborRequired;

        public int currentStage;
        public int stageAmount;

        public string[] stageDirectories;
        public StageInfo[] stageSettings;

        //Items needed for building
        public ItemBundle[] requiredItems;

        //Items currently on build site
        public ItemBundle[] itemsOnSite;

        //Does the building have enough items to be complete?
        public bool sufficientItems;

        public bool grassCleared;

        /// <summary>
        ///     Is a villager depositing to the build site?
        /// </summary>
        public bool beingDeposited;

        [HideInInspector] public bool currentlyProgressing;
        public bool resetPosition;

        public Building buildingInfo;

        private GameObject constructionSitePrefab;
        private Vector3 initialScale;

        private JobManager jobManager;
        private GameObject[] stages;

        private Vector3 templatePositionOffset;

        private void Awake()
        {
            currentlyProgressing = false;
            sufficientItems = false;
            grassCleared = false;
            beingDeposited = false;

            jobManager = FindObjectOfType<JobManager>();
            constructionSitePrefab = Resources.Load<GameObject>("Prefabs/Buildings/Construction_Site");

            stages = new GameObject[stageDirectories.Length];

            stageAmount = stages.Length - 1;

            //0 stage is construction site stage
            currentStage = 0;

            buildingInfo = BuildingOperations.GetBuildingScriptableObject(transform);

            for (var i = 0; i < stageDirectories.Length; i++)
                stages[i] = Resources.Load<GameObject>(stageDirectories[i]);

            initialScale = stageSettings[0].scale;

            templatePositionOffset = GetComponent<BuildingTemplate>().positionOffset;
        }

        private void Update()
        {
            if (GetComponent<BuildingTemplate>() == null)
            {
                var amountOfItemsSufficient = 0;

                for (var i = 0; i < requiredItems.Length; i++)
                    if (itemsOnSite[i].amount == requiredItems[i].amount)
                        amountOfItemsSufficient++;

                if (amountOfItemsSufficient == requiredItems.Length)
                    sufficientItems = true;
                else
                    sufficientItems = false;
            }
            else
            {
                sufficientItems = false;
            }
        }

        //Places temporary template of building before the actual construction occurs. This is so that the user can better visualize their city.
        public void PlaceTemplate()
        {
            var occupiedTiles = BuildingOperations.GetBuildingScript(transform)._occupiedIndices;

            for (var i = 0; i < occupiedTiles.Count; i++)
                Environment.buildingPlaced[(int) occupiedTiles[i].x, (int) occupiedTiles[i].y] = true;

            var cloneMaterial = Instantiate(GetComponent<Renderer>().sharedMaterials[0]);

            for (var i = 0; i < GetComponent<Renderer>().materials.Length; i++)
                GetComponent<Renderer>().materials[i] = cloneMaterial;

            GetComponent<Renderer>().material = cloneMaterial;

            Destroy(GetComponent<BuildingTemplate>());

            StartCoroutine(AssignBuildingJobs());
        }

        private IEnumerator AssignBuildingJobs()
        {
            var firstVillagerIndex = 0;

            for (var i = 0; i < requiredItems.Length; i++)
            {
                var itemType = requiredItems[i].item.itemType;

                //CityInfo.ReserveResourcesForBuilding(ItemInfo.GetItemIndex(itemType), requiredItems[i].amount);

                //Assign one villager per item type. If item amount is greater than two times that villager can handle, assign another, etc.
                var villagersNeededForResource = new List<Villager>();
                var villagerWithdrawAmount = new List<int>();

                //Amount of resources left to bring to the build site from villagers (this is not a literal value, just used in calculations to assign the correct number of villagers).
                var resourceAmountLeft = requiredItems[i].amount;

                //Get villagers to assign to (least busy)
                while (resourceAmountLeft > 0)
                {
                    var villagerIndex = JobUtils.VillagerToAssignTo(JobManager.villagers, "Withdraw");
                    var selectedVillager = JobManager.villagers[villagerIndex];

                    villagersNeededForResource.Add(selectedVillager);

                    if (resourceAmountLeft <= selectedVillager.inventoryCapacity * 2)
                    {
                        villagerWithdrawAmount.Add(resourceAmountLeft);
                        resourceAmountLeft = 0;
                    }
                    else
                    {
                        villagerWithdrawAmount.Add(selectedVillager.inventoryCapacity * 2);
                        resourceAmountLeft -= selectedVillager.inventoryCapacity * 2;
                    }

                    yield return null;
                }

                for (var j = 0; j < villagersNeededForResource.Count; j++)
                {
                    var villager = villagersNeededForResource[j];

                    if (j == 0)
                        firstVillagerIndex = villagersNeededForResource[0]._index;

                    //Debug.Log($"{villager._name} (Index #{villager._index}) will handle the resources needed for this build.");

                    //Amount of trips villager has to make (if the villager has to withdraw more than their inventory capacity, they have to make two trips. If not, they only have to make one.
                    var numberOfTrips = villagerWithdrawAmount[j] <= villager.inventoryCapacity ? 1 : 2;

                    //Debug.Log($"Number of trips: {numberOfTrips}");

                    for (var k = 0; k < numberOfTrips; k++)
                    {
                        int amountToWithdrawThisTrip;

                        if (numberOfTrips == 1)
                        {
                            amountToWithdrawThisTrip = villagerWithdrawAmount[j];
                        }
                        else
                        {
                            if (k == 0)
                                amountToWithdrawThisTrip = villager.inventoryCapacity;
                            else
                                amountToWithdrawThisTrip = villagerWithdrawAmount[j] - villager.inventoryCapacity;
                        }

                        var storagesToWithdrawFrom =
                            JobUtils.GetNearestAvailableStorages(villager, itemType, amountToWithdrawThisTrip, false);

                        for (var l = 0; l < storagesToWithdrawFrom.Length; l++)
                        {
                            //Debug.Log($"Amount to withdraw: {amountToWithdrawThisTrip}; Trip #{k + 1}");

                            storagesToWithdrawFrom[l].GetComponent<Storage>().QueueItemsForWithdrawal(
                                new ItemBundle
                                    {item = new Item {itemType = itemType}, amount = amountToWithdrawThisTrip},
                                villager._index);

                            jobManager.AssignJobGroup("Withdraw", storagesToWithdrawFrom[l].position,
                                new Transform[1] {storagesToWithdrawFrom[l]}, new int[1] {amountToWithdrawThisTrip},
                                villager._index);
                        }

                        jobManager.AssignJobGroup("GetResourcesToBuildSite", transform.position,
                            new Transform[1] {transform}, new int[1] {villagerWithdrawAmount[j]}, villager._index);
                    }
                }

                CityInfo.RemoveResource(ItemInfo.GetItemIndex(itemType), requiredItems[i].amount);
            }

            //Add building job
            if (requiredItems.Length == 0)
                jobManager.AssignJobGroup("Build", transform.position, new Transform[1] {transform});
            else
                jobManager.AssignJobGroup("Build", transform.position, new Transform[1] {transform}, null,
                    firstVillagerIndex);

            //Configure this!
        }

        public void InitializeConstructionSite()
        {
            if (resetPosition)
                transform.position -= templatePositionOffset;

            UpdateBuildingAppearance(0);
        }

        public void ClearGrass(float laborCoefficient, Vector3 endScale)
        {
            var laborNeeded = Mathf.Pow(buildingInfo.width, 2) * 10f;

            laborAmount += laborCoefficient * Time.deltaTime;

            var percentageComplete = laborAmount / laborNeeded;

            transform.localScale = Vector3.Lerp(initialScale, endScale, percentageComplete);

            //Debug.Log("Labor Amount: " + laborAmount + "; Labor Required: " + laborRequired + "; Percentage Complete: " + percentageComplete);

            if (percentageComplete >= 1f)
            {
                laborAmount = 0;

                currentlyProgressing = true;
                grassCleared = true;

                if (!BuildingOperations.GetBuildingScriptableObject(transform).walkable)
                {
                    //Make tiles unwalkable
                    var occupiedTiles = BuildingOperations.GetBuildingScript(transform)._occupiedIndices;

                    for (var i = 0; i < occupiedTiles.Count; i++)
                    {
                        Environment.walkable[(int) occupiedTiles[i].x, (int) occupiedTiles[i].y] = false;
                        Environment.ModifyWalkableTiles((int) occupiedTiles[i].x, (int) occupiedTiles[i].y, false);
                    }
                }
            }
        }

        public void WorkOnBuilding(float laborCoefficient)
        {
            laborAmount += laborCoefficient;

            var nextStage = currentStage + 1;

            if (laborAmount > laborRequired)
                laborAmount = laborRequired;

            //If labor amount is greater than or equal to what is required for the next stage, then update the building's appearance.
            if (laborAmount >= laborRequired / stageAmount * nextStage)
                UpdateBuildingAppearance(nextStage);
        }

        public void UpdateBuildingAppearance(int stage)
        {
            currentStage = stage;

            GetComponent<MeshFilter>().sharedMesh = stages[stage].GetComponent<MeshFilter>().sharedMesh;
            GetComponent<MeshRenderer>().materials = stages[stage].GetComponent<MeshRenderer>().sharedMaterials;

            transform.position += stageSettings[stage].positionOffset;
            transform.eulerAngles += stageSettings[stage].rotationOffset;

            if (stageSettings[stage].scale != Vector3.zero)
                transform.localScale = stageSettings[stage].scale;

            if (stage > 0)
                GetComponent<BoxCollider>().size = Vector3.one;

            if (stage == stageAmount && stageAmount > 0)
            {
                DestroyScript();
            }
        }

        public ItemBundle[] ReturnRequiredResources()
        {
            return buildingInfo.requiredResources;
        }

        public void DestroyScript()
        {
            Destroy(GetComponent<UnderConstruction>());
        }
    }
}