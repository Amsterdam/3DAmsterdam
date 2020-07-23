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
        private InterfaceLayer terrainLayer;

        public DataStructure ToDataStructure()
        {
            var cameraPosition = Camera.main.transform.position;
            var cameraRotation = Camera.main.transform.rotation;

            var dataStructure = new DataStructure
            {
                appVersion = Application.version + "-" + appVersion, //Set in SceneSerializer
                timeStamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), //Overwrite at serverside when possible (clients time cant be trusted)
                buildType = Application.version,
                virtualTimeStamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), //Will be our virtual world time, linked to the Sun
                camera = new DataStructure.Camera
                {
                    position = new DataStructure.Position { x = cameraPosition.x, y = cameraPosition.y, z = cameraPosition.z },
                    rotation = new DataStructure.Rotation { x = cameraRotation.x, y = cameraRotation.y, z = cameraRotation.z, w = cameraRotation.w },
                },
                weather = new DataStructure.Weather { },
                postProcessing = new DataStructure.PostProcessing { }
            };

            dataStructure.camera = new DataStructure.Camera
            {
                position = new DataStructure.Position { x=cameraPosition.x, y=cameraPosition.y, z=cameraPosition.z },
                rotation = new DataStructure.Rotation { x = cameraRotation.x, y = cameraRotation.y, z = cameraRotation.z, w = cameraRotation.w },
            };
            return dataStructure;
        }
    }
}