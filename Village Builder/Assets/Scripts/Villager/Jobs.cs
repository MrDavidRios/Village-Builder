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
        bool jobFinished = false;

        var log = new Item { itemObject = Resources.Load("Prefabs/Items/Log") as GameObject, itemType = "Log" };

        var tree = treeToChop.GetComponent<Resource>();

        int chopRate = villager._harvestRate;
        int chopAmount = villager._harvestAmount;

        //Generate Stockpile
        var pile = new GameObject();
        pile.transform.parent = GameObject.Find("Item Piles").transform;

        pile.transform.position = treeToChop.position;
        pile.name = "Log Pile";
        pile.layer = LayerMask.NameToLayer("Pile");
        pile.tag = "Tree";
        pile.transform.eulerAngles = villager.transform.eulerAngles;

        int logNumber = 0;
        int logYMultiplier = 0;

        while (tree.gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(chopRate);

            if (tree.resourceAmount < chopAmount)
                tree.HarvestResource(tree.resourceAmount);
            else
                tree.HarvestResource(chopAmount);

            //Add to pile
            #region Pile Code
            for (int i = 0; i < chopAmount; i++)
            {
                logNumber++;

                var _log = GameObject.Instantiate(log.itemObject, pile.transform);

                //Position offset for correct log display
                _log.transform.localEulerAngles = Vector3.zero;
                _log.transform.localScale = new Vector3(0.35f, 0.14f, 0.14f);
                _log.transform.position += new Vector3(0.0f, 0.07f, 0.0f);
                _log.name = "Log #" + logNumber;

                //Examples: 1, 5, 9, 13, 17, etc. 
                if ((logNumber - 1) % 4 == 0)
                {
                    if (logNumber != 1)
                        logYMultiplier++;

                    _log.transform.localPosition += new Vector3(0f, 0.14f * logYMultiplier, -0.08f);
                }

                //Examples: 2, 6, 10, 14, 18, etc.
                if ((logNumber - 2) % 4 == 0)
                {
                    _log.transform.localPosition += new Vector3(0f, 0.14f * logYMultiplier, 0.08f);
                }

                //Examples: 3, 7, 11, 15, 19, etc.
                if ((logNumber + 1) % 4 == 0)
                {
                    logYMultiplier++;

                    _log.transform.localPosition += new Vector3(0.085f, 0.14f * logYMultiplier, 0f);
                    _log.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                }

                //Examples: 4, 8, 12, 16, etc.
                if (logNumber % 4 == 0)
                {
                    _log.transform.localPosition += new Vector3(-0.085f, 0.14f * logYMultiplier, 0f);
                    _log.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                }
            }
            #endregion

            //Add tree shake animation every time it is chopped

            if (tree.resourceAmount <= 0)
            {
                Environment.RemoveTree(treeToChop);

                pile.AddComponent<BoxCollider>();
                pile.GetComponent<BoxCollider>().size = new Vector3(0.35f, 0.14f * logYMultiplier, 0.35f);
                pile.GetComponent<BoxCollider>().center = new Vector3(0f, pile.GetComponent<BoxCollider>().size.y / 2f, 0f);

                FinishJob(villager);

                jobFinished = true;

                yield break;
            }

            if (villager.inventoryFull)
            {
                if (!jobFinished)
                {
                    FinishJob(villager);

                    JobGroupAssigned?.Invoke(villager, new AssignJobGroupArgs { jobGroup = "HarvestTree", jobPosition = Vector3.zero, jobTransform = treeToChop, villagerIndex = villager.index });
                }
                else
                    JobGroupAssigned?.Invoke(villager, new AssignJobGroupArgs { jobGroup = "Deposit", jobPosition = Vector3.zero, jobTransform = null, villagerIndex = villager.index });

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
