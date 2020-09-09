using System;
using System.Collections;
using DavidRios.Building;
using DavidRios.Building.Building_Types;
using Pathfinding;
using UnityEngine;
using Object = UnityEngine.Object;

public class VillagerArgs
{
    public int villagerIndex;
}

public class AssignJobGroupArgs
{
    public int[] amounts;
    public string jobGroup;
    public Vector3 jobPosition;
    public Transform[] jobTransforms;
    public int villagerIndex;
}

public class Jobs
{
    public static event EventHandler<VillagerArgs> JobFinished;

    public static event EventHandler<AssignJobGroupArgs> JobGroupAssigned;

    public static IEnumerator Move(Transform villagerToMove, Vector3 desiredPosition)
    {
        var villager = villagerToMove.GetComponent<Villager>();

        var debugLevel = villager.debugLevel;

        var villagerAIPath = villagerToMove.GetComponent<AIPath>();

        var villagerIndex = villager._index;

        villagerAIPath.canSearch = true;

        villagerAIPath.destination = desiredPosition;

        villagerAIPath.SearchPath();

        while (villagerAIPath.pathPending || !villagerAIPath.reachedEndOfPath)
        {
            if (debugLevel == VillagerDebugLevels.OverlyDetailed)
                Debug.Log("Not there yet. Remaining distance: " + villagerAIPath.remainingDistance);

            if (villager.moveJobCancelled)
            {
                // This will clear the path
                villagerAIPath.SetPath(null);
                // This will prevent the agent from immediately recalculating a new path
                villagerAIPath.canSearch = false;

                villager.moveJobCancelled = false;

                break;
            }

            yield return null;
        }

        if (debugLevel == VillagerDebugLevels.OverlyDetailed)
            Debug.Log("Finished Path");

        FinishJob(villagerToMove.GetComponent<Villager>());
    }

    public static IEnumerator ChopTree(Villager villager, Transform treeToChop)
    {
        var log = new Item {itemObject = Resources.Load("Prefabs/Items/Log") as GameObject, itemType = "Log"};

        var tree = treeToChop.GetComponent<Resource>();

        var chopRate = villager._harvestRate;
        var chopAmount = villager._harvestAmount;

        //Generate Stockpile
        var pile = new GameObject();
        pile.transform.parent = GameObject.Find("Item Piles").transform;

        pile.transform.position = treeToChop.position;
        pile.name = "Log Pile";
        pile.layer = LayerMask.NameToLayer("Pile");
        pile.tag = "Tree";
        pile.transform.eulerAngles = villager.transform.eulerAngles;

        var logNumber = 0;
        var logYMultiplier = 0;

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

            for (var i = 0; i < chopAmount; i++)
            {
                logNumber++;

                var _log = Object.Instantiate(log.itemObject, pile.transform);

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
                    _log.transform.localPosition += new Vector3(0f, 0.14f * logYMultiplier, 0.08f);

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
                pile.GetComponent<BoxCollider>().center =
                    new Vector3(0f, pile.GetComponent<BoxCollider>().size.y / 2f, 0f);

                tree.transform.parent.GetComponent<Animator>().SetTrigger("Felled");

                yield return new WaitForSeconds(2);

                FinishJob(villager);

                pile.AddComponent<ItemPile>();

                yield break;
            }
        }
    }

    public static IEnumerator Deposit(Villager villager, Transform[] storages, int amount)
    {
        storages[0].GetComponent<Storage>().UpdateAppearance(villager._index);

        if (villager.items.Count > 0)
            for (var j = 0; j < amount; j++)
                villager.items.RemoveAt(villager.items.Count - 1);

        while (villager.transform.childCount > villager.items.Count)
        {
            Object.Destroy(villager.transform.GetChild(villager.transform.childCount - 1).gameObject);

            yield return null;
        }

        FinishJob(villager);
    }

    public static IEnumerator Withdraw(Villager villager, Transform[] storages, int amount)
    {
        storages[0].GetComponent<Storage>().UpdateAppearance(villager._index);

        yield return new WaitUntil(() => villager.items.Count == amount);

        FinishJob(villager);
    }

    public static IEnumerator TakeFromItemPile(Villager villager, Transform itemPile, int amount)
    {
        var itemPileScript = itemPile.GetComponent<ItemPile>();

        for (var i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(villager._itemExchangeRate);

            villager.items.Add(
                new Item
                {
                    itemType = itemPileScript.TakeItem(villager.items.Count == villager.inventoryCapacity - 1),
                    itemObject =
                        Resources.Load<GameObject>(
                            "Prefabs/Items/Log") //Get Object based on type function would be nice here. 
                }
            );

            villager.DisplayItems();
        }

        FinishJob(villager);
    }

    public static IEnumerator DepositToBuildSite(Villager villager, Transform buildSite, int amount)
    {
        buildSite.GetComponent<UnderConstruction>().beingDeposited = true;

        var itemType = villager.items[0].itemType;
        var itemIndex = ItemInfo.GetItemIndex(itemType);

        while (villager.items.Count > 0)
        {
            yield return new WaitForSeconds(villager._itemExchangeRate);

            if (buildSite == null)
            {
                FinishJob(villager);
                yield break;
            }

            buildSite.GetComponent<UnderConstruction>().itemsOnSite[itemIndex].amount++;

            Object.Destroy(villager.transform.GetChild(villager.transform.childCount - 1).gameObject);

            villager.items.RemoveAt(villager.items.Count - 1);
        }

        FinishJob(villager);
    }

    public static IEnumerator Build(Villager villager, Transform buildSite)
    {
        var buildingConstructionScript = buildSite.GetComponent<UnderConstruction>();

        buildingConstructionScript.InitializeConstructionSite();

        //Assuming that resources required are already there
        var constructionSiteEndScale =
            new Vector3(buildSite.localScale.y, buildSite.localScale.y, buildSite.localScale.z);

        //Clear Grass
        while (buildingConstructionScript.currentStage == 0 && !buildingConstructionScript.currentlyProgressing)
        {
            buildingConstructionScript.ClearGrass(villager._buildAmount, constructionSiteEndScale);

            yield return null;
        }

        //Wait until all of the necessary resources are at the building before continuing.
        yield return new WaitUntil(() => buildingConstructionScript.sufficientItems);

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

    private static void FinishJob(Villager villager)
    {
        JobFinished?.Invoke(villager, new VillagerArgs {villagerIndex = villager._index});
    }
}