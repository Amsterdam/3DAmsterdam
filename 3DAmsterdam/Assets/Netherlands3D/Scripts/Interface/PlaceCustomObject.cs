using Netherlands3D.Events;
using Netherlands3D.Interface.Layers;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface
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

        [SerializeField] private bool makeTransformable = false;
        [SerializeField] private bool startStuckToMouse = false;
        [SerializeField] private bool moveCameraToRenderer = true;
        [SerializeField] private Vector3 cameraOffsetDirection = new Vector3(0, 1, -1);

        public GameObject SpawnedObject { get => spawnedObject; private set => spawnedObject = value; }

		private void Start()
        {
            pointer = FindObjectOfType<Pointer>();
            layers = FindObjectOfType<InterfaceLayers>();
        }

		public void ObjectPlaced(UnityEngine.Object placedObject)
		{
            GameObject placedObjectForLayer = (GameObject)placedObject;
            pointer.WorldPosition = placedObjectForLayer.transform.position;
            PlaceExistingObjectAtPointer(placedObjectForLayer);

            if (!makeTransformable) return;

            var transformable = placedObjectForLayer.AddComponent<Transformable>();
            transformable.stickToMouse = startStuckToMouse;

            if(!transformable.stickToMouse && moveCameraToRenderer)
            {
                MoveCameraToObjectBounds(placedObjectForLayer, cameraOffsetDirection);
            }
        }

        public static void MoveCameraToObjectBounds(GameObject placedObjectForLayer, Vector3 cameraOffsetDirection)
        {
            var targetPosition = placedObjectForLayer.transform.position + cameraOffsetDirection;
            var targetLookat = placedObjectForLayer.transform.position;
            //If the object has renderers, use the bounds to determine camera focus position and distance
            MeshRenderer[] renderers = placedObjectForLayer.GetComponentsInChildren<MeshRenderer>();
            Debug.Log("Renderers: " + renderers.Length);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                if(renderers.Length > 1)
                {
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        bounds.Encapsulate(renderers[i].bounds);
                    }
                }

                targetLookat = bounds.center;
                targetPosition = targetLookat + (cameraOffsetDirection * bounds.size.magnitude);
            }

            Camera.main.transform.position = targetPosition;
            Camera.main.transform.LookAt(targetLookat);
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
            if (layerType == LayerType.ANNOTATION)
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