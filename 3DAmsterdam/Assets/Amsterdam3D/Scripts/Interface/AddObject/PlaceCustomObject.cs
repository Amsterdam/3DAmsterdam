using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface
{
    public class PlaceCustomObject : MonoBehaviour
    {
        [SerializeField]
        private GameObject customObject;

        [SerializeField]
        private CustomLayerType layerType;

        private InterfaceLayers layers;

        private Pointer pointer;

        private void Start()
        {
            pointer = FindObjectOfType<Pointer>();
            layers = FindObjectOfType<InterfaceLayers>();
        }

        /// <summary>
        /// Move existing GameObject to the pointer and create a linked interface layer
        /// </summary>
        /// <param name="existingGameobject">Reference to existing GameObject</param>
        public void AtPointer(GameObject existingGameobject) {
            existingGameobject.transform.position = pointer.WorldPosition;
            layers.AddNewCustomObjectLayer(existingGameobject, layerType);
        }
        /// <summary>
        /// Spawn the customObject prefab at the pointer location and create a linked interface layer
        /// </summary>
        public void AtPointer()
        {
            GameObject newObject = Instantiate(customObject, pointer.WorldPosition, Quaternion.identity);
            layers.AddNewCustomObjectLayer(newObject, layerType);
        }
    }
}