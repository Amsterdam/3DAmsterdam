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

        private float lastClickTime = 0;
        private float doubleClickTime = 0.2f;

        public void OnPointerClick(PointerEventData eventData)
        {
            //Catch double click on layer, to move camera to the linked object
            if (Time.time - lastClickTime < doubleClickTime)
            {
                if (layerType == LayerType.ANNOTATION)
                {
                    CameraControls.Instance.MoveAndFocusOnLocation(linkedObject.GetComponent<Annotation>().WorldPosition);
                }
                else{
                    CameraControls.Instance.MoveAndFocusOnLocation(linkedObject.transform.position);
                }
            }
            lastClickTime = Time.time;
        }

        public void Create(string name, GameObject link, LayerType type, InterfaceLayers interfaceLayers)
        {
            //Move me to first place in parent hierarchy
            transform.SetSiblingIndex(0);

            layerType = type;
            layerNameText.text = name.Replace("(Clone)", ""); //Users do not need to see this is a clone;
            LinkObject(link);
            parentInterfaceLayers = interfaceLayers;
        }

        public void RenameLayer(string newName){
            layerNameText.text = newName;
        }

        public void Remove()
        {
            //TODO: A confirmation before removing might be required. Can be very annoying. Verify with users.
            parentInterfaceLayers.LayerVisuals.Close();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            GameObject.Destroy(linkedObject);
        }
    }
}