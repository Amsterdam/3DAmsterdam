using Amsterdam3D.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Sharing
{
    public class SceneSerializer : MonoBehaviour
    {
        [SerializeField]
        private string appVersion = "0.0.1";

        [SerializeField]
        private RectTransform customLayerContainer;

        [SerializeField]
        private InterfaceLayer buildingsLayer;
        [SerializeField]
        private InterfaceLayer treesLayer;
        [SerializeField]
        private InterfaceLayer groundLayer;

        [SerializeField]
        private string urlViewIDVariable = "?view=";

        [SerializeField]
        private string savedScenePath = "uploads";

        private void Start()
        {
            if(Application.absoluteURL.Contains(urlViewIDVariable)){
                StartCoroutine(GetSharedScene());
            }
        }

        IEnumerator GetSharedScene(){
            
            //Get GET parameter from url (Application.absoluteURL)
            //Split ID, and try to load scene with that ID 
            yield return null;
        }

        public void LoadFromDataStructure(DataStructure dataStructure)
        {
            
        }

        public DataStructure ToDataStructure()
        {
            var cameraPosition = Camera.main.transform.position;
            var cameraRotation = Camera.main.transform.rotation;

            var dataStructure = new DataStructure
            {
                appVersion = Application.version + "-" + appVersion, //Set in SceneSerializer
                timeStamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), //Should be overwritten/determined at serverside when possible
                buildType = Application.version,
                virtualTimeStamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), //Will be our virtual world time, linked to the Sun
                weather = new DataStructure.Weather { },
                postProcessing = new DataStructure.PostProcessing { },
                camera = new DataStructure.Camera
                {
                    position = new DataStructure.Position { x = cameraPosition.x, y = cameraPosition.y, z = cameraPosition.z },
                    rotation = new DataStructure.Rotation { x = cameraRotation.x, y = cameraRotation.y, z = cameraRotation.z, w = cameraRotation.w },
                },
                customLayers = GetCustomLayers(),
                fixedLayers = new DataStructure.FixedLayers {
                    buildings = new DataStructure.FixedLayer {
                        active = buildingsLayer.Active,
                        materials = GetMaterialsAsData(buildingsLayer.UniqueLinkedObjectMaterials)
                    },
                    trees = new DataStructure.FixedLayer
                    {
                        active = treesLayer.Active,
                        materials = GetMaterialsAsData(treesLayer.UniqueLinkedObjectMaterials)
                    },
                    ground = new DataStructure.FixedLayer
                    {
                        active = groundLayer.Active,
                        textureID = 0
                    }
                }
            };
            return dataStructure;
        }
        private DataStructure.CustomLayer[] GetCustomLayers()
        {
            var customLayers = customLayerContainer.GetComponentsInChildren<CustomLayer>(true);
            var customLayersData = new List<DataStructure.CustomLayer>();
            foreach (var layer in customLayers)
            {
                customLayersData.Add(new DataStructure.CustomLayer
                {
                    layerID = layer.gameObject.transform.GetSiblingIndex(),
                    type = (int)layer.LayerType,
                    active = layer.Active,
                    layerName = layer.GetName,
                    modelFilePath = layer.LinkedObject.name,
                    modelFileSize = 0, //Tbtt filesize estimation based on mesh vert count
                    parsedType = "obj", //The parser that was used to parse this model into our platform
                    position = new DataStructure.Position { x = layer.LinkedObject.transform.position.x, y = layer.LinkedObject.transform.position.y, z = layer.LinkedObject.transform.position.z },
                    rotation = new DataStructure.Rotation { x = layer.LinkedObject.transform.rotation.x, y = layer.LinkedObject.transform.rotation.y, z = layer.LinkedObject.transform.rotation.z, w = layer.LinkedObject.transform.rotation.w },
                    materials = GetMaterialsAsData(layer.UniqueLinkedObjectMaterials)
                });
            }
            return customLayersData.ToArray();
        }
        private DataStructure.Material[] GetMaterialsAsData(List<Material> materialList)
        {
            var materialData = new List<DataStructure.Material>();
            foreach(var material in materialList)
            {
                var color = material.GetColor("_BaseColor"); 
                materialData.Add(new DataStructure.Material
                {
                    slotName = material.name,
                    r = color.r,
                    g = color.g,
                    b = color.b,
                    a = color.a
                });
            }
            return materialData.ToArray();
        }
    }
}