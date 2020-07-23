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
                        materials = GetMaterialsAsData(groundLayer.UniqueLinkedObjectMaterials)
                    },
                    trees = new DataStructure.FixedLayer
                    {
                        active = treesLayer.Active,
                        materials = GetMaterialsAsData(groundLayer.UniqueLinkedObjectMaterials)
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
            var simplifiedCustomLayers = new List<DataStructure.CustomLayer>();
            foreach (var layer in customLayers)
            {
                simplifiedCustomLayers.Add(new DataStructure.CustomLayer
                {
                    layerID = layer.gameObject.transform.GetSiblingIndex(),
                    type = (int)layer.LayerType,
                    active = layer.Active,
                    layerName = layer.GetName,
                    modelFilePath = layer.LinkedObject.name,
                    modelFileSize = 0, //Estimation
                    parsedType = "obj",
                    position = new DataStructure.Position { x = layer.LinkedObject.transform.position.x, y = layer.LinkedObject.transform.position.y, z = layer.LinkedObject.transform.position.z },
                    rotation = new DataStructure.Rotation { x = layer.LinkedObject.transform.rotation.x, y = layer.LinkedObject.transform.rotation.y, z = layer.LinkedObject.transform.rotation.z, w = layer.LinkedObject.transform.rotation.w },
                    materials = GetMaterialsAsData(layer.UniqueLinkedObjectMaterials)
                    /*
                    "layerID": 0,
                    "type": 0,
                    "active": true,
                    "layerName": "My custom model",
                    "modelFilePath": "o4kdm5asuonk2o22oa/building",
                    "modelFileSize": 120000,
                    "parsedType": "obj",
                    "position": {
                    "x": 1560.76648,
                    "y": 305.494873,
                    "z": -3748.26978
                    },
                    "rotation": {
                    "x": -0.226790473,
                    "y": 0.04884689,
                    "z": -0.0113894995,
                    "w": -0.9726512
                    },
                    "materials": [
                     */
                });
            }
            return simplifiedCustomLayers.ToArray();
        }
        private DataStructure.Material[] GetMaterialsAsData(List<Material> materialList)
        {
            var simplifiedMaterials = new List<DataStructure.Material>();
            foreach(var material in materialList)
            {
                var color = material.GetColor("_BaseColor"); 
                simplifiedMaterials.Add(new DataStructure.Material
                {
                    r = color.r,
                    g = color.g,
                    b = color.b
                });
            }
            return simplifiedMaterials.ToArray();
        }
    }
}