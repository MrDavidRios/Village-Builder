using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralButtons : MonoBehaviour
{
    //Scripts
    private GameManager gameManager;
    private UIManager UIManagerScript;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        UIManagerScript = FindObjectOfType<UIManager>();
    }

    public void ResumeButton() => gameManager.ResumeGame();

    public void PauseButton() => gameManager.PauseGame();

    public void SettingsButton() => gameManager.ToggleSettingsMenu();

    public void QuitButton() => gameManager.QuitGame();
}
