using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIActions
{
    Activate,
    Rotate,
    Move
}

public class UIAction : MonoBehaviour
{
    [Header("Action")]
    [LabelOverride("UI Action")]
    public UIActions desiredAction;

    [Header("Settings")]

    public GameObject affectedUI;

    [DrawIf("desiredAction", UIActions.Rotate)]
    public Vector3 desiredRotation;

    [DrawIf("desiredAction", UIActions.Move)]
    public Vector3 desiredPosition;

    [DrawIf("desiredAction", UIActions.Activate)]
    public bool reverseActivation;

    private Vector3 initRotation;
    private Vector3 initPosition;

    private RectTransform rectTransform;
    private Button button;
    private Image image;

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
            case UIActions.Move:
                break;
            case UIActions.Rotate:
                if (affectedUI.activeInHierarchy)
                    rectTransform.eulerAngles = desiredRotation;
                else
                    rectTransform.eulerAngles = initRotation;
                break;
        }
    }

    private void ChangeAlpha(float desiredAlpha)
    {
        var color = image.color;
        color.a = desiredAlpha;
        image.color = color;
    }
}
