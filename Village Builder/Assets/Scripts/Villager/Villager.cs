using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour
{
    //Identification
    public int index;

    public string _name;

    public string _gender;

    //Inventory
    public bool inventoryFull;

    public int inventoryCapacity;

    public List<Item> items = new List<Item>();

    //Jobs
    public List<Job> jobList = new List<Job>();

    [SerializeField] private int jobAmount;

    public bool performingJob = false;

    public string unfinishedJob;

    //Stats
    public int harvestAmount;
    public int harvestRate;

    private void Awake()
    {
        Jobs.InventoryUpdated += Jobs_DisplayItems;

        _gender = VillagerPropertiesGenerator.GenerateGender(this);
        _name = VillagerPropertiesGenerator.GenerateName(this);
    }

    private void Update()
    {
        #region Jobs
        jobAmount = jobList.Count;

        //If there's a job to do and the villager currently isn't doing a job, start a new one.
        bool hasJob = jobAmount > 0;

        if (hasJob && !performingJob)
            StartJob();
        #endregion

        #region Inventory
        if (items.Count == inventoryCapacity)
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
                StartCoroutine(Jobs.ChopTree(this, job.objectiveTransform));
                break;
            case "Mine":
                break;
            case "Construct":
                break;
            case "Deposit":
                StartCoroutine(Jobs.Deposit(this, job.objectiveTransform));
                break;
            default:
                Debug.LogError("Invalid job type: " + job.jobType);
                break;
        }
    }

    //Update items being shown every time the Jobs.InventoryUpdated event fires off.
    private void Jobs_DisplayItems(object sender, VillagerArgs e)
    {
        if (e.villagerIndex != index)
            return;

        Debug.Log("Villager " + index + " inventory item amount: " + items.Count);

        GameObject itemObject = Instantiate(Resources.Load("Prefabs/Items/Log"), transform) as GameObject;

        var objectPosition = new Vector3(-0.7f, 0.15f + (0.22f * items.Count), 0f);

        itemObject.transform.localPosition = objectPosition;

        itemObject.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

        itemObject.transform.localScale = new Vector3(
            itemObject.transform.localScale.x * (1f / transform.localScale.x),
            itemObject.transform.localScale.y * (1f / transform.localScale.y),
            itemObject.transform.localScale.z * (1f / transform.localScale.z));
    }
}
