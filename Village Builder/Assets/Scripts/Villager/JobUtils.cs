using System.Collections.Generic;
using System.Linq;
using DavidRios.Building;
using DavidRios.Building.Building_Types;
using UnityEngine;

public static class JobUtils
{
    //Return the villager that has the least amount of jobs.
    public static int VillagerToAssignTo(List<Villager> villagers, string jobType)
    {
        var eligibleVillagers = new List<Villager>();

        foreach (var villager in villagers)
            if (DetermineIfVillagerQualified(villager, jobType))
                eligibleVillagers.Add(villager);

        //Get the villager with the least amount of jobs and return their index.
        return eligibleVillagers.Aggregate((i1, i2) => i1.jobList.Count < i2.jobList.Count ? i1 : i2)._index;
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
            case "Withdraw":
            case "Deposit":
            case "DepositToBuildSite":
                return true;
            default:
                Debug.LogError("Invalid Job Type: " + jobType);
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
                return "Deposit";
            case "Withdraw":
                return "Withdraw";
            case "DepositToBuildSite":
                return "Placing Resources";
            default:
                return jobType;
        }
    }

    /// <summary>
    ///     Gets the villager that has a job with matching transform.
    ///     Useful for finding a villager that will interact with a certain object.
    /// </summary>
    /// <param name="villagers"></param>
    /// <param name="jobTransform"></param>
    /// <returns></returns>
    public static Villager ReturnVillagerFromJobTransform(List<Villager> villagers, Transform jobTransform)
    {
        //Loop through all villagers
        //If villager's job objectiveTransform[0] matches with jobTransform, return that villager.
        foreach (var villager in villagers)
            for (var i = 0; i < villager.jobList.Count; i++)
                if (villager.jobList[i].objectiveTransforms[0] == jobTransform)
                    return villager;

        return null;
    }

    /// <summary>
    ///     Returns the nearest tree to a villager.
    /// </summary>
    /// <param name="villager"></param>
    /// <returns></returns>
    public static Transform NearestTree(Transform villager)
    {
        //Get tree parent and then make a list from its children (the trees)
        var treeParent = GameObject.Find("Trees").transform;

        var trees = new List<Transform>();
        var treeDistances = new List<float>();

        foreach (Transform tree in treeParent)
            if (!tree.GetComponent<Resource>().beingHarvested)
            {
                trees.Add(tree);
                treeDistances.Add(Vector3.Distance(villager.position, tree.position));
            }

        if (trees.Count == 0)
        {
            Debug.LogWarning("No trees to chop.");

            return null;
        }

        var closestTreeIndex = treeDistances.IndexOf(treeDistances.Min());

        return trees[closestTreeIndex];
    }

    //Return nearest storage that can handle villager's items.
    public static Transform[] GetNearestAvailableStorages(Villager villager, string itemType, int itemAmount,
        bool deposit = true)
    {
        var storages = StorageManager.storages;
        var matchingStorages = new List<Transform>();

        //Find storages for deposit
        if (deposit)
        {
            var itemsLeftToDeposit = itemAmount;

            //List<float> storageDistances = new List<float>();

            //Find a way to incorporate distance?

            for (var i = 0; i < storages.Count; i++)
            {
                var spaceForItemType = storages[i].GetComponent<Storage>().GetSpaceForItemsByItemType(itemType);

                if (spaceForItemType > 0)
                {
                    if (spaceForItemType < itemsLeftToDeposit)
                        itemsLeftToDeposit -= spaceForItemType;
                    else
                        itemsLeftToDeposit = 0;

                    matchingStorages.Add(storages[i]);
                }

                if (itemsLeftToDeposit == 0)
                    break;
            }

            if (matchingStorages.Count > 0)
                return matchingStorages.ToArray();
            return null;

            /*
            int minDistIndex = storageDistances.IndexOf(storageDistances.Min());

            if (storages[minDistIndex] != null)
                return storages[minDistIndex];
            else
                return null;
            */
        }
        //Find storages for withdrawal

        var itemsLeftToWithdraw = itemAmount;

        //TO IMPLEMENT LATER: Either make the villager favor shorter walks; if the storage is full but would take longer than doing two storages that are closer and have the same amount of resources, then let the villager do that.
        for (var i = 0; i < storages.Count; i++)
        {
            var itemsInStorage = storages[i].GetComponent<Storage>().GetStoredItemAmountByItemType(itemType);

            if (itemsInStorage > 0)
            {
                if (itemsInStorage > itemsLeftToWithdraw)
                    itemsLeftToWithdraw -= itemsInStorage;
                else
                    itemsLeftToWithdraw = 0;

                matchingStorages.Add(storages[i]);
            }

            if (itemsLeftToWithdraw == 0)
                break;
        }

        if (matchingStorages.Count > 0)
            return matchingStorages.ToArray();
        return null;
    }
}

public class StorageInfo
{
    public int remaningApplicableAmount;
    public Transform transform;
}