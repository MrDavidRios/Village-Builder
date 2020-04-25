using System;
using System.Collections;
using System.Collections.Generic;
using TileOperations;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    private List<Villager> villagers = new List<Villager>();

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

    public void AssignJob(Job job, int villagerIndex) => villagers[villagerIndex].jobList.Add(job);

    public void AssignJob(Job job) => villagers[JobUtils.VillagerToAssignTo(villagers, job.jobType)].jobList.Add(job);

    public void AssignJob(Vector3 position, Transform objectiveTransform, int amount, string jobType, int villagerIndex)
    {
        var newJob = new Job();

        newJob.jobType = jobType;
        newJob.position = position;
        newJob.objectiveTransform = objectiveTransform;
        newJob.amount = amount;

        villagers[villagerIndex].jobList.Add(newJob);
    }

    public void AssignJob(Vector3 position, Transform objectiveTransform, int amount, string jobType)
    {
        var newJob = new Job();

        newJob.jobType = jobType;
        newJob.position = position;
        newJob.objectiveTransform = objectiveTransform;
        newJob.amount = amount;

        villagers[JobUtils.VillagerToAssignTo(villagers, newJob.jobType)].jobList.Add(newJob);
    }

    public void RemoveJob(int villagerIndex, int jobIndex = -1)
    {
        if (jobIndex == -1)
            villagers[villagerIndex].jobList.Clear();
        else
            villagers[villagerIndex].jobList.RemoveAt(jobIndex);
    }

    //Once villager job is finished, remove the first element (completed job).
    private void Jobs_FinishJob(object sender, VillagerArgs e)
    {
        var villagerJobList = villagers[e.villagerIndex].jobList;

        Debug.Log(villagerJobList[0].jobType + " job for " + e.villagerIndex + " finished!");

        villagerJobList.RemoveAt(0);

        villagers[e.villagerIndex].performingJob = false;
    }

    private void Jobs_AssignJobGroups(object sender, AssignJobGroupArgs e) => AssignJobGroup(e.jobGroup, e.jobPosition, e.jobTransform, e.villagerIndex);

    public void AssignJobGroup(string jobGroup, Vector3 jobPosition, Transform objectiveTransform, int villagerIndex)
    {
        switch (jobGroup)
        {
            case "HarvestTree":
                objectiveTransform.GetComponent<Resource>().beingHarvested = true;

                objectiveTransform.GetComponent<Resource>().AddHarvestIndicator();

                var nearestStorage = JobUtils.NearestStorage(jobPosition);
                jobPosition = objectiveTransform.position;

                AssignJob(jobPosition, objectiveTransform, 0, "Move", villagerIndex);
                AssignJob(jobPosition, objectiveTransform, 0, "Chop", villagerIndex);
                //AssignJob(nearestStorage.position, objectiveTransform, 0, "Move", villagerIndex);
                //AssignJob(nearestStorage.position, objectiveTransform, 0, "Deposit", villagerIndex);
                break;
            case "HarvestStone":
                break;
            default:
                Debug.LogError("Undefined job group: " + jobGroup);
                break;
        }
    }

    public void ChopSelectedTree()
    {
        if (!SelectTile.selectedObject.GetComponent<Resource>().beingHarvested)
            AssignJobGroup("HarvestTree", SelectTile.selectedObject.transform.position, SelectTile.selectedObject.transform, VillagerToAssignTo("Chop").index);
    }
}
