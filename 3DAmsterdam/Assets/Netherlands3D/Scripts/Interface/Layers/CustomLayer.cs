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
        private GameObject removeButton;

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
            RenameLayer(layerName);
            LinkObject(link);
            parentInterfaceLayers = interfaceLayers;
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
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            GameObject.Destroy(LinkedObject);
        }
    }
}