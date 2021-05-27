using System.Collections.Generic;
using System.Linq;
using DavidRios.Building;
using DavidRios.Building.Building_Types;
using UnityEngine;

//This script is to help optimize the distribution of jobs among the villagers. (Prevents one villager from hogging everything)
public class HiveMind : MonoBehaviour
{
    //Variables
    public List<Villager> idleVillagers = new List<Villager>();
    private ItemPileManager itemPileManager;

    //Scripts
    private JobManager jobManager;

    private void Awake()
    {
        jobManager = FindObjectOfType<JobManager>();
        itemPileManager = FindObjectOfType<ItemPileManager>();
    }

    private void Update()
    {
        foreach (Transform child in transform)
            if (child.gameObject.activeInHierarchy)
            {
                //If the selected villager currently has no jobs and is not marked as idle, mark them as idle.
                if (child.GetComponent<Villager>().jobList.Count == 0 &&
                    !idleVillagers.Contains(child.GetComponent<Villager>()))
                    idleVillagers.Add(child.GetComponent<Villager>());

                //If the villager has jobs to do and is marked as idle, remove them from the idle list.
                else if (child.GetComponent<Villager>().jobList.Count > 0 &&
                         idleVillagers.Contains(child.GetComponent<Villager>()))
                    idleVillagers.Remove(child.GetComponent<Villager>());
            }

        #region Pile Logic

        //Decide which villager would do the job the fastest based on their current position
        /*
         * Get the item pile if one exists
         * Get the closest idle villager to it and make them pick up the pile
        */

        //If there are any piles that are not currently being picked up, look for a villager to assign it to.
        Transform itemPileTransform = null;
        ItemPile itemPile = null;

        var villagerDistances = new Dictionary<int, float>();

        if (itemPileManager.untendedPiles.Count > 0)
        {
            itemPileTransform = itemPileManager.GetNextAvailableItemPile();

            if (itemPileTransform != null)
            {
                itemPile = itemPileTransform.GetComponent<ItemPile>();

                for (var j = 0; j < idleVillagers.Count; j++)
                    villagerDistances.Add(j,
                        Vector3.Distance(idleVillagers[j].transform.position, itemPileTransform.position));
            }
        }

        if (villagerDistances.Count > 0 && itemPile != null)
        {
            var closestVillagerDictionaryIndex = villagerDistances.OrderBy(kvp => kvp.Value).First().Key;

            var villager = idleVillagers[closestVillagerDictionaryIndex];

            if (StorageManager.SpaceLeftForItemBundle(itemPile.tag, itemPile.amountOfItems))
                PickUpPile(villager, itemPile);
        }

        #endregion
    }

    private void PickUpPile(Villager villager, ItemPile pile)
    {
        //Test out removing these variables and just casting them to floats
        float itemPileAmount = pile.amountOfItems;
        float inventoryCapacity = villager.inventoryCapacity;

        //Amount of times the villager needs to go back to pick up the entire pile (if the pile has 10 items and their inventory capacity is 5, they would have to go back twice)
        var amountOfTimesToLoop = (int) Mathf.Ceil(itemPileAmount / inventoryCapacity);

        for (var i = 0; i < amountOfTimesToLoop; i++)
        {
            //Amount of items that the villager will pick up from the pile this time around. If the villager's inventory capacity is less than the amount in the item pile, they will take as much as they can. Otherwise, they'll take the whole pile.
            //This is a total amount distributed across all of the storages the villager will use.
            var amount = itemPileAmount > villager.inventoryCapacity
                ? villager.inventoryCapacity
                : (int) itemPileAmount;

            //Assign the job of picking up the pile and mark the pile as being picked up.
            jobManager.AssignJobGroup("PickUpPile", pile.transform.position, new Transform[1] {pile.transform},
                new int[1] {amount}, villager._index);

            pile.beingPickedUp = true;

            var itemType = pile.tag;

            //Get all of the storages that the villager would need to use
            var storagesToUse = JobUtils.GetNearestAvailableStorages(villager, itemType, (int) itemPileAmount);

            var amountLeftToDeposit = amount;

            for (var storageIndex = 0; storageIndex < storagesToUse.Length; storageIndex++)
            {
                //Amount of space left for these items in storage
                var amountForThisStorage = storagesToUse[storageIndex].GetComponent<Storage>()
                    .GetSpaceForItemsByItemType(itemType);

                if (amountForThisStorage > amountLeftToDeposit || amountForThisStorage == amountLeftToDeposit)
                    amountForThisStorage = amountLeftToDeposit;

                amountLeftToDeposit -= amountForThisStorage;

                var itemCollection = new ItemBundle
                {
                    item = new Item {itemObject = pile.transform.GetChild(0).gameObject, itemType = itemType},
                    amount = amountForThisStorage
                };

                storagesToUse[storageIndex].GetComponent<Storage>()
                    .QueueItemsForDeposit(itemCollection, villager._index);

                //Assign the job of depositing the items.
                jobManager.AssignJobGroup("Deposit", storagesToUse[storageIndex].transform.position,
                    new Transform[1] {storagesToUse[storageIndex]}, new int[1] {amountForThisStorage}, villager._index);
            }

            itemPileAmount -= amount;
        }
    }
}