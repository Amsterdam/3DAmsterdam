using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface
{
    public class PlaceCustomObject : MonoBehaviour
    {
        [SerializeField]
        private GameObject customObject;

        private GameObject spawnedObject;

        [SerializeField]
        private LayerType layerType;

        private InterfaceLayers layers;

        private Pointer pointer;

        [SerializeField]
        [Tooltip("Leave empty to place object in root")]
        private Transform targetParent;

		public GameObject SpawnedObject { get => spawnedObject; private set => spawnedObject = value; }

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
        public void SpawnNewObjectAtPointer(string objectName = "")
        {
            spawnedObject = Instantiate(customObject, targetParent);
            spawnedObject.name = (objectName!="") ? objectName : spawnedObject.name.Replace("(Clone)", "");

            CustomLayer interfaceLayer = layers.AddNewCustomObjectLayer(spawnedObject, layerType);

            if (layerType == LayerType.ANNOTATION || layerType == LayerType.CAMERA)
            {
                //Set container layer for objects that have a connection with an interfacelayer
                spawnedObject.GetComponent<PlaceOnClick>().interfaceLayer = interfaceLayer;
            }
            else{
                spawnedObject.transform.position = pointer.WorldPosition;
            }
        }
    }
}