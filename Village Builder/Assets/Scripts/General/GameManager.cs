using UnityEngine;
using TileOperations;

public class GameManager : MonoBehaviour
{
    //Booleans
    private bool gamePaused;
    private bool cinematicModeEnabled;

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

    private void Awake()
    {
        placeBuilding = GetComponent<PlaceBuilding>();
        selectScript = GetComponent<SelectTile>();
        UIManagerScript = FindObjectOfType<UIManager>();
    }

    private void Update()
    {
        //Toggle cinematicModeEnabled variable on 'C' keypress.
        if (Input.GetKeyDown(KeyCode.C))
            cinematicModeEnabled = cinematicModeEnabled ? cinematicModeEnabled = false : cinematicModeEnabled = true;

        //If the deselect/pause key was pressed, pause the game.
        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(deselect_and_pauseKey))
            PauseGame();

        //If the build menu toggle key was pressed, open/close the build menu.
        if (Input.GetKeyDown(buildMenuKey))
        {
            if (UIManagerScript.mainPanels["BuildingPanel"].activeInHierarchy)
            {
                UIManagerScript.ClosePanel("BuildingPanel");
                UIManagerScript.ShowMiscUI("BuildingPanelOpenButton");
            }
            else
            {
                UIManagerScript.HideMiscUI("BuildingPanelOpenButton");
                UIManagerScript.OpenPanel("BuildingPanel");
            }
        }

        if (cinematicModeEnabled)
            UIManagerScript.CloseAllPanels(false);
        else if (!UIManagerScript.anyPanelsOpen)
            UIManagerScript.OpenPanels(new string[] { "TopBar" });
    }

    public void PauseGame()
    {
        //If the pause key was pressed and the game is currently paused, resume the game.
        if (gamePaused)
        {
            ResumeGame();
            return;
        }

        if (ElementsLeftToClose())
            return;

        UIManagerScript.CloseAllPanels(false);

        gamePaused = true;

        //Open the pause menu
        UIManagerScript.OpenPanel("PauseMenu");

        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        gamePaused = false;

        //Close the pause menu
        UIManagerScript.ClosePanel("PauseMenu");

        //Open panels that were closed on pause
        UIManagerScript.OpenPanels(new string[] { "TopBar" });

        Time.timeScale = 1;
    }

    private bool ElementsLeftToClose()
    {
        if (selectScript.anythingSelected)
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
}
