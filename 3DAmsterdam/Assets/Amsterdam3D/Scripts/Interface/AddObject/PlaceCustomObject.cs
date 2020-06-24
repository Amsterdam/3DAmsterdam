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

        private void Start()
        {
            layers = FindObjectOfType<InterfaceLayers>();
        }

        public void AtPointer() {
            var pointerPosition = FindObjectOfType<Pointer>().WorldPosition;

            GameObject newObject = Instantiate(customObject, pointerPosition, Quaternion.identity);
            layers.AddNewCustomObject(newObject, layerType);
        }
    }
}