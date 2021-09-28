using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DavidRios.UI
{
    public class ReturnSelectedUI : MonoBehaviour
    {
        //Normal raycasts do not work on UI elements, they require a special kind
        private GraphicRaycaster _raycaster;

        private void Awake()
        {
            //Get both of the components we need to do this
            _raycaster = GetComponent<GraphicRaycaster>();
        }

        public GameObject SelectedUI()
        {
            //Set up the new Pointer Event
            var pointerData = new PointerEventData(EventSystem.current);
            var results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            pointerData.position = UnityEngine.Input.mousePosition;
            _raycaster.Raycast(pointerData, results);

            if (results.Count == 0)
                return null;

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (var result in results) return result.gameObject;

            return null;
        }
    }
}