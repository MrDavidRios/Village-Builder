using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class JobUtils
{
    //Return the villager that has the least amount of jobs.
    public static int VillagerToAssignTo(List<Villager> villagers, string jobType)
    {
        var eligibleVillagers = new List<Villager>();

        foreach (var villager in villagers)
        {
            if (DetermineIfVillagerQualified(villager, jobType))
                eligibleVillagers.Add(villager);
        }

        //Get the villager with the least amount of jobs and return their index.
        return eligibleVillagers.Aggregate((i1, i2) => i1.jobList.Count < i2.jobList.Count ? i1 : i2).index;
    }

    public static bool DetermineIfVillagerQualified(Villager villager, string jobType)
    {
        switch (jobType)
        {
            case "Move":
                return true;
            case "Chop":
            case "HarvestTree":
                return villager._role == "Laborer";
            case "HarvestStone":
            case "Mine":
                return villager._role == "Laborer";
            case "Build":
                return villager._role == "Laborer" || villager._role == "Carpenter";
            case "Deposit":
                return true;
            default:
                return false;
        }
    }

    public static string ReturnJobName(string jobType)
    {
        switch (jobType)
        {
            case "Move":
                return "Walk";
            case "Chop":
                return "Chop Tree";
            case "Mine":
                return "Mine Rock";
            case "Construct":
                return "Construct Building";
            case "Deposit":
                return "Drop Materials Off";
            default:
                return jobType;
        }
    }

    public static Transform NearestTree(Transform villager)
    {
        //Get tree parent and then make a list from its children (the trees)
        var treeParent = GameObject.Find("Trees").transform;

        var trees = new List<Transform>();
        var treeDistances = new List<float>();

        foreach (Transform tree in treeParent)
        {
            if (!tree.GetComponent<Resource>().beingHarvested)
            {
                trees.Add(tree);
                treeDistances.Add(Vector3.Distance(villager.position, tree.position));
            }
        }

        if (trees.Count == 0)
        {
            Debug.LogWarning("No trees to chop.");

            return null;
        }

        int closestTreeIndex = treeDistances.IndexOf(treeDistances.Min());

        return trees[closestTreeIndex];
    }

    //Return nearest storage that can handle villager's items.
    public static StorageInfo[] GetNearestAvailableStorages(Vector3 relativePosition)
    {
        return null;
    }

    public static Transform GetStorageWithPosition(Vector3 storagePos)
    {
        return null;
    }
}

public class StorageInfo
{
    public Transform transform;
    public int remaningApplicableAmount;
}
