using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class CustomLayer : InterfaceLayer
    {               
        [SerializeField]
        private Text layerNameText;

        public string GetName => layerNameText.text;

        [SerializeField]
        private GameObject removeButton;

        public void Create(string name, GameObject link, LayerType type, InterfaceLayers interfaceLayers)
        {
            layerType = type;
            layerNameText.text = name.Replace("(Clone)", ""); //Users do not need to see this is a clone;
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
            GameObject.Destroy(LinkedObject);
        }
    }
}