using Netherlands3D.Cameras;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Layers
{
    public class CustomLayer : InterfaceLayer, IPointerClickHandler
    {               
        [SerializeField]
        private Text layerNameText;
        public string GetName => layerNameText.text;

        [SerializeField]
        private GameObject removeButton;

        private int maxNameLength = 24;

        private void Start()
        {
            //Make sure we always add from the top (minus add button, and annotation group)
            this.transform.SetSiblingIndex(2);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Catch double click on layer, to move camera to the linked object
            if (layerType == LayerType.ANNOTATION)
            {
                Annotation annotation = LinkedObject.GetComponent<Annotation>();
                //ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.MoveAndFocusOnLocation(annotation.WorldPointerFollower.WorldPosition, new Quaternion());
                annotation.StartEditingText();
            }

            else if (layerType == LayerType.CAMERA) 
            {
                WorldPointFollower follower = LinkedObject.GetComponent<WorldPointFollower>();
                FirstPersonLocation obj = LinkedObject.GetComponent<FirstPersonLocation>();
                ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.MoveAndFocusOnLocation(follower.WorldPosition, obj.savedRotation);
            }
            else
            {
                //If this is a Transformable, select it
                var transformable = LinkedObject.GetComponent<Transformable>();
                if (transformable)
                {
                    transformable.Select();
                    ToggleLayerOpened();
                }
            }
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
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            GameObject.Destroy(LinkedObject);
        }
    }
}