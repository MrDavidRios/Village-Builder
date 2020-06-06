using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TileOperations;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    private static List<Villager> villagers = new List<Villager>();

    //On awake, subscribe to the Jobs.cs FinishJob event handler. Might as well give the villagers their indexes while we're at it. Oh, and before you do that, initialize the villager list.
    private void Awake()
    {
        int i = 0;

        foreach (Transform villager in transform)
        {
            villagers.Add(villager.GetComponent<Villager>());

            villager.GetComponent<Villager>().index = i;

            i++;
        }

        Jobs.JobFinished += Jobs_FinishJob;
        Jobs.JobGroupAssigned += Jobs_AssignJobGroups;
    }

    public Villager VillagerToAssignTo(string jobType) => villagers[JobUtils.VillagerToAssignTo(villagers, jobType)];

    public static void AssignJob(Job job, int villagerIndex, int jobIndex)
    {
        villagers[villagerIndex].lastAddedJob = job;

        villagers[villagerIndex].jobList.Insert(jobIndex, job);
    }

    public static void AssignJob(Job job, int villagerIndex)
    {
        villagers[villagerIndex].lastAddedJob = job;

        villagers[villagerIndex].jobList.Add(job);
    }

    public void AssignJob(Job job)
    {
        int villagerIndex = JobUtils.VillagerToAssignTo(villagers, job.jobType);

        villagers[villagerIndex].lastAddedJob = job;

        villagers[villagerIndex].jobList.Add(job);
    }

    public void AssignJob(Vector3 position, Transform[] objectiveTransforms, int[] amounts, string jobType, int villagerIndex)
    {
        var newJob = new Job();

        newJob.jobType = jobType;
        newJob.position = position;
        newJob.objectiveTransforms = objectiveTransforms;
        newJob.amounts = amounts;

        villagers[villagerIndex].lastAddedJob = newJob;

        villagers[villagerIndex].jobList.Add(newJob);
    }

    public void AssignJob(Vector3 position, Transform[] objectiveTransforms, int[] amounts, string jobType)
    {
        var newJob = new Job();

        newJob.jobType = jobType;
        newJob.position = position;
        newJob.objectiveTransforms = objectiveTransforms;
        newJob.amounts = amounts;

        int villagerIndex = JobUtils.VillagerToAssignTo(villagers, jobType);

        villagers[villagerIndex].lastAddedJob = newJob;

        villagers[villagerIndex].jobList.Add(newJob);
    }

    public void RemoveJob(int villagerIndex, int jobIndex = -1)
    {
        if (jobIndex == -1)
        {
            villagers[villagerIndex].lastRemovedJob = null;

            villagers[villagerIndex].jobList.Clear();
        }
        else
        {
            villagers[villagerIndex].lastRemovedJob = villagers[villagerIndex].jobList[jobIndex];

            villagers[villagerIndex].jobList.RemoveAt(jobIndex);
        }
    }

    //Once villager job is finished, remove the first element (completed job).
    private void Jobs_FinishJob(object sender, VillagerArgs e)
    {
        var villager = villagers[e.villagerIndex];

        if (villager.debugLevel == VillagerDebugLevels.Detailed)
            Debug.Log(villager.jobList[0].jobType + " job for " + e.villagerIndex + " finished!");

        RemoveJob(villager.index, 0);

        villager.performingJob = false;
    }

    private void Jobs_AssignJobGroups(object sender, AssignJobGroupArgs e) => AssignJobGroup(e.jobGroup, e.jobPosition, e.jobTransforms, e.amounts, e.villagerIndex);

    public void AssignJobGroup(string jobGroup, Vector3 jobPosition, Transform[] objectiveTransforms, int[] amounts = null, int villagerIndex = -1)
    {
        if (villagerIndex == -1)
            villagerIndex = VillagerToAssignTo(jobGroup).index;

        switch (jobGroup)
        {
            case "HarvestTree":
                Transform objectiveTransform = objectiveTransforms[0];

                objectiveTransform.GetComponent<Resource>().beingHarvested = true;

                objectiveTransform.GetComponent<Resource>().AddHarvestIndicator();

                jobPosition = objectiveTransform.position;

                AssignJob(jobPosition, objectiveTransforms, amounts, "Move", villagerIndex);
                AssignJob(jobPosition, objectiveTransforms, amounts, "Chop", villagerIndex);
                break;
            case "HarvestStone":
                break;
            //If the villager has too many items for one storage, deposit the items they can in that storage, and equally distribute items among the remaining storage spaces available.
            case "Deposit":
                AssignJob(jobPosition, objectiveTransforms, amounts, "Move", villagerIndex);
                AssignJob(jobPosition, new Transform[1] { objectiveTransforms[0] }, amounts, "Deposit", villagerIndex);
                break;
            case "PickUpPile":
                AssignJob(jobPosition, objectiveTransforms, amounts, "Move", villagerIndex);
                AssignJob(jobPosition, objectiveTransforms, amounts, "TakeFromItemPile", villagerIndex);

                //AssignJobGroup("Deposit", jobPosition, objectiveTransforms, new int[1] { objectiveTransforms[0].GetComponent<ItemPile>().amountOfItems }, villagerIndex);
                break;
            case "Build":
                AssignJob(jobPosition, objectiveTransforms, amounts, "Move", villagerIndex);
                AssignJob(jobPosition, objectiveTransforms, amounts, "Build", villagerIndex);
                break;
            default:
                Debug.LogError("Undefined job group: " + jobGroup);
                break;
        }
    }

    public void CancelSelectedJob()
    {
        GameObject selectedObject = Select.selectedObject;

        Villager selectedVillager = null;
        int jobIndex = -1;

        for (int i = 0; i < villagers.Count; i++)
        {
            for (int j = 0; j < villagers[i].jobList.Count; j++)
            {
                var matches = villagers[i].jobList.Any(job => job.objectiveTransforms[0] == selectedObject.transform);

                if (matches)
                {
                    selectedVillager = villagers[i];
                    jobIndex = j;
                }
            }
        }

        if (jobIndex != -1)
        {
            string jobType = selectedVillager.jobList[jobIndex].jobType;

            RemoveJob(selectedVillager.index, jobIndex);

            switch (jobType)
            {
                case "Move":
                    break;
                case "Chop":
                case "HarvestTree":
                    break;
                case "HarvestStone":
                case "Mine":
                    break;
                case "Build":
                    break;
                case "Deposit":
                    break;
                default:
                    break;
            }
        }
    }

    public void ChopSelectedTree()
    {
        if (!Select.selectedObject.GetComponent<Resource>().beingHarvested)
            AssignJobGroup("HarvestTree", Select.selectedObject.transform.position, new Transform[1] { Select.selectedObject.transform });
    }

    //Returns true if any job of any villager contains the selected transform.
    public bool JobContainsTransform(Transform selectedTransform)
    {
        for (int i = 0; i < villagers.Count; i++)
        {
            for (int j = 0; j < villagers[i].jobList.Count; j++)
            {
                if (villagers[i].jobList[j].objectiveTransforms.Contains(selectedTransform))
                    return true;
            }
        }

        return false;
    }
}
