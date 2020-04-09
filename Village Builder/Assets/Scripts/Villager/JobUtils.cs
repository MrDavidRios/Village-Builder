using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class JobUtils
{
    //Return the villager that has the least amount of jobs.
    public static int VillagerToAssignTo(List<Villager> villagers)
    {
        var jobAmount = new List<int>();

        foreach (var villager in villagers)
        {
            jobAmount.Add(villager.jobList.Count);
        }

        int leastBusyVillagerIndex = jobAmount.IndexOf(jobAmount.Min());

        return leastBusyVillagerIndex;
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

    public static Transform NearestStorage(Vector3 relativePosition)
    {
        return null;
    }

    public static Transform GetStorageWithPosition(Vector3 storagePos)
    {
        return null;
    }
}
