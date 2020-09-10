using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidRios.Building.Building_Types
{
    public class StorageUpdateArgs
    {
        public ItemBundle resourcesChanged;
    }

    public class Storage : MonoBehaviour
    {
        public bool beingModified;

        public int numberOfPlots;

        //Number of units each plot can take. Ex: 50 = 50 stone bricks (1x1) & 25 logs (2x1).
        public int plotCapacity;

        public int totalCapacity;

        //Plot Positions
        public Vector3[] plotPositions;

        public bool debug;

        //Solely for inspector display
        private readonly Dictionary<string, Item> items = new Dictionary<string, Item>();

        private readonly Dictionary<int, int> numberOfItemsSpawnedPerPlot = new Dictionary<int, int>();

        private GameObject[] plotObjects;

        public List<List<Item>> plots = new List<List<Item>>();

        //Events

        /// <summary>
        ///     Made primarily to update city info resource values. Accounts for both the addition and removal of resources.
        /// </summary>
        public EventHandler<StorageUpdateArgs> storageUpdated;

        private readonly Dictionary<int, Queue<ItemBundle>> updateQueue = new Dictionary<int, Queue<ItemBundle>>();

        //Ensures 'Awake' method only called when the building has been placed.
        private void Awake()
        {
            StartCoroutine(Init());
        }

        //Wait until the building has finished its construction to begin.
        private IEnumerator Init()
        {
            yield return new WaitUntil(() => GetComponent<UnderConstruction>() == null);

            float storageWidth = GetComponent<InfrastructureBuilding>()._Building.width;

            var tempNumberOfPlots = Mathf.Pow(storageWidth, 2f) * 4;
            numberOfPlots = (int) tempNumberOfPlots;

            totalCapacity = numberOfPlots * plotCapacity;

            //Change this later when save/load is implemented
            for (var i = 0; i < numberOfPlots; i++) plots.Add(new List<Item>());

            //Instantiate Plots
            plotObjects = new GameObject[numberOfPlots];

            for (var i = 0; i < plotObjects.Length; i++)
            {
                var currentPlot = new GameObject();

                currentPlot.name = "Plot #" + (i + 1);

                //SET CURRENT PLOT POSITION HERE BASED ON ITS INDEX
                currentPlot.transform.localPosition = transform.position + plotPositions[i];

                currentPlot.transform.parent = transform;

                plotObjects[i] = currentPlot;

                numberOfItemsSpawnedPerPlot.Add(i, 0);
            }

            items.Add("Log", new Item {itemObject = Resources.Load<GameObject>("Prefabs/Items/Log"), itemType = "Log"});
        }

        /// <summary>
        ///     Updates the data for the storage's items. Adds to a dictionary that allows the storage to show the deposited items
        ///     once the UpdateAppearance() function is called.
        /// </summary>
        /// <param name="itemsToDeposit"></param>
        /// <param name="villagerIndex"></param>
        public void QueueItemsForDeposit(ItemBundle itemsToDeposit, int villagerIndex)
        {
            var itemType = itemsToDeposit.item.itemType;
            var itemAmount = itemsToDeposit.amount;

            var itemsLeftToStore = itemAmount;

            //Get plots to deposit in
            var matchingPlotIndices = ReturnPlotsByItemType(itemType, true);

            var plotsToDepositIn = new List<int>();

            //Make Dictionary with each plot index and the number of items that would go into each plot. Key: plotIndex. Value: item amount
            var itemsPerPlot = new Dictionary<int, int>();

            //Total item amount that can be stored in a given plot
            int amountOfItemsLeftInPlot;

            //Distribute items among storage plots
            for (var i = 0; i < matchingPlotIndices.Count; i++)
                if (itemsLeftToStore > 0)
                {
                    amountOfItemsLeftInPlot =
                        GetRemainingPlotSpace(matchingPlotIndices[i]) / ItemInfo.ItemSizes[itemType];

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
                {
                    break;
                }

            if (debug)
                Debug.Log("Amount of plots to use: " + itemsPerPlot.Count);

            //Each Plot
            foreach (var plot in itemsPerPlot)
            {
                var i = plot.Key;

                var beginningAmountOfItemsInPlot = GetAmountOfItemsInPlot(i);

                if (debug)
                    Debug.Log("Amount of items to deposit: " + plot.Value +
                              "; Number of items in plot before deposit: " + beginningAmountOfItemsInPlot);

                //Each Item
                for (var j = beginningAmountOfItemsInPlot + 1; j <= plot.Value + beginningAmountOfItemsInPlot; j++)
                {
                    if (debug)
                        Debug.Log("Plot #: " + i + "; Amount of items in plot: " + beginningAmountOfItemsInPlot);

                    plots[i - 1].Add(items[itemType]);
                }
            }

            var itemsDeposited = new ItemBundle {item = items[itemType], amount = itemAmount};

            var villagerItemQueue = new Queue<ItemBundle>();

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

        public void QueueItemsForWithdrawal(ItemBundle itemsToWithdraw, int villagerIndex)
        {
            var itemType = itemsToWithdraw.item.itemType;
            var itemAmount = itemsToWithdraw.amount;

            var itemsLeftToWithdraw = itemAmount;

            //Get plots to deposit in
            var matchingPlotIndices = ReturnPlotsByItemType(itemType, false, true);

            var plotsToWithdrawFrom = new List<int>();

            //Make Dictionary with each plot index and the number of items that would go into each plot. Key: plotIndex. Value: item amount
            var itemsToTakeFromEachPlot = new Dictionary<int, int>();

            //Total item amount that can be stored in a given plot
            int amountOfItemsToTakeFromPlot;

            //Distribute items among storage plots
            for (var i = matchingPlotIndices.Count - 1; i >= 0; i--)
                if (itemsLeftToWithdraw > 0)
                {
                    amountOfItemsToTakeFromPlot = GetAmountOfItemsInPlot(matchingPlotIndices[i]);

                    //Ignore this entire process if the plot is empty.
                    if (amountOfItemsToTakeFromPlot > 0)
                    {
                        if (amountOfItemsToTakeFromPlot > itemsLeftToWithdraw)
                            amountOfItemsToTakeFromPlot = itemsLeftToWithdraw;

                        itemsLeftToWithdraw -= amountOfItemsToTakeFromPlot;

                        itemsToTakeFromEachPlot.Add(matchingPlotIndices[i], amountOfItemsToTakeFromPlot);

                        plotsToWithdrawFrom.Add(matchingPlotIndices[i]);
                    }
                }
                else
                {
                    break;
                }

            if (debug)
                Debug.Log("Amount of plots to withdraw from: " + itemsToTakeFromEachPlot.Count);

            //Each Plot
            foreach (var plot in itemsToTakeFromEachPlot)
            {
                var plotIndex = plot.Key;

                var beginningAmountOfItemsInPlot = GetAmountOfItemsInPlot(plotIndex);

                if (debug)
                    Debug.Log("Amount of items to withdraw: " + plot.Value +
                              "; Number of items in plot before withdrawal: " + beginningAmountOfItemsInPlot);

                //Each Item
                for (var j = beginningAmountOfItemsInPlot + 1; j <= plot.Value + beginningAmountOfItemsInPlot; j++)
                    if (debug)
                        Debug.Log("Plot #: " + plotIndex + "; Amount of items in plot: " +
                                  beginningAmountOfItemsInPlot);

                //Debug.Log($"Plot #{plotIndex} Index Removed: {plots[plotIndex - 1].Count - 1}");
                //plots[plotIndex - 1].RemoveAt(plots[plotIndex - 1].Count - 1);
            }

            //A negative item amount tells the 'UpdateAppearance' function that items should be removed here.
            var itemsWithdrawn = new ItemBundle {item = items[itemType], amount = -itemAmount};

            var villagerItemQueue = new Queue<ItemBundle>();

            if (updateQueue.ContainsKey(villagerIndex))
            {
                villagerItemQueue = updateQueue[villagerIndex];

                villagerItemQueue.Enqueue(itemsWithdrawn);

                updateQueue[villagerIndex] = villagerItemQueue;
            }
            else
            {
                villagerItemQueue.Enqueue(itemsWithdrawn);

                updateQueue.Add(villagerIndex, villagerItemQueue);
            }
        }

        //Regenerate storage appearance based on new values (PLAN: UPDATE STORAGE APPEARANCE ONCE THE VILLAGER GETS THERE)
        public void UpdateAppearance(int villagerIndex)
        {
            StartCoroutine(UpdateAppearanceCoroutine(villagerIndex));
        }

        public IEnumerator UpdateAppearanceCoroutine(int villagerIndex)
        {
            //Rework this to allow for item removal.
            if (!updateQueue.ContainsKey(villagerIndex))
            {
                //If this request does not exist, make sure that I know that I queued the deposit before the deposit actually happened.
                Debug.LogError(
                    "Request Key does not exist. Remember to place a queue before making an attempt to deposit!");
                yield break;
            }

            var itemsToUpdate = updateQueue[villagerIndex].Dequeue();

            var itemType = itemsToUpdate.item.itemType;
            var itemObject = itemsToUpdate.item.itemObject;

            var itemAmount = itemsToUpdate.amount;

            //Only display the amount of items in the itemBundle.

            //Every Plot
            var addingItems = itemAmount > 0;

            var itemsLeftToChange = Mathf.Abs(itemAmount);

            if (addingItems)
            {
                foreach (var plot in plots)
                {
                    //If there are no more items to place, then leave!
                    if (itemsLeftToChange == 0)
                        break;

                    //Get the index of the plot so that its object can be accessed.
                    var plotIndex = plots.IndexOf(plot);

                    var isPlotEmpty = plot.Count == 0;

                    var doesPlotMatch = false;

                    if (isPlotEmpty)
                        doesPlotMatch = true;
                    else
                        doesPlotMatch = plot[0].itemType == itemType;

                    //Only continue if the item type is the same
                    if (doesPlotMatch)
                    {
                        //Get remaining amount of items available to fit in the plot
                        var amountOfSpaceItemTakesUp = ItemInfo.ItemSizes[itemType];
                        var amountOfItemsThatCanFit =
                            (plotCapacity - numberOfItemsSpawnedPerPlot[plotIndex] * amountOfSpaceItemTakesUp) /
                            amountOfSpaceItemTakesUp;
                        var amountOfItemsToPutInThisPlot = 0;

                        //If there's space for any items, then continue.
                        if (amountOfItemsThatCanFit > 0)
                        {
                            if (amountOfItemsThatCanFit >= itemsLeftToChange)
                                amountOfItemsToPutInThisPlot = itemsLeftToChange;
                            else
                                amountOfItemsToPutInThisPlot = amountOfItemsThatCanFit;

                            for (var i = 1; i <= amountOfItemsToPutInThisPlot; i++)
                            {
                                var item = Instantiate(itemObject, plotObjects[plotIndex].transform);

                                var itemNumber = numberOfItemsSpawnedPerPlot[plotIndex] + 1;

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

                                itemsLeftToChange--;
                            }
                        }
                    }
                }

                storageUpdated?.Invoke(this, new StorageUpdateArgs {resourcesChanged = itemsToUpdate});
            }
            else //Removing Items
            {
                //Cycle backwards through all of the plots
                for (var plotIndex = plots.Count - 1; plotIndex >= 0; plotIndex--)
                {
                    while (numberOfItemsSpawnedPerPlot[plotIndex] != plots[plotIndex].Count)
                    {
                        JobManager.villagers[villagerIndex].customJobDescription =
                            "Waiting for other villagers to finish depositing.";
                        yield return null;
                    }

                    JobManager.villagers[villagerIndex].ResetCustomJobDescription();

                    //If plot is empty, then skip it.
                    if (plots[plotIndex].Count > 0)
                    {
                        var doesPlotMatch = plots[plotIndex][0].itemType == itemType;

                        if (doesPlotMatch)
                        {
                            //Get the amount of items that need to be removed
                            var amountOfItemsToRemove = itemsLeftToChange;
                            int amountOfItemsAbleToRemove;
                            var amountOfItemsInPlot = GetAmountOfItemsInPlot(plotIndex + 1);

                            if (amountOfItemsToRemove > amountOfItemsInPlot)
                                amountOfItemsAbleToRemove = amountOfItemsInPlot;
                            else
                                amountOfItemsAbleToRemove = amountOfItemsToRemove;

                            for (var i = 0; i < amountOfItemsAbleToRemove; i++)
                            {
                                //Debug.Log("Plot #" + (plotIndex + 1) + ";" + (plotObjects[plotIndex].transform.childCount - 1) + "; " + numberOfItemsSpawnedPerPlot[plotIndex]);

                                numberOfItemsSpawnedPerPlot[plotIndex] -= 1;

                                Destroy(plotObjects[plotIndex].transform
                                    .GetChild(numberOfItemsSpawnedPerPlot[plotIndex]).gameObject);

                                JobManager.villagers[villagerIndex].items.Add(plots[plotIndex][0]);

                                JobManager.villagers[villagerIndex].DisplayItems();

                                plots[plotIndex].RemoveAt(plots[plotIndex].Count - 1);

                                itemsLeftToChange--;
                            }
                        }
                    }

                    if (itemsLeftToChange == 0)
                        break;
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

            var logYMultiplier = (logNumber + 1) / 2;
            logYMultiplier--;

            //Examples: 1, 5, 9, 13, 17, etc. 
            if ((logNumber - 1) % 4 == 0)
                _log.transform.localPosition += new Vector3(0f, 0.14f * logYMultiplier, -0.1f);

            //Examples: 2, 6, 10, 14, 18, etc.
            if ((logNumber - 2) % 4 == 0) _log.transform.localPosition += new Vector3(0f, 0.14f * logYMultiplier, 0.1f);

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

        /// <summary>
        ///     Returns the number of items left that fit in the storage unit depending on the item type provided.
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public int GetSpaceForItemsByItemType(string itemType)
        {
            var remainingSpace = 0;

            var matchingPlots = ReturnPlotsByItemType(itemType, true);

            for (var i = 0; i < matchingPlots.Count; i++) remainingSpace += GetRemainingPlotSpace(matchingPlots[i]);

            return remainingSpace / ItemInfo.ItemSizes[itemType];
        }

        /// <summary>
        ///     Returns the amount of items of a certain type in the storage.
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public int GetStoredItemAmountByItemType(string itemType)
        {
            var matchingPlots = ReturnPlotsByItemType(itemType, false, true);

            var totalItemAmount = 0;

            for (var i = 0; i < matchingPlots.Count; i++) totalItemAmount += GetAmountOfItemsInPlot(matchingPlots[i]);

            return totalItemAmount;
        }

        #region Old Code

        /*Update this to QueueItemsForWithdrawal?
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
    */

        #endregion

        #region Additional Functions

        private bool IsPlotFull(int plotIndex)
        {
            //If plotIndex is 1, then it will return plots[0].Count (number of items in plot)
            if (plots[plotIndex - 1].Count * ItemInfo.ItemSizes[ReturnPlotItemType(plotIndex)] < plotCapacity)
                return false;
            return true;
        }

        private bool IsPlotEmpty(int plotindex)
        {
            return plots[plotindex - 1].Count == 0;
        }

        private int GetAmountOfItemsInPlot(int plotIndex)
        {
            return plots[plotIndex - 1].Count;
        }

        private int GetRemainingPlotSpace(int plotIndex)
        {
            var numberOfItemsInPlot = plots[plotIndex - 1].Count;

            if (numberOfItemsInPlot == 0)
                return plotCapacity;

            var itemTypeSize = ItemInfo.ItemSizes[ReturnPlotItemType(plotIndex)];

            return plotCapacity - numberOfItemsInPlot * itemTypeSize;
        }

        //Returns the itemtype of the first item in the selected plot.
        private string ReturnPlotItemType(int plotIndex)
        {
            return plots[plotIndex - 1][0].itemType;
        }

        private List<int> ReturnPlotsByItemType(string itemType, bool addEmptyPlots = false, bool addFullPlots = false)
        {
            var matchingPlotIndices = new List<int>();

            for (var i = 1; i <= plots.Count; i++)
                if (IsPlotEmpty(i) && addEmptyPlots)
                    matchingPlotIndices.Add(i);
                else if (!IsPlotEmpty(i))
                    if (ReturnPlotItemType(i) == itemType)
                        if (IsPlotFull(i) && addFullPlots || !IsPlotFull(i))
                            matchingPlotIndices.Add(i);

            return matchingPlotIndices;
        }

        private List<int> ReturnEmptyPlots()
        {
            var matchingPlotIndices = new List<int>();

            for (var i = 1; i <= plots.Count; i++)
                if (IsPlotEmpty(i))
                    matchingPlotIndices.Add(i);

            return matchingPlotIndices;
        }

        #endregion
    }
}