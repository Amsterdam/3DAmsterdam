using Netherlands3D.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Netherlands3D.Interface.Layers;

namespace Netherlands3D.Interface.SidePanel
{
    public class ActionButton : MonoBehaviour
    {
        private Action<string> clickAction;

        [SerializeField]
        private TextMeshProUGUI buttonText;

        [SerializeField]
        private Button button;

        private void Start()
        {
            gameObject.AddComponent<AnalyticsClickTrigger>();
        }

        public void Select()
        {
            if (clickAction != null) clickAction.Invoke("");
        }

        public void ToggleButtonInteraction(bool interactable)
        {
            button.interactable = interactable;
        }

        public void SetAction(string title, Action<string> action)
        {
            gameObject.name = title;
            buttonText.text = title;
            clickAction = action;
        }


        //Re-design
        [SerializeField]
        protected LayerType layerType = LayerType.STATIC;
        public LayerType LayerType { get => layerType; set => layerType = value; }

        public TextMeshProUGUI ButtonText { get => buttonText; set => buttonText = value; }

        [SerializeField]
        private GameObject linkedObject;
        public GameObject LinkedObject { get => linkedObject; set => linkedObject = value; }

        public void OnViewpointClick()
        {
            switch(layerType) 
            {
                case LayerType.CAMERA:
                    Camera.main.transform.SetPositionAndRotation(LinkedObject.transform.position, LinkedObject.transform.rotation);
                    break;
                case LayerType.ANNOTATION:
                    Camera.main.transform.SetPositionAndRotation(LinkedObject.GetComponent<WorldPointFollower>().WorldPosition, LinkedObject.transform.rotation);
                    break;
                default:
                    break;
            }
        }
    }
}