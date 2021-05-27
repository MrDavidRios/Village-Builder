using System.Collections.Generic;
using System.Linq;
using DavidRios.Villager;
using TileOperations;
using UnityEngine;

namespace DavidRios.Assets.Scripts.Villager
{
    public class JobManager : MonoBehaviour
    {
        public static List<VillagerLogic> villagers = new List<VillagerLogic>();

        //On awake, subscribe to the Jobs.cs FinishJob event handler. Might as well give the villagers their indexes while we're at it. Oh, and before you do that, initialize the villager list.
        private void Awake()
        {
            var i = 0;

            foreach (Transform villager in transform)
            {
                villagers.Add(villager.GetComponent<VillagerLogic>());

                villager.GetComponent<VillagerLogic>().index = i;

                i++;
            }

            Jobs.JobFinished += Jobs_FinishJob;
            Jobs.JobGroupAssigned += Jobs_AssignJobGroups;
        }

        public VillagerLogic VillagerToAssignTo(string jobType)
        {
            return villagers[JobUtils.VillagerToAssignTo(villagers, jobType)];
        }

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
            var villagerIndex = JobUtils.VillagerToAssignTo(villagers, job.jobType);

            villagers[villagerIndex].lastAddedJob = job;

            villagers[villagerIndex].jobList.Add(job);
        }

        public void AssignJob(Vector3 position, Transform[] objectiveTransforms, int[] amounts, string jobType,
            int villagerIndex)
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

            var villagerIndex = JobUtils.VillagerToAssignTo(villagers, jobType);

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

        private void Jobs_AssignJobGroups(object sender, AssignJobGroupArgs e)
        {
            AssignJobGroup(e.jobGroup, e.jobPosition, e.jobTransforms, e.amounts, e.villagerIndex);
        }

        public void AssignJobGroup(string jobGroup, Vector3 jobPosition, Transform[] objectiveTransforms,
            int[] amounts = null, int villagerIndex = -1)
        {
            if (villagerIndex == -1)
                villagerIndex = VillagerToAssignTo(jobGroup).index;

            switch (jobGroup)
            {
                case "HarvestTree":
                    var objectiveTransform = objectiveTransforms[0];

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
                    AssignJob(jobPosition, new Transform[1] {objectiveTransforms[0]}, amounts, "Deposit", villagerIndex);
                    break;
                case "Withdraw":
                    AssignJob(jobPosition, objectiveTransforms, amounts, "Move", villagerIndex);
                    AssignJob(jobPosition, new Transform[1] {objectiveTransforms[0]}, amounts, "Withdraw", villagerIndex);
                    break;
                case "GetResourcesToBuildSite":
                    AssignJob(jobPosition, new Transform[1] {objectiveTransforms[0]}, amounts, "Move", villagerIndex);
                    AssignJob(jobPosition, new Transform[1] {objectiveTransforms[0]}, amounts, "DepositToBuildSite",
                        villagerIndex);
                    break;
                case "PickUpPile":
                    AssignJob(jobPosition, objectiveTransforms, amounts, "Move", villagerIndex);
                    AssignJob(jobPosition, objectiveTransforms, amounts, "TakeFromItemPile", villagerIndex);
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
            var selectedObject = Select.SelectedObject;

            VillagerLogic selectedVillagerLogic = null;
            var jobIndex = -1;

            for (var i = 0; i < villagers.Count; i++)
            for (var j = 0; j < villagers[i].jobList.Count; j++)
            {
                var matches = villagers[i].jobList.Any(job => job.objectiveTransforms[0] == selectedObject.transform);

                if (matches)
                {
                    selectedVillagerLogic = villagers[i];
                    jobIndex = j;
                }
            }

            if (jobIndex != -1)
            {
                var jobType = selectedVillagerLogic.jobList[jobIndex].jobType;

                RemoveJob(selectedVillagerLogic.index, jobIndex);

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
                }
            }
        }

        public void ChopSelectedTree()
        {
            if (!Select.SelectedObject.GetComponent<Resource>().beingHarvested)
                AssignJobGroup("HarvestTree", Select.SelectedObject.transform.position,
                    new Transform[1] {Select.SelectedObject.transform});
        }

        //Returns true if any job of any villager contains the selected transform.
        public bool JobContainsTransform(Transform selectedTransform)
        {
            for (var i = 0; i < villagers.Count; i++)
            for (var j = 0; j < villagers[i].jobList.Count; j++)
                if (villagers[i].jobList[j].objectiveTransforms.Contains(selectedTransform))
                    return true;

            return false;
        }
    }
}