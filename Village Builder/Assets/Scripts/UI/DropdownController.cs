﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DavidRios.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    [DisallowMultipleComponent]
    public class DropdownController : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Indexes that should be ignored. Indexes are 0 based.")]
        public List<int> indexesToDisable = new List<int>();

        private Dropdown _dropdown;

        private void Awake()
        {
            _dropdown = GetComponent<Dropdown>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var dropDownList = GetComponentInChildren<Canvas>();
            if (!dropDownList) return;

            // If the dropdown was opened find the options toggles
            var toggles = dropDownList.GetComponentsInChildren<Toggle>(true);
            var textElements = dropDownList.GetComponentsInChildren<TMP_Text>(true);

            // the first item will always be a template item from the dropdown we have to ignore
            // so we start at one and all options indexes have to be 1 based
            for (var i = 1; i < toggles.Length; i++)
            {
                // disable buttons if their 0-based index is in indexesToDisable
                // the first item will always be a template item from the dropdown
                // so in order to still have 0 based indexes for the options here we use i-1
                toggles[i].interactable = !indexesToDisable.Contains(i - 1);

                if (indexesToDisable.Contains(i - 1))
                    textElements[i].fontStyle = FontStyles.Italic;
            }
        }

        // Anytime change a value by index
        public void EnableOption(int index, bool enable)
        {
            if (index < 1 || index > _dropdown.options.Count)
            {
                Debug.LogWarning("Index out of range -> ignored!", this);
                return;
            }

            if (enable)
            {
                // remove index from disabled list
                if (indexesToDisable.Contains(index)) indexesToDisable.Remove(index);
            }
            else
            {
                // add index to disabled list
                if (!indexesToDisable.Contains(index)) indexesToDisable.Add(index);
            }

            var dropDownList = GetComponentInChildren<Canvas>();

            // If this returns null than the Dropdown was closed
            if (!dropDownList) return;

            // If the dropdown was opened find the options toggles
            var toogles = dropDownList.GetComponentsInChildren<Toggle>(true);
            toogles[index].interactable = enable;
        }

        // Anytime change a value by string label
        public void EnableOption(string label, bool enable)
        {
            var index = _dropdown.options.FindIndex(o => string.Equals(o.text, label));

            // We need a 1-based index
            EnableOption(index + 1, enable);
        }
    }
}