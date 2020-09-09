using System.Collections;
using UnityEngine;

public class ItemPile : MonoBehaviour
{
    /// <summary>
    ///     Initial value. This should not be modified in the script.
    /// </summary>
    private const int minutesToDespawn = 5;

    public bool beingPickedUp;

    public int amountOfItems;

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

        var itemToRemove = transform.GetChild(amountOfItems);

        Destroy(itemToRemove.gameObject);

        return tag;
    }

    private IEnumerator UpdateTimeToDespawn()
    {
        var timeInSeconds = minutesToDespawn * 60;

        despawnMinutes = minutesToDespawn;
        despawnSeconds = 0;

        //Update time left to despawn.
        for (var timeLeft = 0; timeLeft < timeInSeconds; timeLeft++)
        {
            yield return new WaitForSeconds(1);

            if (despawnSeconds != 0)
            {
                despawnSeconds--;
            }
            else
            {
                if (despawnMinutes == 0)
                    yield break;

                despawnMinutes--;
                despawnSeconds = 59;
            }
        }

        if (!beingPickedUp)
            Destroy(gameObject);
    }
}