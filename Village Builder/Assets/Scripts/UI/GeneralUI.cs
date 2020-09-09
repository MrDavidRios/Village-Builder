using DavidRios.Building;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UITypes
{
    BuildingResourceInfo,
    CityResourceInfo
}

public class GeneralUI : MonoBehaviour
{
    public UITypes UIType;

    public bool updateValues;

    #region City Resource UI Fields

    public string itemType;

    #endregion

    private void Awake()
    {
        parentButton = transform.parent.parent.parent.GetComponent<Button>();

        UpdateValues();
    }

    private void Update()
    {
        if (updateValues)
            UpdateValues();
    }

    private void UpdateValues()
    {
        switch (UIType)
        {
            case UITypes.BuildingResourceInfo:
            {
                var requiredItems = buildingPrefab.GetComponent<UnderConstruction>().requiredItems;

                if (requiredItems.Length >= itemIndex + 1)
                {
                    //Building requires this resource

                    var requiredItemAmount = requiredItems[itemIndex].amount;

                    GetComponent<TMP_Text>().text = requiredItemAmount.ToString();

                    if (CityInfo._cityResources[itemIndex].amount >= requiredItemAmount)
                    {
                        //Sufficient items
                        GetComponent<TMP_Text>().color = sufficientItemsColor;

                        //Make it so that this is only called once.
                        if (itemIndex == 0)
                            if (!buttonActionSet)
                            {
                                parentButton.onClick.AddListener(() => TemplateActions.SetTemplate(buildingPrefab));
                                buttonActionSet = true;
                            }
                    }
                    else
                    {
                        //Insufficient items
                        GetComponent<TMP_Text>().color = insufficientItemsColor;

                        //Make it so that this is only called once.
                        if (itemIndex == 0)
                        {
                            parentButton.onClick.RemoveAllListeners();

                            parentButton.onClick.AddListener(() => TemplateActions.ClearTemplate());

                            buttonActionSet = false;
                        }
                    }
                }
                else if (requiredItems.Length == 0)
                {
                    //Building does not require this resource
                    GetComponent<TMP_Text>().text = "0";
                    GetComponent<TMP_Text>().color = Color.white;

                    //Make it so that this is only called once.
                    if (itemIndex == 0)
                        if (!buttonActionSet)
                        {
                            parentButton.onClick.AddListener(() => TemplateActions.SetTemplate(buildingPrefab));
                            buttonActionSet = true;
                        }
                }
                else
                {
                    //Building does not require this resource
                    GetComponent<TMP_Text>().text = "0";
                    GetComponent<TMP_Text>().color = Color.white;
                }

                break;
            }
            case UITypes.CityResourceInfo:
            {
                var itemIndex = ItemInfo.GetItemIndex(itemType);

                if (CityInfo._cityResources.Length < itemIndex)
                {
                    Debug.LogError("Item not defined in the city's resources array.");
                    break;
                }

                GetComponent<TMP_Text>().text = CityInfo._cityResources[itemIndex].amount.ToString();
                break;
            }
            default:
                Debug.LogError("Invalid UI Type: " + UIType);
                break;
        }
    }

    #region Building UI Fields

    [DrawIf("UIType", UITypes.BuildingResourceInfo)]
    public GameObject buildingPrefab;

    [DrawIf("UIType", UITypes.BuildingResourceInfo)]
    public int itemIndex;

    [DrawIf("UIType", UITypes.BuildingResourceInfo)]
    public Color sufficientItemsColor;

    [DrawIf("UIType", UITypes.BuildingResourceInfo)]
    public Color insufficientItemsColor;

    private Button parentButton;

    private bool buttonActionSet;

    #endregion
}