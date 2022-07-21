using Netherlands3D.Cameras;
using Netherlands3D.Help;
using Netherlands3D.Interface.SidePanel;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface
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

        private Transform originalParent;

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

		public override void Start()
		{
			base.Start();

            if (waitingForClick)
            {
			    MoveOnTopOfUI();
                HelpMessage.Show("<b>Klik</b> op het maaiveld om de annotatie te plaatsen");
            }
        }

        /// <summary>
        /// We move this object on top of the UI so we can start dragging it from UI panels
        /// </summary>
		private void MoveOnTopOfUI()
		{
			originalParent = transform.parent;
			transform.SetParent(originalParent.parent);
			transform.SetAsLastSibling();
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

            //Move back into orignal ordered parent
            transform.SetParent(originalParent);
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