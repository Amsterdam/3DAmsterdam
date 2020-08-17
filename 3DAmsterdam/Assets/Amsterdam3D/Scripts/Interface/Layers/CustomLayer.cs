﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
    public class CustomLayer : InterfaceLayer
    {               
        [SerializeField]
        private Text layerNameText;

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