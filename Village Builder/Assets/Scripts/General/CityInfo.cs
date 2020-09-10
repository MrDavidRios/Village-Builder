public class CityInfo
{
    public static string CityName;

    public static int Population;
    public static int Happiness;

    public static int Time;

    //Resources
    public static ItemBundle[] CityResources =
    {
        //Implement a different way to initialize this when save/load system is implemented.

        new ItemBundle {item = new Item {itemObject = null, itemType = "Log"}, amount = 0},
        new ItemBundle {item = new Item {itemObject = null, itemType = "Stone"}, amount = 0},
        new ItemBundle {item = new Item {itemObject = null, itemType = "StoneBrick"}, amount = 0},
        new ItemBundle {item = new Item {itemObject = null, itemType = "IronOre"}, amount = 0},
        new ItemBundle {item = new Item {itemObject = null, itemType = "Iron"}, amount = 0},
        new ItemBundle {item = new Item {itemObject = null, itemType = "Tools"}, amount = 0},
        new ItemBundle {item = new Item {itemObject = null, itemType = "Food"}, amount = 0}
    };

    /*
    public static ItemBundle[] _cityResourcesLeftForBuilding = new ItemBundle[]
    {
        //Implement a different way to initialize this when save/load system is implemented.

        new ItemBundle { item = new Item { itemObject = null, itemType = "Log" }, amount = 0 },
        new ItemBundle { item = new Item { itemObject = null, itemType = "Stone" }, amount = 0 },
        new ItemBundle { item = new Item { itemObject = null, itemType = "StoneBrick" }, amount = 0 },
        new ItemBundle { item = new Item { itemObject = null, itemType = "IronOre" }, amount = 0 },
        new ItemBundle { item = new Item { itemObject = null, itemType = "Iron" }, amount = 0 },
        new ItemBundle { item = new Item { itemObject = null, itemType = "Tools" }, amount = 0 },
        new ItemBundle { item = new Item { itemObject = null, itemType = "Food" }, amount = 0 }
    };
    */

    public static void AddResource(int itemIndex, int amount)
    {
        CityResources[itemIndex].amount += amount;
        //_cityResourcesLeftForBuilding[itemIndex].amount += amount;
    }

    public static void RemoveResource(int itemIndex, int amount)
    {
        CityResources[itemIndex].amount -= amount;
    }

    /*
    public static int _wood;
    public static int _stone;
    public static int _food;
    public static int _charcoal;
    public static int _stoneBricks;
    public static int _ironOre;
    public static int _iron;
    public static int _tools;
    */
}