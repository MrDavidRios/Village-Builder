using UnityEngine;
using UnityEngine.UI;

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

    private Button button;
    private Image image;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        button = GetComponent<Button>();
        image = GetComponent<Image>();

        initRotation = rectTransform.eulerAngles;
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
                    button.interactable = false;

                    //Make the button transparent
                    ChangeAlpha(0.0f);
                }
                else
                {
                    //Make the button interactable
                    button.interactable = true;

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
                    rectTransform.eulerAngles = desiredRotation;
                else
                    rectTransform.eulerAngles = initRotation;
                break;
            }
            case UIActions.ChangeColor:
            {
                if (affectedUI.activeInHierarchy)
                    image.color = endingColor;
                else
                    image.color = startingColor;
                break;
            }
        }
    }

    private void ChangeAlpha(float desiredAlpha)
    {
        var color = image.color;
        color.a = desiredAlpha;
        image.color = color;
    }

    #region Rotate

    [DrawIf("desiredAction", UIActions.Rotate)]
    public Vector3 desiredRotation;

    private Vector3 initRotation;

    #endregion

    #region Move

    [DrawIf("desiredAction", UIActions.Move)]
    public Vector3 desiredPosition;

    private Vector3 initPosition;

    #endregion

    #region Change Color

    [DrawIf("desiredAction", UIActions.ChangeColor)]
    public Color startingColor;

    [DrawIf("desiredAction", UIActions.ChangeColor)]
    public Color endingColor;

    #endregion
}