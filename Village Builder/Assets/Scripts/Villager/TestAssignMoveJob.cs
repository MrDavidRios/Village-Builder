using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAssignMoveJob : MonoBehaviour
{
    public void AssignMoveJob(Transform newPosition)
    {
        FindObjectOfType<JobManager>().AssignJob(new Job { jobType = "Move", position = newPosition.position, amount = 0 });
    }

    public void AssignTreeChoppingJob()
    {
        var villager = FindObjectOfType<JobManager>().VillagerToAssignTo();
        var nearestTreeTransform = JobUtils.NearestTree(villager.transform);

        FindObjectOfType<JobManager>().AssignJobGroup("HarvestTree", Vector3.zero, nearestTreeTransform, villager.index);
    }
}
