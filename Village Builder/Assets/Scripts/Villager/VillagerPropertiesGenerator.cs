using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerPropertiesGenerator
{
    private static string[] femaleNames = new string[] { "Aethelu", "Agnes", "Alba", "Ava", "Beatrice", "Beverly", "Cecily", "Edith", "Ella", "Emma", "Isabella", "Margery", "Matilda", "Merry", "Odilia", "Reina", "Rhoslyn", "Trea", "Juanita" };

    private static string[] maleNames = new string[] { "Aldous", "Alistair", "Bennett", "Conrad", "Constantine", "Dietrich", "Drake", "Everard", "Gawain", "Godwin", "Jeffery", "Joachim", "Ladislas", "Luther", "Milo", "Odo", "Percival", "Robin", "Wade", "Wolfgang", "Amadeus", "Warner" };

    public static string GenerateName(Villager villager)
    {
        int nameIndex;

        if (villager._gender == "Female")
        {
            nameIndex = Mathf.RoundToInt(Random.Range(0, femaleNames.Length));

            return femaleNames[nameIndex];
        }
        else
        {
            nameIndex = Mathf.RoundToInt(Random.Range(0, maleNames.Length));

            return maleNames[nameIndex];
        }
    }

    public static string GenerateGender(Villager villager)
    {
        int genderDecider = Mathf.RoundToInt(Random.Range(0, 1));

        if (genderDecider == 1) 
            return "Female";
        else 
            return "Male";
    }

    public static string CurrentJobDescription(Villager villager)
    {
        if (villager.jobList.Count == 0)
            return "Idle";

        Job job = villager.jobList[0];

        switch (job.jobType)
        {
            case "Move":
                if (job.objectiveTransform != null)
                    return "walking to " + job.objectiveTransform.name;
                else
                    return "walking somewhere";
            case "Chop":
                return "chopping tree.";
            case "Mine":
                return "mining mine.";
            case "Construct":
                return "building something.";
            case "Deposit":
                return "transferring " + job.amount + " items to storage.";
            default:
                Debug.LogError("Invalid job type: " + job.jobType);
                return "Who knows?";
        }
    }
}
