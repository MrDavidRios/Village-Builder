using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class VillagerArgs { public int villagerIndex; }

public class AssignJobGroupArgs { public string jobGroup; public Vector3 jobPosition; public Transform jobTransform; public int villagerIndex; }

public class Jobs
{
    public static event EventHandler<VillagerArgs> JobFinished;

    public static event EventHandler<AssignJobGroupArgs> JobGroupAssigned;

    public static event EventHandler<VillagerArgs> InventoryUpdated;

    public static IEnumerator Move(Transform villagerToMove, Vector3 desiredPosition)
    {
        var debugLevel = villagerToMove.GetComponent<Villager>().debugLevel;

        var villagerAIPath = villagerToMove.GetComponent<AIPath>();

        int villagerIndex = villagerToMove.GetComponent<Villager>().index;

        villagerAIPath.destination = desiredPosition;

        villagerAIPath.SearchPath();

        while (villagerAIPath.pathPending || !villagerAIPath.reachedEndOfPath)
        {
            if (debugLevel == VillagerDebugLevels.OverlyDetailed)
                Debug.Log("Not there yet. Remaining distance: " + villagerAIPath.remainingDistance);

            yield return null;
        }

        if (debugLevel == VillagerDebugLevels.OverlyDetailed)
            Debug.Log("Finished Path");

        FinishJob(villagerToMove.GetComponent<Villager>());
    }

    public static IEnumerator ChopTree(Villager villager, Transform treeToChop)
    {
        var log = new Item { itemObject = Resources.Load("Prefabs/Items/Log") as GameObject, itemType = "Log" };

        var tree = treeToChop.GetComponent<Resource>();

        int chopRate = villager._harvestRate;
        int chopAmount = villager._harvestAmount;

        while (tree.gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(chopRate);

            if (tree.resourceAmount < chopAmount)
                tree.HarvestResource(tree.resourceAmount);
            else
                tree.HarvestResource(chopAmount);

            //Add a log to the villager's inventory for each log chopped
            if (!villager.inventoryFull)
            {
                for (int i = 0; i < chopAmount; i++)
                {
                    villager.items.Add(log);

                    InventoryUpdated?.Invoke(villager, new VillagerArgs { villagerIndex = villager.index });
                }
            }

            //Add tree shake animation every time it is chopped

            if (tree.resourceAmount <= 0)
            {
                Environment.RemoveTree(treeToChop);

                FinishJob(villager);

                yield break;
            }
            else if (villager.inventoryFull)
            {
                FinishJob(villager);

                JobGroupAssigned?.Invoke(villager, new AssignJobGroupArgs { jobGroup = "HarvestTree", jobPosition = Vector3.zero, jobTransform = treeToChop, villagerIndex = villager.index });

                yield break;
            }
        }
    }

    public static IEnumerator Deposit(Villager villager, Transform storage)
    {
        yield return null;
    }

    private static void FinishJob(Villager villager) => JobFinished?.Invoke(villager, new VillagerArgs { villagerIndex = villager.index });
}
