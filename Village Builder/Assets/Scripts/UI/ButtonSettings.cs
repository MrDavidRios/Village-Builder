using UnityEngine;
using UnityEngine.EventSystems;

namespace DavidRios.UI
{
    public class ButtonSettings : MonoBehaviour, IPointerExitHandler
    {
        private Animator _buttonAnimator;
        private static readonly int OpenSettings = Animator.StringToHash("OpenSettings");

        private void Awake()
        {
            _buttonAnimator = GetComponent<Animator>();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_buttonAnimator.GetBool(OpenSettings))
                _buttonAnimator.SetBool(OpenSettings, false);
        }

        public void ToggleOpenSettingsBool()
        {
            _buttonAnimator.SetBool(OpenSettings, !_buttonAnimator.GetBool(OpenSettings));
        }
    }
}