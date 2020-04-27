using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Main Panels
    public bool anyPanelsOpen;

    [Header("Main Panels")]
    public GameObject[] mainPanelGameObjects;
    public string[] mainPanelNames;
    public string[] staticUI;

    /*  
     *  Dictionaries are like arrays (but they derive from hashsets, but idk a lot about those), but instead of using indexes, 
     *  you can find the members of an array with a key.
     *  A key is a variable assigned to an array member that sets it apart from every other member. In this case, it's a string that says its name.
     *  Using this, the script knows what gameObject you're looking for in the Dictionary with just passing the string through. 
     *  You can see this in effect on line 54, for example.
     */

    public Dictionary<string, GameObject> mainPanels = new Dictionary<string, GameObject>();

    //Text fields
    [Header("Text Fields")]
    public GameObject[] textFieldGameObjects;
    public string[] textFieldNames;

    public Dictionary<string, GameObject> textFields = new Dictionary<string, GameObject>();

    //Miscellaneous (buttons, dropdowns, etc.)
    [Header("Miscellaneous UI Elements")]
    public GameObject[] miscUIGameObjects;
    public string[] miscUINames;

    public Dictionary<string, GameObject> miscUIElements = new Dictionary<string, GameObject>();

    #region Cached Common-Use Elements
    public static GameObject jobPrefab;
    #endregion

    #region Initialization
    void Awake()
    {
        jobPrefab = Resources.Load("Prefabs/Job/Job") as GameObject;

        //If the 'mainPanelGameObjects' array isn't equal to the 'mainPanelNames' array, return an error.
        if (mainPanelGameObjects.Length == mainPanelNames.Length)
        {
            for (int i = 0; i < mainPanelGameObjects.Length; i++)
            {
                mainPanels.Add(mainPanelNames[i], mainPanelGameObjects[i]);
            }
        }
        else
            Debug.LogError("MainPanelGameObjects length is unequal to MainPanelNames' array length!");

        //If the 'textFieldGameObjects' array isn't equal to the 'textFieldNames' array, return an error.
        if (textFieldGameObjects.Length == textFieldNames.Length)
        {
            for (int i = 0; i < textFieldGameObjects.Length; i++)
            {
                textFields.Add(textFieldNames[i], textFieldGameObjects[i]);
            }
        }
        else
            Debug.LogError("TextFieldGameObjects length is unequal to TextFieldNames' array length!");

        //If the 'miscUIGameObjects' array isn't equal to the 'textFieldNames' array, return an error.
        if (miscUIGameObjects.Length == miscUINames.Length)
        {
            for (int i = 0; i < miscUIGameObjects.Length; i++)
            {
                miscUIElements.Add(miscUINames[i], miscUIGameObjects[i]);
            }
        }
        else
            Debug.LogError("MiscUIGameObjects length is unequal to MiscUINames' array length!");
    }
    #endregion

    #region Panels
    //Open a panel given its name in the mainPanels dictionary.
    public void OpenPanel(string panelName)
    {
        GameObject panel = mainPanels[panelName];

        panel.SetActive(true);
    }

    public void OpenPanels(string[] panelNames)
    {
        for (int i = 0; i < panelNames.Length; i++)
        {
            OpenPanel(panelNames[i]);
        }
    }

    public void OpenStaticUI()
    {
        foreach (KeyValuePair<string, GameObject> entry in mainPanels)
        {
            if (staticUI.Contains(entry.Key))
                OpenPanel(entry.Key);
        }

        foreach (KeyValuePair<string, GameObject> entry in miscUIElements)
        {
            if (staticUI.Contains(entry.Key))
                ShowMiscUI(entry.Key);
        }

        foreach (KeyValuePair<string, GameObject> entry in textFields)
        {
            if (staticUI.Contains(entry.Key))
                ShowText(entry.Key);
        }
    }

    //Close a panel given its name in the mainPanels dictionary.
    public void ClosePanel(string panelName)
    {
        GameObject panel = mainPanels[panelName];

        panel.SetActive(false);
    }

    public void ClosePanels(string[] panelNames)
    {
        for (int i = 0; i < panelNames.Length; i++)
        {
            ClosePanel(panelNames[i]);
        }
    }

    public void CloseAllPanels(bool staticOnly)
    {
        foreach (KeyValuePair<string, GameObject> entry in mainPanels)
        {
            if (staticOnly)
            {
                if (!staticUI.Contains(entry.Key))
                    ClosePanel(entry.Key);
            }
            else
                ClosePanel(entry.Key);
        }
    }
    #endregion

    #region Text
    //Show text given its name in the textFields dictionary.
    public void ShowText(string textName)
    {
        GameObject text = textFields[textName];

        text.SetActive(true);
    }

    //Hide text given its name in the textFields dictionary.
    public void HideText(string textName)
    {
        GameObject text = textFields[textName];

        text.SetActive(false);
    }

    //Change a text object's text value given its name in the textFields dictionary, the new text, and if whether or not its text is displayed using TextMeshPro (look it up).
    public void ChangeText(string textName, string newText, bool TMP = true)
    {
        GameObject text = textFields[textName];

        if (TMP)
        {
            text.GetComponent<TextMeshProUGUI>().text = newText;
        }
        else
        {
            text.GetComponent<TextMesh>().text = newText;
        }
    }
    #endregion

    #region Misc
    //Show miscellaneous UI elements given its name in the miscUIElements dictionary.
    public void ShowMiscUI(string elementName)
    {
        GameObject element = miscUIElements[elementName];

        element.SetActive(true);
    }

    //Hide miscellaneous UI elements given its name in the miscUIElements dictionary.
    public void HideMiscUI(string elementName = "NaN")
    {
        if (elementName == "NaN")
        {
            HideAllMiscUI();
            return;
        }

        GameObject element = miscUIElements[elementName];

        element.SetActive(false);
    }

    private void HideAllMiscUI()
    {
        foreach (KeyValuePair<string, GameObject> entry in miscUIElements)
        {
            HideMiscUI(entry.Key);
        }
    }

    public void ToggleUIObject(GameObject UIObject)
    {
        if (UIObject.activeInHierarchy)
            UIObject.SetActive(false);
        else
            UIObject.SetActive(true);
    }
    #endregion

    private void Update()
    {
        anyPanelsOpen = CheckIfPanelsOpen();
    }

    public void UpdateDropdown(string elementName, int choiceIndex) 
    {
        TMP_Dropdown dropdown = miscUIElements[elementName].GetComponent<TMP_Dropdown>();

        dropdown.value = choiceIndex;
    }

    private bool CheckIfPanelsOpen()
    {
        int openPanels = 0;

        foreach (KeyValuePair<string, GameObject> entry in mainPanels)
        {
            if (entry.Value.activeInHierarchy && !staticUI.Contains(entry.Key))
                openPanels++;
        }

        if (openPanels > 0)
            return true;
        else
            return false;
    }

    public bool UIElementOpen(string elementName, string elementType)
    {
        elementType = elementType.ToLower();

        switch (elementType)
        {
            case "mainpanel":
                return mainPanels[elementName].activeInHierarchy;
            case "textfield":
                return textFields[elementName].activeInHierarchy;
            case "miscui":
                return miscUIElements[elementName].activeInHierarchy;
            default:
                Debug.LogError("Invalid element type provided: " + elementType);
                return false;
        }
    }

    public void FlipImage(GameObject UIElement, bool xAxis)
    {
        var elementRotation = UIElement.GetComponent<RectTransform>().localEulerAngles;

        if (xAxis)
            elementRotation.z *= -1;
        else
            elementRotation.z *= -1;

        UIElement.GetComponent<RectTransform>().localEulerAngles = elementRotation;
    }

    public void FlipImageXAxis(GameObject UIElement) => FlipImage(UIElement, true);
    public void FlipImageYAxis(GameObject UIElement) => FlipImage(UIElement, false);

    public void ActivateButton(GameObject button, bool activateButton = true, bool activateFunction = true) 
    {
        var buttonColor = button.GetComponent<Image>().color;

        if (activateButton)
            buttonColor.a = 1.0f;
        else
            buttonColor.a = 0.2f;

        button.GetComponent<Image>().color = buttonColor;

        button.GetComponent<Button>().interactable = activateFunction;
    }
}