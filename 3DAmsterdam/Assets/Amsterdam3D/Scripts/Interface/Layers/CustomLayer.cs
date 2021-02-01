using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class CustomLayer : InterfaceLayer, IPointerClickHandler
    {               
        [SerializeField]
        private Text layerNameText;
        public string GetName => layerNameText.text;

        [SerializeField]
        private GameObject removeButton;

        private float lastClickTime = 0;
        private float doubleClickTime = 0.2f;

        private int maxNameLength = 24;

        private void Start()
        {
            //Make sure we always add from the top (minus add button, and annotation group)
            this.transform.SetSiblingIndex(2);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Catch double click on layer, to move camera to the linked object
            if (Time.time - lastClickTime < doubleClickTime)
            {
                if (layerType == LayerType.ANNOTATION)
                {
                    Annotation annotation = LinkedObject.GetComponent<Annotation>();
                    CameraModeChanger.Instance.CurrentCameraControls.MoveAndFocusOnLocation(annotation.WorldPointerFollower.WorldPosition, new Quaternion());
                    annotation.StartEditingText();
                }

                else if (layerType == LayerType.CAMERA) 
                {
                    WorldPointFollower follower = LinkedObject.GetComponent<WorldPointFollower>();
                    FirstPersonLocation obj = LinkedObject.GetComponent<FirstPersonLocation>();
                    CameraModeChanger.Instance.CurrentCameraControls.MoveAndFocusOnLocation(follower.WorldPosition, obj.savedRotation);
                }
                else
                {
                    CameraModeChanger.Instance.CurrentCameraControls.MoveAndFocusOnLocation(LinkedObject.transform.position, new Quaternion());
                }
            }
            lastClickTime = Time.time;
        }

        public void Create(string layerName, GameObject link, LayerType type, InterfaceLayers interfaceLayers)
        {
            layerType = type;
            layerNameText.text = layerName;
            LinkObject(link);
            parentInterfaceLayers = interfaceLayers;
        }

        public void RenameLayer(string newName){
            name = newName; //use our object name to store our full name

            if (newName.Length > maxNameLength)
                newName = newName.Substring(0, maxNameLength - 3) + "...";

            layerNameText.text = newName;
        }
        /// <summary>
        /// Enable or disable layer options based on view mode
        /// </summary>
        /// <param name="viewOnly">Only view mode enabled</param>
        public void ViewingOnly(bool viewOnly)
        {
            removeButton.SetActive(!viewOnly);
        }

        public void Remove()
        {
            //TODO: A confirmation before removing might be required. Can be very annoying. Verify with users.
            parentInterfaceLayers.LayerVisuals.Close();
            ObjectProperties.Instance.ClosePanel();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            GameObject.Destroy(LinkedObject);
        }
    }
}