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
        private LayerType layerType;

        private InterfaceLayers layers;

        private Pointer pointer;

        [SerializeField]
        [Tooltip("Leave empty to place object in root")]
        private Transform targetParent;

        private void Start()
        {
            pointer = FindObjectOfType<Pointer>();
            layers = FindObjectOfType<InterfaceLayers>();
        }

        /// <summary>
        /// Move existing GameObject to the pointer and create a linked interface layer
        /// </summary>
        /// <param name="existingGameobject">Reference to existing GameObject</param>
        public void PlaceExistingObjectAtPointer(GameObject existingGameobject) {
            existingGameobject.transform.position = pointer.WorldPosition;
            layers.AddNewCustomObjectLayer(existingGameobject, layerType);
        }
        /// <summary>
        /// Spawn the customObject prefab at the pointer location and create a linked interface layer
        /// </summary>
        public void SpawnNewObjectAtPointer()
        {
            GameObject newObject = Instantiate(customObject, targetParent);
            newObject.name = newObject.name.Replace("(Clone)", "");
            CustomLayer interfaceLayer = layers.AddNewCustomObjectLayer(newObject, layerType);

            if (layerType == LayerType.ANNOTATION)
            {
                var annotation = newObject.GetComponent<Annotation>();
                annotation.interfaceLayer = interfaceLayer;
            }
            else{
                newObject.transform.position = pointer.WorldPosition;
            }
        }
    }
}