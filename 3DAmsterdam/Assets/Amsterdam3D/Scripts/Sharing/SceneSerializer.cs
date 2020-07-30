using Amsterdam3D.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityScript.Steps;

namespace Amsterdam3D.Sharing
{
    public class SceneSerializer : MonoBehaviour
    {
        [SerializeField]
        private string appVersion = "0.0.1";

        [SerializeField]
        private InterfaceLayers interfaceLayers;

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

        private List<GameObject> customMeshObjects;

        public string sharedSceneId = "";

        [SerializeField]
        private Material sourceMaterialForParsedMeshes;

        [SerializeField]
        private GameObject[] removeObjectsWhenViewing;

        private void Start()
        {
            if (Application.absoluteURL.Contains(urlViewIDVariable)){
                StartCoroutine(GetSharedScene(Application.absoluteURL.Split('=')[1]));
            }
            customMeshObjects = new List<GameObject>();
        }

        [ContextMenu("Load last saved ID")]
        public void GetTestId(){
            if (sharedSceneId != "") StartCoroutine(GetSharedScene(sharedSceneId));
        }

        IEnumerator GetSharedScene(string sceneId)
        {
            UnityWebRequest getSceneRequest = UnityWebRequest.Get(Constants.SHARE_URL + "share/" + sceneId + "/scene.json");
            getSceneRequest.SetRequestHeader("Content-Type", "application/json");
            yield return getSceneRequest.SendWebRequest();
            if (getSceneRequest.isNetworkError)
            {
                Debug.Log("Error: " + getSceneRequest.error);
            }
            else
            {
                ParseSerializableScene(JsonUtility.FromJson<SerializableScene>(getSceneRequest.downloadHandler.text), sceneId);
            }

            yield return null;
        }

        public void ParseSerializableScene(SerializableScene scene, string sceneId)
        {
            RemoveObjectsForViewing();

            Camera.main.transform.position = new Vector3(scene.camera.position.x, scene.camera.position.y, scene.camera.position.z);
            Camera.main.transform.rotation = new Quaternion(scene.camera.rotation.x, scene.camera.rotation.y, scene.camera.rotation.z, scene.camera.rotation.w);

            var cameraRotation = Camera.main.transform.rotation;

            //Fixed layer settings
            buildingsLayer.Active = scene.fixedLayers.buildings.active;
            treesLayer.Active = scene.fixedLayers.trees.active;
            groundLayer.Active = scene.fixedLayers.ground.active;

            //Create all custom layers
            for (int i = 0; i < scene.customLayers.Length; i++)
            {
                SerializableScene.CustomLayer customLayer = scene.customLayers[i];
                GameObject customObject = new GameObject();
                customObject.name = customLayer.layerName;
                ApplyLayerMaterialsToObject(customLayer, customObject);

                CustomLayer newCustomLayer = interfaceLayers.AddNewCustomObjectLayer(customObject, LayerType.OBJMODEL);
                newCustomLayer.ViewingOnly(true);
                newCustomLayer.Active = customLayer.active;
                newCustomLayer.FetchUniqueNestedMaterials();
                newCustomLayer.UpdateLayerPrimaryColor();

                StartCoroutine(GetCustomMeshObject(customObject, sceneId, customLayer.token, customLayer.position, customLayer.rotation, customLayer.scale));
            }

            //Set material properties for fixed layers
            SetFixedLayerProperties(buildingsLayer, scene.fixedLayers.buildings);
            SetFixedLayerProperties(treesLayer, scene.fixedLayers.trees);
            SetFixedLayerProperties(groundLayer, scene.fixedLayers.ground);
        }

        private void ApplyLayerMaterialsToObject(SerializableScene.CustomLayer customLayer, GameObject customObject)
        {
            Material[] materials = new Material[customLayer.materials.Length];
            foreach (SerializableScene.Material material in customLayer.materials)
            {
                var newMaterial = new Material(sourceMaterialForParsedMeshes);
                newMaterial.SetColor("_BaseColor", new Color(material.r, material.g, material.b, material.a));
                newMaterial.name = material.slotName;
                materials[material.slotId] = newMaterial;
            }
            customObject.AddComponent<MeshRenderer>().materials = materials;
        }

        private void RemoveObjectsForViewing()
        {
            foreach(GameObject gameObject in removeObjectsWhenViewing)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator GetCustomMeshObject(GameObject gameObjectTarget, string sceneId, string token, SerializableScene.Vector3 position, SerializableScene.Quaternion rotation, SerializableScene.Vector3 scale)
        {
            
            Debug.Log(Constants.SHARE_URL + "share/" + token + ".dat");
            UnityWebRequest getModelRequest = UnityWebRequest.Get(Constants.SHARE_URL + "share/" + sceneId + "/" + token + ".dat");
            getModelRequest.SetRequestHeader("Content-Type", "application/json");
            yield return getModelRequest.SendWebRequest();
            if (getModelRequest.isNetworkError)
            {
                Debug.Log("Error: " + getModelRequest.error);
            }
            else
            {
                Mesh parsedMesh = ParseSerializableMesh(JsonUtility.FromJson<SerializableMesh>(getModelRequest.downloadHandler.text));
                gameObjectTarget.AddComponent<MeshFilter>().mesh = parsedMesh;
                gameObjectTarget.transform.position = new Vector3(position.x, position.y, position.z);
                gameObjectTarget.transform.rotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
                gameObjectTarget.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
            }

            yield return null;
            //interfaceLayers.AddNewCustomObjectLayer(, LayerType.OBJMODEL);
        }

        private Mesh ParseSerializableMesh(SerializableMesh serializableMesh){
            Mesh parsedMesh = new Mesh();
            parsedMesh.indexFormat = (serializableMesh.meshBitType == 0) ? IndexFormat.UInt16 : IndexFormat.UInt32;
            var subMeshCount = serializableMesh.subMeshes.Length;
            parsedMesh.subMeshCount = subMeshCount;
            parsedMesh.SetVertices(MeshSerializer.SeperateVector3Array(serializableMesh.verts));
            parsedMesh.SetUVs(0, MeshSerializer.SeperateVector2Array(serializableMesh.uvs));
            parsedMesh.SetNormals(MeshSerializer.SeperateVector3Array(serializableMesh.normals));
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMesh = serializableMesh.subMeshes[i];
                parsedMesh.SetTriangles(subMesh.triangles,i);
            }
            return parsedMesh;
        }

        private void SetFixedLayerProperties(InterfaceLayer targetLayer, SerializableScene.FixedLayer fixedLayerProperties)
        {
            for (int i = 0; i < fixedLayerProperties.materials.Length; i++)
            {
                var materialProperties = fixedLayerProperties.materials[i];
                targetLayer.SetMaterialProperties(materialProperties.slotId, new Color(materialProperties.r, materialProperties.g, materialProperties.b, materialProperties.a));
            }
            targetLayer.UpdateLayerPrimaryColor();
        }

        public SerializableMesh SerializeCustomObject(int customMeshIndex, string sceneId, string meshToken){
            var targetMesh = customMeshObjects[customMeshIndex].GetComponent<MeshFilter>().mesh;
            
            var newSerializableMesh = new SerializableMesh
            {
                sceneId = sceneId,
                meshToken = meshToken,
                version = appVersion,
                meshBitType = (targetMesh.indexFormat == IndexFormat.UInt32) ? 1 : 0,
                verts = MeshSerializer.FlattenVector3Array(targetMesh.vertices),
                //uvs = MeshSerializer.FlattenVector2Array(targetMesh.uv), //No texture support yet. So we dont need these yet.
                normals = MeshSerializer.FlattenVector3Array(targetMesh.normals),
                subMeshes = SerializeSubMeshes(targetMesh)
            };
            return newSerializableMesh;
        }

        public SerializableSubMesh[] SerializeSubMeshes(Mesh mesh){
            var subMeshes = new SerializableSubMesh[mesh.subMeshCount];
            for (int i = 0; i < subMeshes.Length; i++)
            {
                subMeshes[i] = new SerializableSubMesh
                {
                    triangles = mesh.GetTriangles(i)
                };
            }
            return subMeshes;
        }

        public SerializableScene SerializeScene(bool allowSceneEditAfterSharing = false)
        {
            var cameraPosition = Camera.main.transform.position;
            var cameraRotation = Camera.main.transform.rotation;

            var dataStructure = new SerializableScene
            {
                appVersion = Application.version + "-" + appVersion, //Set in SceneSerializer
                timeStamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), //Should be overwritten/determined at serverside when possible
                buildType = Application.version,
                virtualTimeStamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), //Will be our virtual world time, linked to the Sun
                allowSceneEdit = allowSceneEditAfterSharing,
                weather = new SerializableScene.Weather { },
                postProcessing = new SerializableScene.PostProcessing { },
                camera = new SerializableScene.Camera
                {
                    position = new SerializableScene.Vector3 { x = cameraPosition.x, y = cameraPosition.y, z = cameraPosition.z },
                    rotation = new SerializableScene.Quaternion { x = cameraRotation.x, y = cameraRotation.y, z = cameraRotation.z, w = cameraRotation.w },
                },
                customLayers = GetCustomLayers(),
                fixedLayers = new SerializableScene.FixedLayers {
                    buildings = new SerializableScene.FixedLayer {
                        active = buildingsLayer.Active,
                        materials = GetMaterialsAsData(buildingsLayer.UniqueLinkedObjectMaterials)
                    },
                    trees = new SerializableScene.FixedLayer
                    {
                        active = treesLayer.Active,
                        materials = GetMaterialsAsData(treesLayer.UniqueLinkedObjectMaterials)
                    },
                    ground = new SerializableScene.FixedLayer
                    {
                        active = groundLayer.Active,
                        textureID = 0
                    }
                }
            };
            return dataStructure;
        }
        private SerializableScene.CustomLayer[] GetCustomLayers()
        {
            var customLayers = customLayerContainer.GetComponentsInChildren<CustomLayer>(true);
            var customLayersData = new List<SerializableScene.CustomLayer>();
            customMeshObjects.Clear();

            var customModelId = 0;
            foreach (var layer in customLayers)
            {
                switch (layer.LayerType){
                    case LayerType.OBJMODEL:
                    case LayerType.BASICSHAPE:
                        customMeshObjects.Add(layer.LinkedObject);
                        customLayersData.Add(new SerializableScene.CustomLayer
                        {
                            layerID = layer.gameObject.transform.GetSiblingIndex(),
                            type = (int)layer.LayerType,
                            active = layer.Active,
                            layerName = layer.GetName,
                            modelFilePath = customModelId.ToString(),
                            modelFileSize = 0, //Tbtt filesize estimation based on mesh vert count
                            parsedType = "obj", //The parser that was used to parse this model into our platform
                            position = new SerializableScene.Vector3 { x = layer.LinkedObject.transform.position.x, y = layer.LinkedObject.transform.position.y, z = layer.LinkedObject.transform.position.z },
                            rotation = new SerializableScene.Quaternion { x = layer.LinkedObject.transform.rotation.x, y = layer.LinkedObject.transform.rotation.y, z = layer.LinkedObject.transform.rotation.z, w = layer.LinkedObject.transform.rotation.w },
                            scale = new SerializableScene.Vector3 { x=layer.LinkedObject.transform.localScale.x, y = layer.LinkedObject.transform.localScale.y , z = layer.LinkedObject.transform.localScale.z },
                            materials = GetMaterialsAsData(layer.UniqueLinkedObjectMaterials)
                        });
                        break;
                }
                customModelId++;
            }
            return customLayersData.ToArray();
        }
        private SerializableScene.Material[] GetMaterialsAsData(List<Material> materialList)
        {
            var materialData = new List<SerializableScene.Material>();
            for (int i = 0; i < materialList.Count; i++)
            {
                var material = materialList[i];
                var color = material.GetColor("_BaseColor");
                materialData.Add(new SerializableScene.Material
                {
                    slotId = i,
                    slotName = material.name,
                    r = color.r,
                    g = color.g,
                    b = color.b,
                    a = color.a
                }); ;
            }
            return materialData.ToArray();
        }
    }
}