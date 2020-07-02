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

        public void Create(string name, GameObject link, CustomLayerType type)
        {
            layerType = type;
            layerNameText.text = name;
            linkedObject = link;
        }

        public void RenameLayer(string newName){
            layerNameText.text = newName;
        }

        public void Remove()
        {
            //TODO: A confirmation before removing might be required. Can be very annoying. Verify with users.
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            GameObject.Destroy(linkedObject);
        }
    }
}