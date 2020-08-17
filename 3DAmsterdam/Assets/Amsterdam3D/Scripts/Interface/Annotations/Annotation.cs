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

        private Plane groundPlane;
        private float lastClickTime = 0;
        private float doubleClickTime = 0.2f;

        public CustomLayer interfaceLayer { get; set; }

        private void Start()
        {
            PointLine();
        }

        private void PointLine()
        {
            //If we have colliders in our models, we can draw a line pointing to the exact location
        }

        public void OnDrag(PointerEventData eventData)
        {
            groundPlane = new Plane(Vector3.up, new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0));
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                AlignWithWorldPosition(hitPoint);
            }
            PointLine();
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

        public void StopEditingText()
        {
            balloonText.text = editInputField.text;
            interfaceLayer.RenameLayer(balloonText.text);
            editInputField.gameObject.SetActive(false);
        }
    }
}