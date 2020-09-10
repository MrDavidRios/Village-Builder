using System;
using DavidRios.Building;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DavidRios.UI
{
    public enum UITypes
    {
        BuildingResourceInfo,
        CityResourceInfo
    }

    public class GeneralUI : MonoBehaviour
    {
        [LabelOverride("UI Type")] public UITypes uiType;

        public bool updateValues;
        
        [DrawIf("uiType", UITypes.CityResourceInfo)]
        public string itemType;

        #region Building UI Fields

        [DrawIf("uiType", UITypes.BuildingResourceInfo)]
        public GameObject buildingPrefab;

        private UnderConstruction _buildingPrefabUnderConstruction;

        [DrawIf("uiType", UITypes.BuildingResourceInfo)]
        public int itemIndex;

        [DrawIf("uiType", UITypes.BuildingResourceInfo)]
        public Color sufficientItemsColor;

        [DrawIf("uiType", UITypes.BuildingResourceInfo)]
        public Color insufficientItemsColor;

        private Button _parentButton;

        private bool _buttonActionSet;

        private TMP_Text _uiText;
        private int _itemIndex;

        #endregion
        
        private void Awake()
        {
            _parentButton = transform.parent.parent.parent.GetComponent<Button>();

            switch (uiType)
            {
                case UITypes.CityResourceInfo:
                    _itemIndex = ItemInfo.GetItemIndex(itemType);
                    break;
                case UITypes.BuildingResourceInfo:
                    _buildingPrefabUnderConstruction = buildingPrefab.GetComponent<UnderConstruction>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _uiText = GetComponent<TMP_Text>();

            UpdateValues();
        }

        private void Update()
        {
            if (updateValues)
                UpdateValues();
        }

        private void UpdateValues()
        {
            switch (uiType)
            {
                case UITypes.BuildingResourceInfo:
                {
                    var requiredItems = _buildingPrefabUnderConstruction.requiredItems;

                    if (requiredItems.Length >= itemIndex + 1)
                    {
                        //Building requires this resource

                        var requiredItemAmount = requiredItems[itemIndex].amount;

                        _uiText.text = requiredItemAmount.ToString();

                        if (CityInfo.CityResources[itemIndex].amount >= requiredItemAmount)
                        {
                            //Sufficient items
                            _uiText.color = sufficientItemsColor;

                            //Make it so that this is only called once.
                            if (itemIndex == 0)
                                if (!_buttonActionSet)
                                {
                                    _parentButton.onClick.AddListener(() => TemplateActions.SetTemplate(buildingPrefab));
                                    _buttonActionSet = true;
                                }
                        }
                        else
                        {
                            //Insufficient items
                            _uiText.color = insufficientItemsColor;

                            //Make it so that this is only called once.
                            if (itemIndex == 0)
                            {
                                _parentButton.onClick.RemoveAllListeners();

                                _parentButton.onClick.AddListener(TemplateActions.ClearTemplate);

                                _buttonActionSet = false;
                            }
                        }
                    }
                    else if (requiredItems.Length == 0)
                    {
                        //Building does not require this resource
                        _uiText.text = "0";
                        _uiText.color = Color.white;

                        //Make it so that this is only called once.
                        if (itemIndex == 0)
                            if (!_buttonActionSet)
                            {
                                _parentButton.onClick.AddListener(() => TemplateActions.SetTemplate(buildingPrefab));
                                _buttonActionSet = true;
                            }
                    }
                    else
                    {
                        //Building does not require this resource
                        _uiText.text = "0";
                        _uiText.color = Color.white;
                    }

                    break;
                }
                case UITypes.CityResourceInfo:
                {
                    if (CityInfo.CityResources.Length < _itemIndex)
                    {
                        Debug.LogError("Item not defined in the city's resources array.");
                        break;
                    }

                    _uiText.text = CityInfo.CityResources[_itemIndex].amount.ToString();
                    break;
                }
                default:
                    Debug.LogError("Invalid UI Type: " + uiType);
                    break;
            }
        }
    }
}