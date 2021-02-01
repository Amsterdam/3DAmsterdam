using Amsterdam3D.CameraMotion;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class Annotation : PlaceOnClick, IPointerClickHandler
    {
        [SerializeField]
        private Image balloon;

        [SerializeField]
        private Text balloonText;

        [SerializeField]
        private InputField editInputField;

        private float lastClickTime = 0;
        private const float doubleClickTime = 0.2f;

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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (waitingForClick) return;

            if (Time.time - lastClickTime < doubleClickTime)
            {
                StartEditingText();
            }
            lastClickTime = Time.time;
        }

		protected override void Placed()
		{
			base.Placed();
            //After we placed the annotation, start editing it, so the user can immediatly change its content
            StartEditingText();
        }

		/// <summary>
		/// Start editing the annotation body text
		/// </summary>
		public void StartEditingText()
        {
            if (waitingForClick || !allowEdit) return;

            editInputField.gameObject.SetActive(true);
            editInputField.text = BodyText;

            editInputField.Select();
        }

        /// <summary>
        /// Apply the text from the editor directly to the balloon
        /// and the layer name.
        /// </summary>
        public void ApplyText()
        {
            BodyText = editInputField.text;
            interfaceLayer.RenameLayer(BodyText);
        }

        private void Update()
		{
			AutoHideByCamera();
		}

        /// <summary>
        /// Hides the annotation based on camera type and distance
        /// </summary>
		private void AutoHideByCamera()
		{
			if (CameraModeChanger.Instance.CameraMode == CameraMode.StreetView)
			{
				var distance = WorldPointerFollower.WorldPosition - CameraModeChanger.Instance.ActiveCamera.transform.position;
				var viewportPosition = CameraModeChanger.Instance.ActiveCamera.WorldToViewportPoint(WorldPointerFollower.WorldPosition);
				//Alternate way, connect annotations to world tiles?
				// World space canvas instead of using canvas space?
				if (viewportPosition.x > 1 || viewportPosition.x < -1 || viewportPosition.y > 1 || viewportPosition.y < -1 || viewportPosition.z < 0)
				{
					balloon.gameObject.SetActive(false);
					balloonText.gameObject.SetActive(false);
				}
				else if (distance.x > 100 || distance.z > 100 || distance.x < -100 || distance.z < -100)
				{
					balloon.gameObject.SetActive(false);
					balloonText.gameObject.SetActive(false);
				}
				else
				{
					balloon.gameObject.SetActive(true);
					balloonText.gameObject.SetActive(true);
				}
			}
			else
			{
				balloon.gameObject.SetActive(true);
				balloonText.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Hides the editor, and applies the last text inputs to the balloon and layer name
		/// </summary>
		public void StopEditingText()
        {
            ApplyText();
            editInputField.gameObject.SetActive(false);
        }
    }
}