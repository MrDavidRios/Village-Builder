using UnityEngine;
using TileOperations;

public class GameManager : MonoBehaviour
{
    //Booleans
    private bool gamePaused;
    public bool cinematicModeEnabled;

    //Integers
    private int _timeSpeedBeforePause;

    //Hotkeys
    [Header("Hotkeys")]
    [LabelOverride("Deselect/Pause Key")]
    [Space(5)]
    public KeyCode deselect_and_pauseKey;
    public KeyCode pauseKey;

    [LabelOverride("Build Menu Toggle Key")]
    [Space(5)]
    public KeyCode buildMenuKey;

    //Scripts
    private PlaceBuilding placeBuilding;
    private SelectTile selectScript;
    private UIManager UIManagerScript;

    //GameObjects
    private GameObject villagerParent;

    private void Awake()
    {
        placeBuilding = GetComponent<PlaceBuilding>();
        selectScript = GetComponent<SelectTile>();
        UIManagerScript = FindObjectOfType<UIManager>();

        villagerParent = GameObject.Find("Villagers");

        #region Initialize Time Speed Controls
        UIManagerScript.ActivateButton(UIManagerScript.miscUIElements["Paused"], false);
        UIManagerScript.ActivateButton(UIManagerScript.miscUIElements["NormalSpeed"], false);
        UIManagerScript.ActivateButton(UIManagerScript.miscUIElements["FastSpeed"], false);
        UIManagerScript.ActivateButton(UIManagerScript.miscUIElements["MaxSpeed"], false);

        switch (Time.timeScale)
        {
            case 0:
                UIManagerScript.ActivateButton(UIManagerScript.miscUIElements["Paused"]);
                break;
            case 1:
                UIManagerScript.ActivateButton(UIManagerScript.miscUIElements["NormalSpeed"]);
                break;
            case 2:
                UIManagerScript.ActivateButton(UIManagerScript.miscUIElements["FastSpeed"]);
                break;
            case 3:
                UIManagerScript.ActivateButton(UIManagerScript.miscUIElements["MaxSpeed"]);
                break;
        }
        #endregion
    }

    private void Update()
    {
        //Toggle cinematicModeEnabled variable on 'LeftAlt' keypress.
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !gamePaused)
        {
            cinematicModeEnabled = cinematicModeEnabled ? cinematicModeEnabled = false : cinematicModeEnabled = true;

            if (cinematicModeEnabled)
            {
                selectScript.DeselectAll();
                SelectTile.canSelect = false;

                UIManagerScript.CloseAllPanels(false);
                UIManagerScript.HideMiscUI();
            }
            else if (!UIManagerScript.anyPanelsOpen)
            {
                UIManagerScript.OpenStaticUI();
                SelectTile.canSelect = true;
            }
        }

        //If the deselect/pause key was pressed, pause the game.
        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(deselect_and_pauseKey))
            PauseGame();

        //If the build menu toggle key was pressed, open/close the build menu.
        if (Input.GetKeyDown(buildMenuKey))
        {
            if (UIManagerScript.mainPanels["BuildingPanel"].activeInHierarchy)
                UIManagerScript.ClosePanel("BuildingPanel");
            else
                UIManagerScript.OpenPanel("BuildingPanel");
        }

        //Time Speed Controls
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ChangeTimeSpeed(1);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            ChangeTimeSpeed(2);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            ChangeTimeSpeed(3);

        UpdateUI();
    }

    public void PauseGame(bool onePress = false)
    {
        //If the pause key was pressed and the game is currently paused, resume the game.
        if (gamePaused)
        {
            ResumeGame();
            return;
        }

        _timeSpeedBeforePause = (int)Time.timeScale;

        if (ElementsLeftToClose() && !onePress)
            return;

        UIManagerScript.CloseAllPanels(false);

        gamePaused = true;

        //Open the pause menu
        UIManagerScript.OpenPanel("PauseMenu");

        ChangeTimeSpeed(0);
    }

    public void ResumeGame()
    {
        gamePaused = false;

        //Close the pause menu
        UIManagerScript.ClosePanel("PauseMenu");

        //Open panels that were closed on pause
        UIManagerScript.OpenStaticUI();

        ChangeTimeSpeed(_timeSpeedBeforePause);
    }

    public void ChangeTimeSpeed(int timeSpeed)
    {
        var timeControlButtons = new GameObject[4];
        timeControlButtons = new[] { UIManagerScript.miscUIElements["Paused"], UIManagerScript.miscUIElements["NormalSpeed"], UIManagerScript.miscUIElements["FastSpeed"], UIManagerScript.miscUIElements["MaxSpeed"] };

        int oldTimeSpeed = (int)Time.timeScale;

        Time.timeScale = timeSpeed;

        UIManagerScript.ActivateButton(timeControlButtons[oldTimeSpeed], false);
        UIManagerScript.ActivateButton(timeControlButtons[timeSpeed]);
    }

    private bool ElementsLeftToClose()
    {
        if (UIManagerScript.UIElementOpen("JobsPanel", "MainPanel"))
        {
            UIManagerScript.ClosePanel("JobsPanel");
            UIManagerScript.OpenPanel("SelectionDescriptionPanel");
            return true;
        }
        else if (selectScript.anythingSelected)
        {
            selectScript.DeselectAll();
            return true;
        }
        else if (placeBuilding.templateBuilding)
        {
            placeBuilding.ClearTemplate();
            return true;
        }
        else if (UIManagerScript.anyPanelsOpen)
        {
            //If any panels are open, close them.
            UIManagerScript.CloseAllPanels(true);
            return true;
        }

        return false;
    }

    private void UpdateUI()
    {
        UIManagerScript.textFields["PopulationCounter"].GetComponent<TMPro.TMP_Text>().text = villagerParent.transform.childCount.ToString();
    }
}
