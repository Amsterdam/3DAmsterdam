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

        private void Start()
        {
            StartCoroutine(StickToMouse());
        }

        IEnumerator StickToMouse()
        {
            //Keep following mouse untill we clicked, than start to edit the text
            while (!Input.GetMouseButton(0))
            {
                FollowMousePointer();
                yield return new WaitForEndOfFrame();
            }
            StartEditingText();
        }

        private void FollowMousePointer()
        {
            AlignWithWorldPosition(CameraControls.Instance.GetMousePositionInWorld());
        }

        public void OnDrag(PointerEventData eventData)
        {
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

        public void StartEditingText()
        {
            editInputField.gameObject.SetActive(true);
            editInputField.text = balloonText.text;

            editInputField.Select();
        }

        public void EditText()
        {
            balloonText.text = editInputField.text;
            interfaceLayer.RenameLayer(balloonText.text);
        }

        public void StopEditingText()
        {
            balloonText.text = editInputField.text;
            interfaceLayer.RenameLayer(balloonText.text);
            editInputField.gameObject.SetActive(false);
        }
    }
}