using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

//This script is to help optimize the distribution of jobs among the villagers. (Prevents one villager from hogging everything)
public class HiveMind : MonoBehaviour
{
    //Variables
    public List<Villager> idleVillagers = new List<Villager>();

    //Scripts
    private JobManager jobManager;
    private ItemPileManager itemPileManager;

    private void Awake()
    {
        jobManager = FindObjectOfType<JobManager>();
        itemPileManager = FindObjectOfType<ItemPileManager>();
    }

    void Update()
    {
        foreach (Transform child in transform)
        {
            //If the selected villager currently has no jobs and is not marked as idle, mark them as idle.
            if (child.GetComponent<Villager>().jobList.Count == 0 && !idleVillagers.Contains(child.GetComponent<Villager>()))
                idleVillagers.Add(child.GetComponent<Villager>());

            //If the villager has jobs to do and is marked as idle, remove them from the idle list.
            else if (child.GetComponent<Villager>().jobList.Count > 0 && idleVillagers.Contains(child.GetComponent<Villager>()))
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

        Dictionary<int, float> villagerDistances = new Dictionary<int, float>();

        if (itemPileManager.untendedPiles.Count > 0)
        {
            itemPileTransform = itemPileManager.GetNextAvailableItemPile();

            if (itemPileTransform != null)
            {
                itemPile = itemPileTransform.GetComponent<ItemPile>();

                for (int j = 0; j < idleVillagers.Count; j++)
                {
                    villagerDistances.Add(j, Vector3.Distance(idleVillagers[j].transform.position, itemPileTransform.position));
                }
            }
        }

        if (villagerDistances.Count > 0 && itemPile != null)
        {
            int closestVillagerDictionaryIndex = villagerDistances.OrderBy(kvp => kvp.Value).First().Key;

            Villager villager = idleVillagers[closestVillagerDictionaryIndex];

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
        int amountOfTimesToLoop = (int)Mathf.Ceil(itemPileAmount / inventoryCapacity);

        for (int i = 0; i < amountOfTimesToLoop; i++)
        {
            //Amount of items that the villager will pick up from the pile this time around. If the villager's inventory capacity is less than the amount in the item pile, they will take as much as they can. Otherwise, they'll take the whole pile.
            //This is a total amount distributed across all of the storages the villager will use.
            int amount = itemPileAmount > villager.inventoryCapacity ? villager.inventoryCapacity : (int)itemPileAmount;

            //Assign the job of picking up the pile and mark the pile as being picked up.
            jobManager.AssignJobGroup("PickUpPile", pile.transform.position, new Transform[1] { pile.transform }, new int[1] { amount }, villager.index);

            pile.beingPickedUp = true;

            string itemType = pile.tag;

            //Get all of the storages that the villager would need to use
            Transform[] storagesToUse = JobUtils.GetNearestAvailableStorages(villager, itemType, (int)itemPileAmount);

            int amountLeftToDeposit = amount;

            for (int storageIndex = 0; storageIndex < storagesToUse.Length; storageIndex++)
            {
                //Amount of space left for these items in storage
                int amountForThisStorage = storagesToUse[storageIndex].GetComponent<Storage>().GetSpaceForItemsByItemType(itemType);

                if (amountForThisStorage > amountLeftToDeposit || amountForThisStorage == amountLeftToDeposit)
                    amountForThisStorage = amountLeftToDeposit;

                amountLeftToDeposit -= amountForThisStorage;

                ItemBundle itemCollection = new ItemBundle { item = new Item { itemObject = pile.transform.GetChild(0).gameObject, itemType = itemType }, amount = amountForThisStorage };

                storagesToUse[storageIndex].GetComponent<Storage>().QueueItemsForDeposit(itemCollection, villager.index);

                //Assign the job of depositing the items.
                jobManager.AssignJobGroup("Deposit", storagesToUse[storageIndex].transform.position, new Transform[1] { storagesToUse[storageIndex] }, new int[1] { amountForThisStorage }, villager.index);
            }

            itemPileAmount -= amount;
        }
    }
}
