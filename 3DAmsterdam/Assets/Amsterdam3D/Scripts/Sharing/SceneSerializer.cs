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
                        materials = GetMaterialsData(groundLayer.UniqueLinkedObjectMaterials)
                    },
                    trees = new DataStructure.FixedLayer
                    {
                        active = treesLayer.Active,
                        materials = GetMaterialsData(groundLayer.UniqueLinkedObjectMaterials)
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
            throw new NotImplementedException();
        }

        private DataStructure.Material[] GetMaterialsData(List<Material> materialList)
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