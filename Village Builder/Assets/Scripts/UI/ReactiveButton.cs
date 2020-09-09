using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResourcePanelOpenArgs
{
    public int buttonIndex;
    public bool open;
}

public class ReactiveButton : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    //UI
    private Button buttonComponent;
    private bool duplicateCall;

    //Floats
    private float moveTime;

    //Booleans
    private bool moving;

    //Vector2s
    private Vector2 originalPosition;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    private void Start()
    {
        buttonComponent = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();

        moveTime = transform.parent.GetComponent<ReactiveButtonManager>().buttonMoveTime;

        originalPosition = rectTransform.anchoredPosition;
    }

    private void OnDisable()
    {
        moving = false;
        duplicateCall = false;

        rectTransform.anchoredPosition = originalPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GetComponent<Animator>().SetBool("open", !GetComponent<Animator>().GetBool("open"));

        ToggleResourcesPanel?.Invoke(this,
            new ResourcePanelOpenArgs
                {open = GetComponent<Animator>().GetBool("open"), buttonIndex = transform.GetSiblingIndex()});
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToggleResourcesPanel?.Invoke(this,
            new ResourcePanelOpenArgs {open = false, buttonIndex = transform.GetSiblingIndex()});

        GetComponent<Animator>().SetBool("open", false);

        GetComponent<Animator>().ResetTrigger("Highlighted");
    }

    //Event
    public event EventHandler<ResourcePanelOpenArgs> ToggleResourcesPanel;

    //Smoothly move to position
    public IEnumerator MoveToPosition(float yOffset)
    {
        if (moving)
        {
            duplicateCall = true;
            yield return new WaitUntil(() => !moving);
        }

        moving = true;

        var startPos = rectTransform.anchoredPosition;
        var endPos = Vector2.zero;

        if (yOffset < 0)
            endPos = new Vector2(rectTransform.anchoredPosition.x, originalPosition.y + yOffset);
        else
            endPos = originalPosition;

        var elapsedTime = 0f;

        //Debug.Log(Mathf.Abs(rectTransform.anchoredPosition.y - endPos.y));

        while (Mathf.Abs(rectTransform.anchoredPosition.y - endPos.y) > 0.5f && !duplicateCall)
        {
            elapsedTime += Time.unscaledDeltaTime;

            elapsedTime = Mathf.Clamp(elapsedTime, 0f, moveTime);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / moveTime);

            yield return null;
        }

        moving = false;
        duplicateCall = false;
    }
}