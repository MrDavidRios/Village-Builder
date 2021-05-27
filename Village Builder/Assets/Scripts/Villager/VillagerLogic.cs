using System.Collections.Generic;
using DavidRios.Assets.Scripts.Villager;
using DavidRios.UI;
using Pathfinding;
using Unity.Collections;
using UnityEngine;

namespace DavidRios.Villager
{
    public enum VillagerRoles
    {
        Laborer = 1,
        Carpenter,
        Forester,
        Blacksmith,
        Baker,
        Stonemason
    }

    public enum VillagerDebugLevels
    {
        None,
        Basic,
        Detailed,
        OverlyDetailed
    }

    public class VillagerLogic : MonoBehaviour
    {
        //Identification
        [Header("Identification")] public int index;

        public new string name;

        public string sex;

        [LabelOverride("Role")] public VillagerRoles roleToAssign;

        [HideInInspector] public string role;

        //Inventory
        [Header("Inventory")] public bool inventoryFull;

        public int numberOfItems;

        public int inventoryCapacity;

        public List<Item> items = new List<Item>();

        //Jobs
        [Header("Jobs")] public bool performingJob;

        public bool moveJobCancelled;

        public List<Job> jobList = new List<Job>();

        public Job lastAddedJob;
        public Job lastRemovedJob;

        [SerializeField] private int jobAmount;

        public string customJobDescription;

        //Stats
        [Header("Stats")] 
        
        [Tooltip("The amount of items the villager collects in each harvest action.")] public int harvestAmount;

        [Tooltip("The amount of seconds the villager takes between each harvest action.")] public int harvestRate;

        [Tooltip("The amount of building the villager accomplishes in each build action.")] public int buildAmount;
        [Tooltip("The amount of seconds the villager takes between each build action.")]  public int buildRate;

        [Tooltip("The amount of seconds the villager takes between each item exchange.")] public int itemExchangeRate;

        public float walkSpeed;
        [SerializeField] private float currentWalkSpeed;

        //Debug
        [Header("Debug")] public VillagerDebugLevels debugLevel;

        //Scripts
        private DisplayJobsList _displayJobsList;

        private void Awake()
        {
            _displayJobsList = FindObjectOfType<DisplayJobsList>();

            sex = VillagerPropertiesGenerator.GenerateSex(this);
            name = VillagerPropertiesGenerator.GenerateName(this);
            role = VillagerPropertiesGenerator.ProcessRole((int) roleToAssign);

            GetComponent<AIPath>().maxSpeed = walkSpeed;

            currentWalkSpeed = walkSpeed;
        }

        private void Update()
        {
            #region Jobs

            if (jobAmount != jobList.Count)
            {
                StartCoroutine(jobAmount > jobList.Count
                    ? _displayJobsList.DisplayVillagerJobs("VillagerLogic.cs", lastRemovedJob, this)
                    : _displayJobsList.DisplayVillagerJobs("VillagerLogic.cs", lastAddedJob, this));
            }

            jobAmount = jobList.Count;

            var inventoryFillPercentage = items.Count / (float) inventoryCapacity;

            currentWalkSpeed = walkSpeed * (1.50f - inventoryFillPercentage);

            GetComponent<AIPath>().maxSpeed = currentWalkSpeed;

            //If there's a job to do and the villager currently isn't doing a job, start a new one.
            var hasJob = jobAmount > 0;

            if (hasJob && !performingJob && Time.timeScale != 0)
                StartJob();

            #endregion

            #region Inventory

            numberOfItems = items.Count;

            if (numberOfItems == inventoryCapacity)
                inventoryFull = true;
            else
                inventoryFull = false;

            #endregion
        }

        //Use delegates for this, where only the 'job' class is passed through every function. Only the stored function changes?
        private void StartJob()
        {
            performingJob = true;

            var job = jobList[0];

            switch (job.jobType)
            {
                case "Move":
                    StartCoroutine(Jobs.Move(transform, job.position));
                    break;
                case "Chop":
                    StartCoroutine(Jobs.ChopTree(this, job.objectiveTransforms[0]));
                    break;
                case "Mine":
                    break;
                case "Build":
                    StartCoroutine(Jobs.Build(this, job.objectiveTransforms[0]));
                    break;
                case "Deposit":
                    StartCoroutine(Jobs.Deposit(this, job.objectiveTransforms, job.amounts[0]));
                    break;
                case "Withdraw":
                    StartCoroutine(Jobs.Withdraw(this, job.objectiveTransforms, job.amounts[0]));
                    break;
                case "DepositToBuildSite":
                    StartCoroutine(Jobs.DepositToBuildSite(this, job.objectiveTransforms[0], job.amounts[0]));
                    break;
                case "TakeFromItemPile":
                    StartCoroutine(Jobs.TakeFromItemPile(this, job.objectiveTransforms[0], job.amounts[0]));
                    break;
                default:
                    Debug.LogError("Invalid job type: " + job.jobType);
                    break;
            }
        }

        /// <summary>
        ///     Updates items being shown (one at a time) every time it is called.
        /// </summary>
        public void DisplayItems()
        {
            numberOfItems = items.Count;

            if (debugLevel == VillagerDebugLevels.OverlyDetailed)
                Debug.Log("Villager " + index + " inventory item amount: " + numberOfItems);

            var rightArmHold = numberOfItems % 2 == 0;

            var itemObject = Instantiate(Resources.Load("Prefabs/Items/Log"), transform) as GameObject;

            Vector3 objectPosition;

            if (rightArmHold)
            {
                objectPosition = new Vector3(-0.7f, 0.15f + 0.22f * (numberOfItems / 2), 0f);
            }
            else
            {
                float numberOfItemsCalc = numberOfItems + 1;
                objectPosition = new Vector3(0.7f, 0.15f + 0.22f * (numberOfItemsCalc / 2), 0f);
            }

            itemObject.transform.localPosition = objectPosition;

            itemObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

            itemObject.transform.localScale = new Vector3(
                itemObject.transform.localScale.x * (1f / transform.localScale.x),
                itemObject.transform.localScale.y * (1f / transform.localScale.y),
                itemObject.transform.localScale.z * (1f / transform.localScale.z));
        }

        public void ResetCustomJobDescription()
        {
            customJobDescription = "";
        }
    }
}