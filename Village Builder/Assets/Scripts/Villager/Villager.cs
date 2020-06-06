using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using TileOperations;
using UnityEngine;

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

public class Villager : MonoBehaviour
{
    //Identification
    [Header("Identification")]
    public int index;

    public string _name;

    public string _sex;

    [LabelOverride("Role")]
    public VillagerRoles roleToAssign;

    [HideInInspector]
    public string _role;

    //Inventory
    [Header("Inventory")]
    public bool inventoryFull;

    public int numberOfItems;

    public int inventoryCapacity;

    public List<Item> items = new List<Item>();

    //Jobs
    [Header("Jobs")]
    public bool performingJob = false;
    public bool moveJobCancelled = false;
    public bool currentlyDepositing = false;

    public List<Job> jobList = new List<Job>();

    public Job lastAddedJob;
    public Job lastRemovedJob;

    [SerializeField] private int jobAmount;

    public string unfinishedJob;

    //Stats
    [Header("Stats")]
    public int _harvestAmount;
    public int _harvestRate;

    public int _buildAmount;
    public int _buildRate;

    public int _itemExchangeRate;

    public float _walkSpeed;
    [SerializeField] private float _currentWalkSpeed;

    //Scripts
    private DisplayJobsList displayJobsList;

    //Debug
    [Header("Debug")]
    public VillagerDebugLevels debugLevel;

    private void Awake()
    {
        displayJobsList = FindObjectOfType<DisplayJobsList>();

        Jobs.InventoryUpdated += Jobs_DisplayItems;

        _sex = VillagerPropertiesGenerator.GenerateSex(this);
        _name = VillagerPropertiesGenerator.GenerateName(this);
        _role = VillagerPropertiesGenerator.ProcessRole((int)roleToAssign);

        GetComponent<AIPath>().maxSpeed = _walkSpeed;

        _currentWalkSpeed = _walkSpeed;
    }

    private void Update()
    {
        #region Jobs
        if (jobAmount != jobList.Count)
        {
            if (jobAmount > jobList.Count)
                StartCoroutine(displayJobsList.DisplayVillagerJobs("Villager.cs", lastRemovedJob));
            else
                StartCoroutine(displayJobsList.DisplayVillagerJobs("Villager.cs", lastAddedJob));
        }

        jobAmount = jobList.Count;

        float inventoryFillPercentage = (float)items.Count / (float)inventoryCapacity;

        _currentWalkSpeed = _walkSpeed * (1.50f - inventoryFillPercentage);

        GetComponent<AIPath>().maxSpeed = _currentWalkSpeed;

        //If there's a job to do and the villager currently isn't doing a job, start a new one.
        bool hasJob = jobAmount > 0;

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

        Job job = jobList[0];

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
                //if (!currentlyDepositing)
                StartCoroutine(Jobs.Deposit(this, job.objectiveTransforms, job.amounts[0]));
                break;
            case "Withdraw":
                StartCoroutine(Jobs.Withdraw(this, job.amounts[0]));
                break;
            case "TakeFromItemPile":
                StartCoroutine(Jobs.TakeFromItemPile(this, job.objectiveTransforms[0], job.amounts[0]));
                break;
            default:
                Debug.LogError("Invalid job type: " + job.jobType);
                break;
        }
    }

    //Update items being shown every time the Jobs.InventoryUpdated event fires off.
    private void Jobs_DisplayItems(object sender, VillagerArgs e)
    {
        numberOfItems = items.Count;

        if (e.villagerIndex != index)
            return;

        if (debugLevel == VillagerDebugLevels.OverlyDetailed)
            Debug.Log("Villager " + index + " inventory item amount: " + numberOfItems);

        bool rightArmHold = numberOfItems % 2 == 0;

        GameObject itemObject = Instantiate(Resources.Load("Prefabs/Items/Log"), transform) as GameObject;

        Vector3 objectPosition;

        if (rightArmHold)
            objectPosition = new Vector3(-0.7f, 0.15f + (0.22f * (numberOfItems / 2)), 0f);
        else
        {
            float numberOfItemsCalc = numberOfItems + 1;
            objectPosition = new Vector3(0.7f, 0.15f + (0.22f * (numberOfItemsCalc / 2)), 0f);
        }

        itemObject.transform.localPosition = objectPosition;

        itemObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

        itemObject.transform.localScale = new Vector3(
            itemObject.transform.localScale.x * (1f / transform.localScale.x),
            itemObject.transform.localScale.y * (1f / transform.localScale.y),
            itemObject.transform.localScale.z * (1f / transform.localScale.z));
    }
}
