using System;
using DavidRios.Villager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DavidRios.Assets.Scripts.Villager
{
    public class VillagerPropertiesGenerator
    {
        private static readonly string[] femaleNames =
        {
            "Aethelu", "Agnes", "Alba", "Ava", "Beatrice", "Beverly", "Cecily", "Edith", "Ella", "Emma", "Isabella",
            "Margery", "Matilda", "Merry", "Odilia", "Reina", "Rhoslyn", "Trea", "Juanita"
        };

        private static readonly string[] maleNames =
        {
            "Aldous", "Alistair", "Bennett", "Conrad", "Constantine", "Dietrich", "Drake", "Everard", "Gawain", "Godwin",
            "Jeffery", "Joachim", "Ladislas", "Luther", "Milo", "Odo", "Percival", "Robin", "Wade", "Wolfgang", "Amadeus",
            "Warner"
        };

        public static string GenerateName(VillagerLogic villagerLogic)
        {
            int nameIndex;

            if (villagerLogic.sex == "Female")
            {
                nameIndex = Mathf.RoundToInt(Random.Range(0, femaleNames.Length));

                return femaleNames[nameIndex];
            }

            nameIndex = Mathf.RoundToInt(Random.Range(0, maleNames.Length));

            return maleNames[nameIndex];
        }

        public static string GenerateSex(VillagerLogic villagerLogic)
        {
            var genderDecider = Mathf.RoundToInt(Random.Range(0, 1));

            if (genderDecider == 1)
                return "Female";
            return "Male";
        }

        public static string CurrentJobDescription(VillagerLogic villagerLogic)
        {
            if (villagerLogic.jobList.Count == 0)
                return "Idle";

            var job = villagerLogic.jobList[0];

            var suffix = "";

            if (job.amounts != null)
                if (job.amounts.Length > 0)
                    suffix = job.amounts[0] > 0 ? "s" : "";

            switch (job.jobType)
            {
                case "Move":
                    if (job.objectiveTransforms != null)
                    {
                        if (job.objectiveTransforms.Length > 0)
                            return "walking to " + job.objectiveTransforms[0].name;
                        return "walking somewhere";
                    }
                    else
                    {
                        return "walking somewhere";
                    }
                case "Chop":
                    return "chopping tree.";
                case "Mine":
                    return "mining mine.";
                case "Build":
                    return "building something.";
                case "Deposit":
                    return $"transferring {job.amounts[0]} item{suffix} to storage.";
                case "Withdraw":
                    return $"withdrawing {job.amounts[0]} item{suffix} from storage.";
                case "DepositToBuildSite":
                    return $"Placing {job.amounts[0]} item{suffix} in a build site.";
                case "TakeFromItemPile":
                    return "picking up a pile of items.";
                default:
                    Debug.LogError("Invalid job type: " + job.jobType);
                    return "Who knows?";
            }
        }

        public static string ProcessRole(int roleID)
        {
            return Enum.GetName(typeof(VillagerRoles), roleID);
        }
    }
}