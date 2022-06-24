using Netherlands3D.Cameras;
using Netherlands3D.Help;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class Annotation : Interactable, IPointerClickHandler
    {
        [SerializeField]
        private Image balloon;

        [SerializeField]
        private Text balloonText;

        [SerializeField]
        private InputField editInputField;

        [SerializeField]
        private BuildingMeasuring linePrefab;
        private BuildingMeasuring line;

        private float lastClickTime = 0;
        private const float doubleClickTime = 0.2f;

        private Transform originalParent;
        private WorldPointFollower worldPointFollower;

        private bool allowEdit = true;
        public bool AllowEdit
        {
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

        public string BodyText
        {
            get
            {
                return balloonText.text;
            }
            set
            {
                balloonText.text = value;
            }
        }

        //public override void Start()
        //{
        //	//base.Start();

        //          if (waitingForClick)
        //          {
        //	    MoveOnTopOfUI();
        //              //ServiceLocator.GetService<HelpMessage>().Show("<b>Klik</b> op het maaiveld om de annotatie te plaatsen");
        //          }
        //      }

        private void Awake()
        {
            worldPointFollower = GetComponent<WorldPointFollower>();
        }

        private void Start()
        {
            DrawLine();
            StartEditingText();
        }

        private void DrawLine()
        {
            line = Instantiate(linePrefab);
            line.LinePoints[0].transform.position = worldPointFollower.WorldPosition;
            var cam = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera;
            var screenPosition = cam.WorldToScreenPoint(worldPointFollower.WorldPosition);
            var distanceFromCamera = Vector3.Distance(cam.transform.position, worldPointFollower.WorldPosition);
            var offsetScreenPosition = (Vector2)screenPosition + Vector2.right * 50 + Vector2.up * 50;
            var offsetPosition = cam.ScreenToWorldPoint(new Vector3(offsetScreenPosition.x, offsetScreenPosition.y, distanceFromCamera));
            worldPointFollower.AlignWithWorldPosition(offsetPosition);
            line.LinePoints[1].transform.position = worldPointFollower.WorldPosition;
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
            //if (waitingForClick) return;

            if (Time.time - lastClickTime < doubleClickTime)
            {
                StartEditingText();
            }

            lastClickTime = Time.time;
        }

        //      protected override void Placed()
        //{
        //	base.Placed();
        //          //After we placed the annotation, start editing it, so the user can immediatly change its content
        //          StartEditingText();
        //          //ServiceLocator.GetService<PropertiesPanel>().OpenAnnotations();

        //          //Move back into orignal ordered parent
        //          transform.SetParent(originalParent);
        //      }

        /// <summary>
        /// Start editing the annotation body text
        /// </summary>
        public void StartEditingText()
        {
            //if (waitingForClick || !allowEdit) return;

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
            //interfaceLayer.RenameLayer(BodyText);
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