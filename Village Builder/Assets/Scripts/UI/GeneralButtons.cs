using UnityEngine;

namespace DavidRios.UI
{
    public class GeneralButtons : MonoBehaviour
    {
        //Scripts
        private GameManager _gameManager;
        private UIManager _uiManagerScript;

        private void Awake()
        {
            _gameManager = FindObjectOfType<GameManager>();
            _uiManagerScript = FindObjectOfType<UIManager>();
        }

        public void ResumeButton()
        {
            _gameManager.ResumeGame();
        }

        public void PauseButton()
        {
            _gameManager.PauseGame();
        }

        public void SettingsButton()
        {
            _gameManager.ToggleSettingsMenu();
        }

        public void QuitButton()
        {
            _gameManager.QuitGame();
        }
    }
}