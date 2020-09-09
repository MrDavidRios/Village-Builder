using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSettings : MonoBehaviour, IPointerExitHandler
{
    private Animator buttonAnimator;

    private void Awake()
    {
        buttonAnimator = GetComponent<Animator>();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonAnimator.GetBool("OpenSettings"))
            buttonAnimator.SetBool("OpenSettings", false);
    }

    public void ToggleOpenSettingsBool()
    {
        buttonAnimator.SetBool("OpenSettings", !buttonAnimator.GetBool("OpenSettings"));
    }
}