using Amsterdam3D.CameraMotion;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class Annotation : WorldPointFollower, IDragHandler, IPointerClickHandler
    {
        [SerializeField]
        private Image balloon;

        [SerializeField]
        private Text balloonText;

        [SerializeField]
        private InputField editInputField;

        private float lastClickTime = 0;
        private float doubleClickTime = 0.2f;

        public CustomLayer interfaceLayer { get; set; }

        private bool allowEdit = true;
        public bool AllowEdit {
            set
            {
                allowEdit = value;
                balloon.raycastTarget = allowEdit; //Allows passing rays through ballons for drag/drop
            }
            get
            {
                return allowEdit;
            }
        }

        public string BodyText {
            get{
                return balloonText.text;
            }
            set{
                balloonText.text = value;
            }
        }

        public void PlaceUsingMouse()
        {
            StartCoroutine(StickToMouse());
        }

        /// <summary>
        /// Stick to the mouse pointer untill we click. 
        /// Starts editing after the click.
        /// </summary>
        /// <returns></returns>
        IEnumerator StickToMouse()
        {
            while (!Input.GetMouseButton(0))
            {
                FollowMousePointer();
                yield return new WaitForEndOfFrame();
            }
            StartEditingText();
        }

        /// <summary>
        /// Align the annotation with the mouse pointer position
        /// </summary>
        private void FollowMousePointer()
        {
            AlignWithWorldPosition(CameraControls.Instance.GetMousePositionInWorld());
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!allowEdit) return; 

            FollowMousePointer();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Time.time - lastClickTime < doubleClickTime)
            {
                StartEditingText();
            }
            lastClickTime = Time.time;
        }

        /// <summary>
        /// Start editing the annotation body text
        /// </summary>
        public void StartEditingText()
        {
            if (!allowEdit) return;

            editInputField.gameObject.SetActive(true);
            editInputField.text = BodyText;

            editInputField.Select();
        }

        /// <summary>
        /// Apply the text from the editor directly to the balloon
        /// and the layer name.
        /// </summary>
        public void EditText()
        {
            BodyText = editInputField.text;
            interfaceLayer.RenameLayer(BodyText);
        }

        /// <summary>
        /// Hides the editor, and applies the last text inputs to the balloon and layer name
        /// </summary>
        public void StopEditingText()
        {
            BodyText = editInputField.text;
            interfaceLayer.RenameLayer(BodyText);
            editInputField.gameObject.SetActive(false);
        }
    }
}