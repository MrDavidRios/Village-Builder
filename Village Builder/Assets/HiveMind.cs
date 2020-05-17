using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (child.GetComponent<Villager>().jobList.Count == 0 && !idleVillagers.Contains(child.GetComponent<Villager>()))
                idleVillagers.Add(child.GetComponent<Villager>());
            else if (child.GetComponent<Villager>().jobList.Count > 0 && idleVillagers.Contains(child.GetComponent<Villager>()))
                idleVillagers.Remove(child.GetComponent<Villager>());
        }

        for (int i = 0; i < idleVillagers.Count; i++)
        {
            if (itemPileManager.untendedPiles.Count > 0)
            {
                if (idleVillagers[i].items.Count == 0 && idleVillagers[i]._role == "Laborer")
                {
                    Transform pileTransform = itemPileManager.GetNextAvailableItemPile();

                    if (pileTransform != null)
                    {
                        ItemPile pile = pileTransform.GetComponent<ItemPile>();

                        if (StorageManager.SpaceLeftForItemBundle(pile.tag, pile.amountOfItems))
                        {
                            PickUpPile(idleVillagers[i], pile);

                            string itemType = pile.tag;

                            Villager villager = idleVillagers[i];

                            Transform[] storagesToUse = JobUtils.GetNearestAvailableStorages(villager, itemType, pile.amountOfItems);

                            int amount = pile.amountOfItems;

                            int amountLeftToDeposit = amount;

                            for (int j = 0; j < storagesToUse.Length; j++)
                            {
                                //Amount of space left for these items in storage
                                int amountForThisStorage = storagesToUse[j].GetComponent<Storage>().GetSpaceForItemsByItemType(itemType);

                                if (amountForThisStorage > amountLeftToDeposit || amountForThisStorage == amountLeftToDeposit)
                                    amountForThisStorage = amountLeftToDeposit;

                                amountLeftToDeposit -= amountForThisStorage;

                                ItemBundle itemCollection = new ItemBundle { item = new Item { itemObject = pile.transform.GetChild(0).gameObject, itemType = itemType }, amount = amountForThisStorage };

                                storagesToUse[j].GetComponent<Storage>().DepositItem(itemCollection, villager.index);

                                //AssignJobGroup Deposit
                                jobManager.AssignJobGroup("Deposit", storagesToUse[j].transform.position, new Transform[1] { storagesToUse[j] }, new int[1] { amountForThisStorage }, idleVillagers[i].index);
                            }
                        }
                    }
                }
            }
        }
    }

    private void PickUpPile(Villager villager, ItemPile pile)
    {
        jobManager.AssignJobGroup("PickUpPile", pile.transform.position, new Transform[1] { pile.transform }, null, villager.index);

        pile.beingPickedUp = true;
    }
}
