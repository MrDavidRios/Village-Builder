using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Storage : MonoBehaviour
{
    Dictionary<string, int> itemSizes = new Dictionary<string, int>()
    {
        { "Log", 2 },
        { "Stone", 1 },
        { "RefinedStone", 1 },
        { "Iron", 1 }
    };

    public int numberOfPlots;

    //Number of units each plot can take. Ex: 50 = 50 stone bricks (1x1) & 25 logs (2x1).
    public int plotCapacity;

    public int totalCapacity;

    public List<List<Item>> plots = new List<List<Item>>();

    private GameObject[] plotObjects;

    //Plot Positions
    public Vector3[] plotPositions;

    //Solely for inspector display
    private Item[] plotItems;

    Dictionary<string, Item> items = new Dictionary<string, Item>();

    Dictionary<int, Queue<ItemBundle>> updateQueue = new Dictionary<int, Queue<ItemBundle>>();

    Dictionary<int, int> numberOfItemsSpawnedPerPlot = new Dictionary<int, int>();

    public bool debug;

    //Ensures 'Awake' method only called when the building has been placed.
    private void Awake() => StartCoroutine(Init());

    //Wait until the building has finished its construction to begin.
    IEnumerator Init()
    {
        yield return new WaitUntil(() => GetComponent<UnderConstruction>() == null);

        float storageWidth = GetComponent<InfrastructureBuilding>().building.width;

        float tempNumberOfPlots = Mathf.Pow(storageWidth, 2f) * 4;
        numberOfPlots = (int)tempNumberOfPlots;

        totalCapacity = numberOfPlots * plotCapacity;

        //Change this later when save/load is implemented
        for (int i = 0; i < numberOfPlots; i++)
        {
            plots.Add(new List<Item>());
        }

        //Instantiate Plots
        plotObjects = new GameObject[numberOfPlots];

        for (int i = 0; i < plotObjects.Length; i++)
        {
            GameObject currentPlot = new GameObject();

            currentPlot.name = "Plot #" + (i + 1);

            //SET CURRENT PLOT POSITION HERE BASED ON ITS INDEX
            currentPlot.transform.localPosition = transform.position + plotPositions[i];

            currentPlot.transform.parent = transform;

            plotObjects[i] = currentPlot;

            numberOfItemsSpawnedPerPlot.Add(i, 0);
        }

        items.Add("Log", new Item { itemObject = Resources.Load<GameObject>("Prefabs/Items/Log"), itemType = "Log" });
    }

    /// <summary>
    /// Updates the data for the storage's items. Adds to a dictionary that allows the storage to show the deposited items once the UpdateAppearance() function is called.
    /// </summary>
    /// <param name="itemsToDeposit"></param>
    /// <param name="villagerIndex"></param>
    public void QueueItemsForDeposit(ItemBundle itemsToDeposit, int villagerIndex)
    {
        string itemType = itemsToDeposit.item.itemType;
        int itemAmount = itemsToDeposit.amount;

        int itemsLeftToStore = itemAmount;

        //Get plots to deposit in
        List<int> matchingPlotIndices = ReturnPlotsByItemType(itemType, true);

        List<int> plotsToDepositIn = new List<int>();

        //Make Dictionary with each plot index and the number of items that would go into each plot. Key: plotIndex. Value: item amount
        Dictionary<int, int> itemsPerPlot = new Dictionary<int, int>();

        //Total item amount that can be stored in a given plot
        int amountOfItemsLeftInPlot;

        //Distribute items among storage plots
        for (int i = 0; i < matchingPlotIndices.Count; i++)
        {
            if (itemsLeftToStore > 0)
            {
                amountOfItemsLeftInPlot = GetRemainingPlotSpace(matchingPlotIndices[i]) / itemSizes[itemType];

                //Ignore this entire process if the plot is full.
                if (amountOfItemsLeftInPlot > 0)
                {
                    if (amountOfItemsLeftInPlot > itemsLeftToStore)
                        amountOfItemsLeftInPlot = itemsLeftToStore;

                    itemsLeftToStore -= amountOfItemsLeftInPlot;

                    itemsPerPlot.Add(matchingPlotIndices[i], amountOfItemsLeftInPlot);

                    plotsToDepositIn.Add(matchingPlotIndices[i]);
                }
            }
            else
                break;
        }

        if (debug)
            Debug.Log("Amount of plots to use: " + itemsPerPlot.Count);

        //Each Plot
        foreach (var plot in itemsPerPlot)
        {
            int i = plot.Key;

            int beginningAmountOfItemsInPlot = GetAmountOfItemsInPlot(i);

            if (debug)
                Debug.Log("Amount of items to deposit: " + plot.Value + "; Number of items in plot before deposit: " + beginningAmountOfItemsInPlot);

            //Each Item
            for (int j = beginningAmountOfItemsInPlot + 1; j <= plot.Value + beginningAmountOfItemsInPlot; j++)
            {
                if (debug)
                    Debug.Log("Plot #: " + i + "; Amount of items in plot: " + beginningAmountOfItemsInPlot);

                plots[i - 1].Add(items[itemType]);
            }
        }

        ItemBundle itemsDeposited = new ItemBundle { item = items[itemType], amount = itemAmount };

        Queue<ItemBundle> villagerItemQueue = new Queue<ItemBundle>();

        if (updateQueue.ContainsKey(villagerIndex))
        {
            villagerItemQueue = updateQueue[villagerIndex];

            villagerItemQueue.Enqueue(itemsDeposited);

            updateQueue[villagerIndex] = villagerItemQueue;
        }
        else
        {
            villagerItemQueue.Enqueue(itemsDeposited);

            updateQueue.Add(villagerIndex, villagerItemQueue);
        }

        /*
        if (updateQueue.ContainsKey(villagerIndex) && updateQueue[villagerIndex].item.itemType == itemType)
            updateQueue[villagerIndex] = new ItemBundle { item = updateQueue[villagerIndex].item, amount = updateQueue[villagerIndex].amount + itemAmount };
        else
            updateQueue.Add(villagerIndex, itemsDeposited);
        */
    }

    public ItemBundle WithdrawItem(Item itemToTake, int itemAmount)
    {
        //Get plots to deposit in
        List<int> matchingPlotIndices = ReturnPlotsByItemType(itemToTake.itemType, false, true);

        List<int> plotsToWithdrawFrom = new List<int>();

        int itemsLeftToWithdraw = itemAmount;
        int amountOfItemsLeftInPlot;
        int itemsToTakeFromPlot;

        //Cycles backwards
        for (int i = matchingPlotIndices.Count - 1; i >= 0; i--)
        {
            amountOfItemsLeftInPlot = GetAmountOfItemsInPlot(matchingPlotIndices[i]);

            if (amountOfItemsLeftInPlot > itemsLeftToWithdraw)
                itemsToTakeFromPlot = itemsLeftToWithdraw;
            else
                itemsToTakeFromPlot = amountOfItemsLeftInPlot;

            itemsLeftToWithdraw -= itemsToTakeFromPlot;

            Transform selectedPlot = transform.GetChild(i);
            int initialItemCount = selectedPlot.childCount;

            for (int j = 1; j <= itemsToTakeFromPlot; j++)
            {
                //GameObject.Destroy(selectedPlot.GetChild(initialItemCount - j).gameObject);

                plots[i].RemoveAt(initialItemCount - j);
            }
        }

        return new ItemBundle { item = new Item { itemObject = itemToTake.itemObject, itemType = itemToTake.itemType }, amount = itemAmount };
    }

    //Regenerate storage appearance based on new values (PLAN: UPDATE STORAGE APPEARANCE ONCE THE VILLAGER GETS THERE)
    public void UpdateAppearance(int villagerIndex)
    {
        ItemBundle itemsToAdd = updateQueue[villagerIndex].Dequeue();

        string itemType = itemsToAdd.item.itemType;
        GameObject itemObject = itemsToAdd.item.itemObject;

        int itemAmount = itemsToAdd.amount;

        //Only display the amount of items in the itemBundle.

        //Every Plot
        int itemsLeftToPlace = itemAmount;

        foreach (var plot in plots)
        {
            //If there are no more items to place, then leave!
            if (itemsLeftToPlace == 0)
                break;

            //Get the index of the plot so that its object can be accessed.
            int plotIndex = plots.IndexOf(plot);

            //Dictionary<int, int> amountToPlaceInEachMatchingPlot = new Dictionary<int, int>();

            bool isPlotEmpty = plot.Count == 0;

            bool doesPlotMatch = false;

            if (isPlotEmpty)
                doesPlotMatch = true;
            else
                doesPlotMatch = plot[0].itemType == itemType;

            //Only continue if the item type is the same
            if (doesPlotMatch)
            {
                //Get remaining amount of items available to fit in the plot
                int amountOfSpaceItemTakesUp = itemSizes[itemType];

                int amountOfItemsThatCanFit = (plotCapacity - (numberOfItemsSpawnedPerPlot[plotIndex]/*plotObjects[plotIndex].transform.childCount*/ * amountOfSpaceItemTakesUp)) / amountOfSpaceItemTakesUp;

                int amountOfItemsToPutInThisPlot = 0;

                //If there's space for any items, then continue.
                if (amountOfItemsThatCanFit > 0)
                {
                    if (amountOfItemsThatCanFit > itemsLeftToPlace || amountOfItemsThatCanFit == itemsLeftToPlace)
                        amountOfItemsToPutInThisPlot = itemsLeftToPlace;

                    if (amountOfItemsThatCanFit < itemsLeftToPlace)
                        amountOfItemsToPutInThisPlot = amountOfItemsThatCanFit;

                    for (int i = 1; i <= amountOfItemsToPutInThisPlot; i++)
                    {
                        GameObject item = Instantiate(itemObject, plotObjects[plotIndex].transform);

                        int itemNumber = numberOfItemsSpawnedPerPlot[plotIndex] + 1;

                        numberOfItemsSpawnedPerPlot[plotIndex] = itemNumber;

                        switch (itemType)
                        {
                            case "Log":
                                PlaceLog(item, itemNumber);
                                break;
                            default:
                                Debug.LogError("Unknown itemType: " + itemType);
                                break;
                        }

                        itemsLeftToPlace--;
                    }
                }
            }
        }
    }

    private void PlaceLog(GameObject _log, int logNumber)
    {
        //Position offset for correct log display
        _log.transform.localEulerAngles = Vector3.zero;
        _log.transform.localScale = new Vector3(0.35f, 0.14f, 0.14f);
        _log.name = "Log #" + logNumber;
        _log.transform.position += new Vector3(0.0f, 0.07f, 0.0f);

        //Vector3 startingPos = new Vector3(0.5f, 6f, -0.17f);

        int logYMultiplier = (logNumber + 1) / 2;
        logYMultiplier--;

        //Examples: 1, 5, 9, 13, 17, etc. 
        if ((logNumber - 1) % 4 == 0)
        {
            _log.transform.localPosition += new Vector3(0f, 0.14f * logYMultiplier, -0.1f);
        }

        //Examples: 2, 6, 10, 14, 18, etc.
        if ((logNumber - 2) % 4 == 0)
        {
            _log.transform.localPosition += new Vector3(0f, 0.14f * logYMultiplier, 0.1f);
        }

        //Examples: 3, 7, 11, 15, 19, etc.
        if ((logNumber + 1) % 4 == 0)
        {
            _log.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

            _log.transform.localPosition += new Vector3(0.1f, 0.14f * logYMultiplier, 0f);
        }

        //Examples: 4, 8, 12, 16, etc.
        if (logNumber % 4 == 0)
        {
            _log.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

            _log.transform.localPosition += new Vector3(-0.1f, 0.14f * logYMultiplier, 0f);
        }
    }

    #region Additional Functions
    private bool IsPlotFull(int plotIndex)
    {
        //If plotIndex is 1, then it will return plots[0].Count (number of items in plot)
        if ((plots[plotIndex - 1].Count * itemSizes[ReturnPlotItemType(plotIndex)]) < plotCapacity)
            return false;
        else
            return true;
    }

    private bool IsPlotEmpty(int plotindex) => plots[plotindex - 1].Count == 0;

    private int GetAmountOfItemsInPlot(int plotIndex) => plots[plotIndex - 1].Count;

    private int GetRemainingPlotSpace(int plotIndex)
    {
        int numberOfItemsInPlot = plots[plotIndex - 1].Count;

        if (numberOfItemsInPlot == 0)
            return plotCapacity;

        int itemTypeSize = itemSizes[ReturnPlotItemType(plotIndex)];

        return plotCapacity - (numberOfItemsInPlot * itemTypeSize);
    }

    //Returns the itemtype of the first item in the selected plot.
    private string ReturnPlotItemType(int plotIndex) => plots[plotIndex - 1][0].itemType;

    private List<int> ReturnPlotsByItemType(string itemType, bool addEmptyPlots = false, bool addFullPlots = false)
    {
        List<int> matchingPlotIndices = new List<int>();

        for (int i = 1; i <= plots.Count; i++)
        {
            if (IsPlotEmpty(i) && addEmptyPlots)
            {
                matchingPlotIndices.Add(i);
            }
            else if (!IsPlotEmpty(i))
            {
                if (ReturnPlotItemType(i) == itemType)
                {
                    if ((IsPlotFull(i) && addFullPlots) || !IsPlotFull(i))
                    {
                        matchingPlotIndices.Add(i);
                    }
                }
            }
        }

        return matchingPlotIndices;
    }

    private List<int> ReturnEmptyPlots()
    {
        List<int> matchingPlotIndices = new List<int>();

        for (int i = 1; i <= plots.Count; i++)
        {
            if (IsPlotEmpty(i))
                matchingPlotIndices.Add(i);
        }

        return matchingPlotIndices;
    }
    #endregion

    //Returns the number of items left that fit in the storage unit depending on the item type provided.
    public int GetSpaceForItemsByItemType(string itemType)
    {
        int remainingSpace = 0;

        List<int> matchingPlots = ReturnPlotsByItemType(itemType, true);

        for (int i = 0; i < matchingPlots.Count; i++)
        {
            remainingSpace += GetRemainingPlotSpace(matchingPlots[i]);
        }

        return remainingSpace / itemSizes[itemType];
    }
}
