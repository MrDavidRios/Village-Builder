using UnityEngine;
using UnityEngine.UI;

namespace DavidRios.UI
{
    public class ReactiveButtonManager : MonoBehaviour
    {
        //Floats
        public float buttonMoveDistance;
        public float buttonMoveTime;

        private void Start()
        {
            //Subscribe to events from all of the buttons
            foreach (Transform child in transform)
                if (child.GetComponent<Button>() != null)
                    child.GetComponent<ReactiveButton>().ToggleResourcesPanel += MoveButtons;
        }

        public void MoveButtons(object sender, ResourcePanelOpenArgs e)
        {
            var buttonIndex = e.ButtonIndex;

            if (e.Open)
            {
                //Panel open

                //Move buttons that have higher indices down
                foreach (Transform child in transform)
                    if (child.GetComponent<Button>() != null)
                        if (child.GetSiblingIndex() > buttonIndex)
                            StartCoroutine(child.GetComponent<ReactiveButton>().MoveToPosition(-buttonMoveDistance));
            }
            else
            {
                //Panel closed

                //Move buttons that have higher indices back up
                foreach (Transform child in transform)
                    if (child.GetComponent<Button>() != null)
                        if (child.GetSiblingIndex() > buttonIndex)
                            StartCoroutine(child.GetComponent<ReactiveButton>().MoveToPosition(buttonMoveDistance));
            }
        }
    }
}