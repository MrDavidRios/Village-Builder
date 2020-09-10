using UnityEngine;
using UnityEngine.UI;

namespace DavidRios.UI
{
    public enum UIActions
    {
        Activate,
        Rotate,
        Move,
        ChangeColor
    }

    public class UIAction : MonoBehaviour
    {
        [Header("Action")] [LabelOverride("UI Action")]
        public UIActions desiredAction;

        [Header("Settings")] public GameObject affectedUI;

        #region Activate

        [DrawIf("desiredAction", UIActions.Activate)]
        public bool reverseActivation;

        #endregion

        private Button _button;
        private Image _image;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            _button = GetComponent<Button>();
            _image = GetComponent<Image>();

            _initRotation = _rectTransform.eulerAngles;
        }

        private void Update()
        {
            switch (desiredAction)
            {
                case UIActions.Activate:
                {
                    if (affectedUI.activeInHierarchy && reverseActivation)
                    {
                        //Make the button unable to be interacted with
                        _button.interactable = false;

                        //Make the button transparent
                        ChangeAlpha(0.0f);
                    }
                    else
                    {
                        //Make the button interactable
                        _button.interactable = true;

                        //Make the button visible
                        ChangeAlpha(1.0f);
                    }

                    break;
                }
                case UIActions.Move:
                {
                    break;
                }
                case UIActions.Rotate:
                {
                    if (affectedUI.activeInHierarchy)
                        _rectTransform.eulerAngles = desiredRotation;
                    else
                        _rectTransform.eulerAngles = _initRotation;
                    break;
                }
                case UIActions.ChangeColor:
                {
                    if (affectedUI.activeInHierarchy)
                        _image.color = endingColor;
                    else
                        _image.color = startingColor;
                    break;
                }
            }
        }

        private void ChangeAlpha(float desiredAlpha)
        {
            var color = _image.color;
            color.a = desiredAlpha;
            _image.color = color;
        }

        #region Rotate

        [DrawIf("desiredAction", UIActions.Rotate)]
        public Vector3 desiredRotation;

        private Vector3 _initRotation;

        #endregion

        #region Move

        [DrawIf("desiredAction", UIActions.Move)]
        public Vector3 desiredPosition;

        private Vector3 _initPosition;

        #endregion

        #region Change Color

        [DrawIf("desiredAction", UIActions.ChangeColor)]
        public Color startingColor;

        [DrawIf("desiredAction", UIActions.ChangeColor)]
        public Color endingColor;

        #endregion
    }
}