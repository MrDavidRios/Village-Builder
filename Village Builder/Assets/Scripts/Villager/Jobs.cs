﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class VillagerArgs { public int villagerIndex; }

public class AssignJobGroupArgs { public string jobGroup; public Vector3 jobPosition; public Transform[] jobTransforms; public int[] amounts; public int villagerIndex; }

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
        //bool jobFinished = false;

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

        //Reactivate tree animator component to display animations
        tree.transform.parent.GetComponent<Animator>().enabled = true;

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
            tree.transform.parent.GetComponent<Animator>().SetTrigger("Hit");

            if (tree.resourceAmount <= 0)
            {
                pile.AddComponent<BoxCollider>();
                pile.GetComponent<BoxCollider>().size = new Vector3(0.35f, 0.14f * logYMultiplier, 0.35f);
                pile.GetComponent<BoxCollider>().center = new Vector3(0f, pile.GetComponent<BoxCollider>().size.y / 2f, 0f);

                FinishJob(villager);

                tree.transform.parent.GetComponent<Animator>().SetTrigger("Felled");

                pile.AddComponent<ItemPile>();

                //jobFinished = true;

                yield break;
            }

            /*
            if (villager.inventoryFull)
            {
                if (!jobFinished)
                {
                    FinishJob(villager);

                    JobGroupAssigned?.Invoke(villager, new AssignJobGroupArgs { jobGroup = "HarvestTree", jobPosition = Vector3.zero, jobTransforms = new Transform[1] { treeToChop }, amounts = new int[0], villagerIndex = villager.index });
                }
                else
                    JobGroupAssigned?.Invoke(villager, new AssignJobGroupArgs { jobGroup = "Deposit", jobPosition = Vector3.zero, jobTransforms = null, amounts = new int[0], villagerIndex = villager.index });

                yield break;
            }
            */
        }
    }

    public static IEnumerator Deposit(Villager villager, Transform[] storages, int amount)
    {
        //villager.currentlyDepositing = true;

        string itemType = villager.items[0].itemType;

        //Transform[] storagesToUse = JobUtils.GetNearestAvailableStorages(villager, itemType, amount);
        /*Move to storage unit
        JobManager.AssignJob(new Job { position = storagesToUse[i].position, objectiveTransforms = storagesToUse, amounts = new int[0], jobType = "Move" }, villager.index, 0);
        villager.performingJob = false;

        yield return new WaitUntil(() => villager.jobList[0].jobType == "Deposit");
        */

        storages[0].GetComponent<Storage>().UpdateAppearance(villager.index);

        if (villager.items.Count > 0)
        {
            for (int j = 0; j < amount; j++)
            {
                villager.items.RemoveAt(villager.items.Count - 1);
            }
        }

        while (villager.transform.childCount > villager.items.Count)
        {
            GameObject.Destroy(villager.transform.GetChild(villager.transform.childCount - 1).gameObject);

            yield return null;
        }

        /*
        int amountToDeposit = storagesToUse[i].GetComponent<Storage>().GetSpaceForItemsByItemType(itemType);

        if (amountToDeposit > amount)
            amountToDeposit = amount;

        int amountDeposited = 0;

        while (amountDeposited < amountToDeposit)
        {
            yield return new WaitForSeconds(villager._itemExchangeRate);

            amountDeposited++;

            storagesToUse[i].GetComponent<Storage>().UpdateAppearance();

            villager.items.RemoveAt(villager.items.Count - 1);

            InventoryUpdated?.Invoke(villager, new VillagerArgs { villagerIndex = villager.index });
        }
        */

        villager.currentlyDepositing = false;

        FinishJob(villager);
    }

    public static IEnumerator Withdraw(Villager villager, int amount)
    {
        yield return null;
    }

    public static IEnumerator TakeFromItemPile(Villager villager, Transform itemPile)
    {
        ItemPile itemPileScript = itemPile.GetComponent<ItemPile>();

        while (!villager.inventoryFull && itemPileScript.amountOfItems > 0)
        {
            yield return new WaitForSeconds(villager._itemExchangeRate);

            villager.items.Add(
                new Item
                {
                    itemType = itemPileScript.TakeItem(villager.items.Count == villager.inventoryCapacity - 1),
                    itemObject = Resources.Load<GameObject>("Prefabs/Items/Log") //Get Object based on type function would be nice here. 
                }
            );

            InventoryUpdated?.Invoke(villager, new VillagerArgs { villagerIndex = villager.index });
        }

        FinishJob(villager);
    }

    public static IEnumerator Build(Villager villager, Transform buildSite)
    {
        UnderConstruction buildingConstructionScript = buildSite.GetComponent<UnderConstruction>();

        buildingConstructionScript.InitializeConstructionSite();

        //Assuming that resources required are already there
        Vector3 constructionSiteEndScale = new Vector3(buildSite.localScale.y, buildSite.localScale.y, buildSite.localScale.z);

        //Clear Grass
        while (buildingConstructionScript.currentStage == 0 && !buildingConstructionScript.currentlyProgressing)
        {
            buildingConstructionScript.ClearGrass(villager._buildAmount, constructionSiteEndScale);

            yield return null;
        }

        if (buildingConstructionScript.stageAmount == 0)
        {
            buildingConstructionScript.DestroyScript();
            FinishJob(villager);
            yield break;
        }

        //Begin Construction
        while (buildingConstructionScript != null)
        {
            yield return new WaitForSeconds(villager._buildRate);

            buildingConstructionScript.WorkOnBuilding(villager._buildAmount);
        }

        FinishJob(villager);

        yield return null;
    }

    private static void FinishJob(Villager villager) => JobFinished?.Invoke(villager, new VillagerArgs { villagerIndex = villager.index });
}
