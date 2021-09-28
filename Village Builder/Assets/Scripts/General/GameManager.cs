using DavidRios.Building;
using DavidRios.Input;
using DavidRios.UI;
using TileOperations;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Integers
    private int _timeSpeedBeforePause;

    //Booleans
    public bool cinematicModeEnabled;
    
    private bool _gamePaused;

    //GameObjects
    private GameObject _villagerParent;
    
    //Scripts
    private Select _selectScript;
    private UIManager _uiManagerScript;
    
    //Input
    private PlayerController.DefaultActions _input;

    private void Awake()
    {
        _selectScript = GetComponent<Select>();
        _uiManagerScript = FindObjectOfType<UIManager>();

        _villagerParent = GameObject.Find("Villagers");

        #region Initialize Time Speed Controls

        _uiManagerScript.ActivateButton(_uiManagerScript.MiscUIElements["Paused"], false);
        _uiManagerScript.ActivateButton(_uiManagerScript.MiscUIElements["NormalSpeed"], false);
        _uiManagerScript.ActivateButton(_uiManagerScript.MiscUIElements["FastSpeed"], false);
        _uiManagerScript.ActivateButton(_uiManagerScript.MiscUIElements["MaxSpeed"], false);

        switch (Time.timeScale)
        {
            case 0:
                _uiManagerScript.ActivateButton(_uiManagerScript.MiscUIElements["Paused"]);
                break;
            case 1:
                _uiManagerScript.ActivateButton(_uiManagerScript.MiscUIElements["NormalSpeed"]);
                break;
            case 2:
                _uiManagerScript.ActivateButton(_uiManagerScript.MiscUIElements["FastSpeed"]);
                break;
            case 3:
                _uiManagerScript.ActivateButton(_uiManagerScript.MiscUIElements["MaxSpeed"]);
                break;
        }

        #endregion
    }
    
    private void Start() => _input = InputHandler.playerControllerInstance.Default;

    private void Update()
    {
        //Toggle cinematicModeEnabled variable on 'LeftAlt' keypress.
        ToggleCinematicMode();

        //If the deselect/pause key was pressed, pause the game.
        if (InputHandler.Pressed(_input.DeselectPause))
            PauseGame();

        //Time Speed Controls
        TimeSpeedControls();

        //Update the UI
        UpdateUI();
    }

    private void ToggleCinematicMode()
    {
        if (InputHandler.Pressed(_input.CinematicMode) && !_gamePaused)
        {
            cinematicModeEnabled = cinematicModeEnabled ? cinematicModeEnabled = false : cinematicModeEnabled = true;

            if (cinematicModeEnabled)
            {
                _selectScript.DeselectAll();
                Select.CanSelect = false;

                _uiManagerScript.CloseAllPanels(false);
                _uiManagerScript.HideMiscUI();
            }
            else if (!_uiManagerScript.anyPanelsOpen)
            {
                _uiManagerScript.OpenStaticUI();
                Select.CanSelect = true;
            }
        }
    }

    private void TimeSpeedControls()
    {
        if (InputHandler.Pressed(_input.StopTime))
            ChangeTimeSpeed(0);

        if (InputHandler.Pressed(_input.TimeSpeed1))
            ChangeTimeSpeed(1);

        if (InputHandler.Pressed(_input.TimeSpeed2))
            ChangeTimeSpeed(2);

        if (InputHandler.Pressed(_input.TimeSpeed3))
            ChangeTimeSpeed(3);
    }

    public void PauseGame(bool onePress = false)
    {
        //If the pause key was pressed and the game is currently paused, resume the game.
        if (_gamePaused)
        {
            ResumeGame();
            return;
        }

        _timeSpeedBeforePause = (int) Time.timeScale;

        if (ElementsLeftToClose() && !onePress)
            return;

        _uiManagerScript.CloseAllPanels(false);

        _gamePaused = true;

        //Open the pause menu
        _uiManagerScript.OpenPanel("PauseMenu");

        ChangeTimeSpeed(0);
    }

    public void ResumeGame()
    {
        _gamePaused = false;

        //Close the pause menu
        _uiManagerScript.ClosePanel("PauseMenu");

        //Open panels that were closed on pause
        _uiManagerScript.OpenStaticUI();

        ChangeTimeSpeed(_timeSpeedBeforePause);
    }

    public void ToggleSettingsMenu()
    {
    }

    public void QuitGame() => Application.Quit();

    public void ChangeTimeSpeed(int timeSpeed)
    {
        var timeControlButtons = new GameObject[4];
        timeControlButtons = new[]
        {
            _uiManagerScript.MiscUIElements["Paused"], _uiManagerScript.MiscUIElements["NormalSpeed"],
            _uiManagerScript.MiscUIElements["FastSpeed"], _uiManagerScript.MiscUIElements["MaxSpeed"]
        };

        var oldTimeSpeed = (int) Time.timeScale;

        Time.timeScale = timeSpeed;

        _uiManagerScript.ActivateButton(timeControlButtons[oldTimeSpeed], false);
        _uiManagerScript.ActivateButton(timeControlButtons[timeSpeed]);
    }

    private bool ElementsLeftToClose()
    {
        if (_uiManagerScript.UIElementOpen("JobsPanel", "MainPanel"))
        {
            _uiManagerScript.ClosePanel("JobsPanel");
            _uiManagerScript.OpenPanel("SelectionDescriptionPanel");
            return true;
        }

        if (_selectScript.anythingSelected)
        {
            _selectScript.DeselectAll();
            return true;
        }

        if (PositionBuildingTemplate.TemplateBuilding)
        {
            TemplateActions.ClearTemplate();
            return true;
        }

        if (_uiManagerScript.anyPanelsOpen)
        {
            //If any panels are open, close them.
            _uiManagerScript.CloseAllPanels(true);
            return true;
        }

        return false;
    }

    private void UpdateUI()
    {
        _uiManagerScript.TextFields["PopulationCounter"].GetComponent<TMP_Text>().text =
            _villagerParent.transform.childCount.ToString();
    }
}