using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DavidRios.UI
{
    public class ResourcePanelOpenArgs
    {
        public int ButtonIndex;
        public bool Open;
    }

    public class ReactiveButton : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
    {
        //UI
        private Button _buttonComponent;
        private bool _duplicateCall;

        //Floats
        private float _moveTime;

        //Booleans
        private bool _moving;

        //Vector2s
        private Vector2 _originalPosition;
        private RectTransform _rectTransform;

        // Start is called before the first frame update
        private void Start()
        {
            _buttonComponent = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();

            _moveTime = transform.parent.GetComponent<ReactiveButtonManager>().buttonMoveTime;

            _originalPosition = _rectTransform.anchoredPosition;
        }

        private void OnDisable()
        {
            _moving = false;
            _duplicateCall = false;

            _rectTransform.anchoredPosition = _originalPosition;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            GetComponent<Animator>().SetBool("open", !GetComponent<Animator>().GetBool("open"));

            ToggleResourcesPanel?.Invoke(this,
                new ResourcePanelOpenArgs
                    {Open = GetComponent<Animator>().GetBool("open"), ButtonIndex = transform.GetSiblingIndex()});
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToggleResourcesPanel?.Invoke(this,
                new ResourcePanelOpenArgs {Open = false, ButtonIndex = transform.GetSiblingIndex()});

            GetComponent<Animator>().SetBool("open", false);

            GetComponent<Animator>().ResetTrigger("Highlighted");
        }

        //Event
        public event EventHandler<ResourcePanelOpenArgs> ToggleResourcesPanel;

        //Smoothly move to position
        public IEnumerator MoveToPosition(float yOffset)
        {
            if (_moving)
            {
                _duplicateCall = true;
                yield return new WaitUntil(() => !_moving);
            }

            _moving = true;

            var startPos = _rectTransform.anchoredPosition;
            var endPos = Vector2.zero;

            if (yOffset < 0)
                endPos = new Vector2(_rectTransform.anchoredPosition.x, _originalPosition.y + yOffset);
            else
                endPos = _originalPosition;

            var elapsedTime = 0f;

            //Debug.Log(Mathf.Abs(rectTransform.anchoredPosition.y - endPos.y));

            while (Mathf.Abs(_rectTransform.anchoredPosition.y - endPos.y) > 0.5f && !_duplicateCall)
            {
                elapsedTime += Time.unscaledDeltaTime;

                elapsedTime = Mathf.Clamp(elapsedTime, 0f, _moveTime);

                _rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsedTime / _moveTime);

                yield return null;
            }

            _moving = false;
            _duplicateCall = false;
        }
    }
}