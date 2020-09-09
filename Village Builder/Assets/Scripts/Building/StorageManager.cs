using System.Collections.Generic;
using DavidRios.Building.Building_Types;
using UnityEngine;

namespace DavidRios.Building
{
    public class StorageManager : MonoBehaviour
    {
        public static List<Transform> storages = new List<Transform>();

        public bool debug;

        public void Update()
        {
            foreach (Transform child in transform)
                if (!storages.Contains(child) && child.GetComponent<UnderConstruction>() == null)
                    InitializeNewStorage(child);
        }

        private void InitializeNewStorage(Transform storageTransform)
        {
            storageTransform.GetComponent<Storage>().storageUpdated += UpdateCityInfoStorageValues;

            storages.Add(storageTransform);
        }

        private void UpdateCityInfoStorageValues(object sender, StorageUpdateArgs updatedItems)
        {
            //Modify city info resource values.
            var itemType = updatedItems.resourcesChanged.item.itemType;
            var itemAmount = updatedItems.resourcesChanged.amount;

            if (debug)
            {
                var resourceNameSuffix = Mathf.Abs(itemAmount) > 1 ? "s" : "";

                if (itemAmount > 0)
                    Debug.Log($"{itemAmount} {itemType}{resourceNameSuffix} added.");
                else
                    Debug.Log($"{Mathf.Abs(itemAmount)} {itemType}{resourceNameSuffix} removed.");
            }

            CityInfo.AddResource(ItemInfo.GetItemIndex(itemType), itemAmount);
        }

        public static bool EnoughResources(ItemBundle[] requiredItems)
        {
            for (var i = 0; i < requiredItems.Length; i++)
                if (CityInfo._cityResources[ItemInfo.GetItemIndex(requiredItems[i].item.itemType)].amount <
                    requiredItems[i].amount)
                    return false;

            return true;
        }

        public static bool SpaceLeftForItemBundle(string itemType, int itemAmount)
        {
            if (storages.Count == 0)
                return false;

            var spaceLeft = 0;

            for (var i = 0; i < storages.Count; i++)
                spaceLeft += storages[i].GetComponent<Storage>().GetSpaceForItemsByItemType(itemType);

            if (spaceLeft > itemAmount)
                return true;
            return false;
        }
    }
}