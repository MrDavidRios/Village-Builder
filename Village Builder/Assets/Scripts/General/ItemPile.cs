using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ItemPile : MonoBehaviour
{
    public bool beingPickedUp;

    public int amountOfItems;

    /// <summary>
    /// Initial value. This should not be modified in the script.
    /// </summary>
    private const int minutesToDespawn = 5;

    public int despawnSeconds;
    public int despawnMinutes;

    private void Awake()
    { 
        amountOfItems = transform.childCount;

        switch (tag)
        {
            case "Tree":
                tag = "Log";
                break;
            default:
                Debug.LogError("Undefined item type:" + tag);
                break;
        }

        StartCoroutine(UpdateTimeToDespawn());
    }

    //Returns itemType.
    public string TakeItem(bool lastTake)
    {
        amountOfItems--;

        Transform itemToRemove = transform.GetChild(transform.childCount - 1);

        GameObject.Destroy(itemToRemove.gameObject);

        if (lastTake && amountOfItems > 0)
            beingPickedUp = false;

        return tag;
    }

    IEnumerator UpdateTimeToDespawn() 
    {
        int timeInSeconds = minutesToDespawn * 60;

        despawnMinutes = minutesToDespawn;
        despawnSeconds = 0;

        //Update time left to despawn.
        for (int timeLeft = 0; timeLeft < timeInSeconds; timeLeft++)
        {
            yield return new WaitForSeconds(1);

            if (despawnSeconds != 0)
                despawnSeconds--;
            else
            {
                if (despawnMinutes == 0)
                    yield break;

                despawnMinutes--;
                despawnSeconds = 59;
            }
        }

        GameObject.Destroy(gameObject);
    }
}
