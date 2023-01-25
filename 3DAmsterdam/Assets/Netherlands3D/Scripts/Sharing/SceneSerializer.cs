using Netherlands3D.Cameras;
using Netherlands3D.Core;
using Netherlands3D.Interface;
using Netherlands3D.Interface.Layers;
using Netherlands3D.ObjectInteraction;
using Netherlands3D.TileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Netherlands3D.Sharing
{
    public class SceneSerializer : MonoBehaviour
    {
        private InterfaceLayers interfaceLayers;

        [SerializeField] private RectTransform customLayerContainer;
        [SerializeField] private Annotation annotationPrefab;

        [SerializeField] private Transform annotationsContainer;
        [SerializeField] private Transform cameraPositionsContainer;
        [SerializeField] private GameObject cameraPrefab;

        private SunSettings sunSettings;

        [SerializeField] private InterfaceLayer buildingsLayer;
        [SerializeField] private InterfaceLayer treesLayer;
        [SerializeField] private InterfaceLayer groundLayer;

        private string urlViewIDVariable = "view=";

        private List<GameObject> customMeshObjects;

        public string sharedSceneId = "";

        [Header("Custom object shader source references")]
        [SerializeField] private Material opaqueMaterialSource;
        [SerializeField] private Material transparentMaterialSource;

        [Tooltip("Remove these objects when we are looking at a shared scene with editing allowed")]
        [SerializeField] private GameObject[] objectsRemovedInEditMode;

        [Tooltip("Remove these objects when we are looking at a shared scene with editing not allowed")]
        [SerializeField] private GameObject[] objectsRemovedInViewMode;

        private void Awake()
        {
            interfaceLayers = FindObjectOfType<InterfaceLayers>(true);
            sunSettings = FindObjectOfType<SunSettings>(true);
        }

        private void Start()
		{
            //Optionaly load an existing scene if we supplied a 'view=' id in the url parameters.
			CheckURLForSharedSceneID();

			customMeshObjects = new List<GameObject>();
		}

		private void CheckURLForSharedSceneID()
		{
            //HttpUtility not embedded in Unity's .net integration, so lets just filter out the ID ourselves
            if (Application.absoluteURL.Contains("?" + urlViewIDVariable) || Application.absoluteURL.Contains("&" + urlViewIDVariable))
			{
                var uniqueSceneID = Application.absoluteURL.Split(new string[] { urlViewIDVariable }, StringSplitOptions.None)[1].Split('&')[0].Split('#')[0];
				StartCoroutine(GetSharedScene(uniqueSceneID));
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// This test method allows you to right click this MonoBehaviour in the editor.
		/// And test the downloading of a specific sharedSceneId.
		/// </summary>
		[ContextMenu("Load last saved ID")] 
        public void GetTestId(){
            Debug.Log($"Trying to load scene: {sharedSceneId}");
            if (sharedSceneId != "") StartCoroutine(GetSharedScene(sharedSceneId));
        }
        #endif

        /// <summary>
        /// Download a shared scene JSON file from the objectstore using a unique ID.
        /// </summary>
        /// <param name="sceneId">The unique sharing ID</param>
        /// <returns></returns>
        IEnumerator GetSharedScene(string sceneId)
        {
            yield return new WaitUntil(() => Config.isLoadingOverrides == false);

            var getSceneURL = Config.activeConfiguration.sharingDownloadScenePath.Replace("{sceneId}",sceneId);
            UnityWebRequest getSceneRequest = UnityWebRequest.Get(getSceneURL);
            getSceneRequest.SetRequestHeader("Content-Type", "application/json");
            yield return getSceneRequest.SendWebRequest();
            if (getSceneRequest.result != UnityWebRequest.Result.Success || !getSceneRequest.downloadHandler.text.StartsWith("{"))
            {
                Debug.Log(getSceneRequest.error);
                WarningDialogs.Instance.ShowNewDialog("De gedeelde scene is helaas niet actief of verlopen. Dit gebeurt automatisch na 14 dagen.");
            }
            else
            {
                Debug.Log("Return: " + getSceneRequest.downloadHandler.text);
                ParseSerializableScene(JsonUtility.FromJson<SerializableScene>(getSceneRequest.downloadHandler.text), sceneId);
            }

            yield return null;
        }

        /// <summary>
        /// Recreates a scene based on parsed JSON data (SerializableScene).
        /// </summary>
        /// <param name="scene">The scene data we parsed from JSON</param>
        /// <param name="sceneId">The unique ID of the scene, used for downloading custom objects</param>
        public void ParseSerializableScene(SerializableScene scene, string sceneId)
        {
            HideObjectsInViewMode(scene.allowSceneEdit);

            CameraModeChanger.Instance.ActiveCamera.transform.position = new Vector3(scene.camera.position.x, scene.camera.position.y, scene.camera.position.z);
            CameraModeChanger.Instance.ActiveCamera.transform.rotation = new Quaternion(scene.camera.rotation.x, scene.camera.rotation.y, scene.camera.rotation.z, scene.camera.rotation.w);

            //Apply sunlight settings
            sunSettings.SetDateTimeFromString(scene.sunTimeStamp);

            //Fixed layer settings
            var buildingsSelectSubObjects = FindObjectOfType<SelectSubObjects>();
            if(buildingsSelectSubObjects)
            {
                buildingsSelectSubObjects.HiddenIDs = scene.fixedLayers.buildings.hiddenIds.ToList<string>();
                buildingsSelectSubObjects.TilesWithInteractedSubObjects = scene.fixedLayers.buildings.interactedTiles.ToList<string>();
                buildingsSelectSubObjects.UpdateHiddenListToChildren(true);
            }
            else
            {
                Debug.LogWarning("Cant find buildingsSelectSubObjects");
            }

            buildingsLayer.Active = scene.fixedLayers.buildings.active;
            treesLayer.Active = scene.fixedLayers.trees.active;
            groundLayer.Active = scene.fixedLayers.ground.active;

            buildingsLayer.EnableOptions(scene.allowSceneEdit);
            treesLayer.EnableOptions(scene.allowSceneEdit);
            groundLayer.EnableOptions(scene.allowSceneEdit);

            //Create annotations
            for (int i = 0; i < scene.annotations.Length; i++)
            {
                //Create the 2D annotation
                var annotationData = scene.annotations[i];

                Annotation annotation = Instantiate(annotationPrefab, annotationsContainer);
                annotation.WorldPointerFollower.WorldPosition = new Vector3(annotationData.position.x, annotationData.position.y, annotationData.position.z);
                annotation.BodyText = annotationData.bodyText;
                annotation.AllowEdit = scene.allowSceneEdit;
                annotation.waitingForClick = false;

                //Create a custom annotation layer
                CustomLayer newCustomAnnotationLayer = interfaceLayers.AddNewCustomObjectLayer(annotation.gameObject, LayerType.ANNOTATION, false);
                newCustomAnnotationLayer.RenameLayer(annotationData.bodyText);
                annotation.interfaceLayer = newCustomAnnotationLayer;
                newCustomAnnotationLayer.ViewingOnly(!scene.allowSceneEdit);

                newCustomAnnotationLayer.Active = annotationData.active;
            }

            //Create all custom layers with meshes
            for (int i = 0; i < scene.customLayers.Length; i++)
            {
                SerializableScene.CustomLayer customLayer = scene.customLayers[i];
                GameObject customObject = new GameObject();
                customObject.name = customLayer.layerName;
                ApplyLayerMaterialsToObject(customLayer, customObject);

                CustomLayer newCustomLayer = interfaceLayers.AddNewCustomObjectLayer(customObject, LayerType.OBJMODEL, false);
                newCustomLayer.ViewingOnly(!scene.allowSceneEdit);
                newCustomLayer.EnableOptions(scene.allowSceneEdit);

                newCustomLayer.Active = customLayer.active;
                newCustomLayer.GetUniqueNestedMaterials();
                newCustomLayer.UpdateLayerPrimaryColor();

                StartCoroutine(GetCustomMeshObject(customObject, sceneId, customLayer.token, customLayer.position, customLayer.rotation, customLayer.scale, scene.allowSceneEdit));
            }

            //Create all custom camera points
            for (int i = 0; i < scene.cameraPoints.Length; i++) 
            {
                SerializableScene.CameraPoint cameraPoint = scene.cameraPoints[i];
                var cameraObject = Instantiate(cameraPrefab,cameraPositionsContainer);
                cameraObject.name = cameraPoint.name;
                cameraObject.transform.SetPositionAndRotation(cameraPoint.position, cameraPoint.rotation);
                CustomLayer newCustomLayer = interfaceLayers.AddNewCustomObjectLayer(cameraObject, LayerType.CAMERA);
                newCustomLayer.Active = true;
            }

            //Set material properties for fixed layers
            SetFixedLayerProperties(buildingsLayer, scene.fixedLayers.buildings);
            SetFixedLayerProperties(treesLayer, scene.fixedLayers.trees);
            SetFixedLayerProperties(groundLayer, scene.fixedLayers.ground);
        }

        /// <summary>
        /// Apply the materials found in our downloaded scene/layer and onto our GameObject's MeshRenderer
        /// </summary>
        /// <param name="customLayer"></param>
        /// <param name="customObject"></param>
        private void ApplyLayerMaterialsToObject(SerializableScene.CustomLayer customLayer, GameObject customObject)
        {
            Material[] materials = new Material[customLayer.materials.Length];
            foreach (SerializableScene.Material material in customLayer.materials)
            {
                var newMaterial = (material.a == 1) ? new Material(opaqueMaterialSource) : new Material(transparentMaterialSource);
                newMaterial.SetFloat("_Surface", (material.a == 1) ? 0 : 1); //0 Opaque, 1 Alpha
                newMaterial.SetColor("_BaseColor", new Color(material.r, material.g, material.b, material.a));
                newMaterial.name = material.slotName;
                materials[material.slotId] = newMaterial;
            }
            customObject.AddComponent<MeshRenderer>().materials = materials;
        }

        /// <summary>
        /// Removes a list of objects we do not want to show in view or edit mode.
        /// </summary>
        private void HideObjectsInViewMode(bool editAllowed = false)
        {
            if (editAllowed)
            {
                foreach (GameObject gameObject in objectsRemovedInEditMode)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                foreach (GameObject gameObject in objectsRemovedInViewMode)
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Download a custom mesh object, and put it onto a target object.
        /// </summary>
        /// <param name="gameObjectTarget">The target GameObject where to mesh will be added to</param>
        /// <param name="sceneId">The unique scene ID this mesh is part of</param>
        /// <param name="token">The unqiue token received from the server we can use to download this mesh</param>
        /// <param name="position">The new position for the target GameObject</param>
        /// <param name="rotation">The new rotation for the target GameObject</param>
        /// <param name="scale">The new scale for the target GameObject</param>
        /// <returns></returns>
        private IEnumerator GetCustomMeshObject(GameObject gameObjectTarget, string sceneId, string token, SerializableScene.Vector3 position, SerializableScene.Quaternion rotation, SerializableScene.Vector3 scale, bool transformable = false)
        {
            var getModelURL = Config.activeConfiguration.sharingDownloadModelPath.Replace("{sceneId}",sceneId).Replace("{modelToken}",token);
            UnityWebRequest getModelRequest = UnityWebRequest.Get(getModelURL);
            yield return getModelRequest.SendWebRequest();
            
            if (getModelRequest.result == UnityWebRequest.Result.Success)
            {
                Mesh parsedMesh = BinaryMeshConversion.ReadBinaryMesh(getModelRequest.downloadHandler.data, out int[] materialIndices);
                gameObjectTarget.AddComponent<MeshFilter>().mesh = parsedMesh;
                if (transformable)
                {
                    gameObjectTarget.AddComponent<MeshCollider>().sharedMesh = parsedMesh;
                    gameObjectTarget.AddComponent<Transformable>().stickToMouse = false;
                }

                gameObjectTarget.transform.position = new Vector3(position.x, position.y, position.z);
                gameObjectTarget.transform.rotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
                gameObjectTarget.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
            }
            else
            {
                WarningDialogs.Instance.ShowNewDialog("Een van de modellen kon niet worden geladen.\nDe scene is waarschijnlijk dus niet compleet.");
            }

            yield return null;
        }

        /// <summary>
        /// Apply all the properties from the scene we loaded onto the fixed layer
        /// </summary>
        /// <param name="targetLayer">The target fixed layer</param>
        /// <param name="fixedLayerProperties">The data object containing the loaded properties</param>
        private void SetFixedLayerProperties(InterfaceLayer targetLayer, SerializableScene.FixedLayer fixedLayerProperties)
        {
            //Apply all materials
            for (int i = 0; i < fixedLayerProperties.materials.Length; i++)
            {
                var materialProperties = fixedLayerProperties.materials[i];

                Material materialInSlot = targetLayer.GetMaterialFromSlot(materialProperties.slotId);
                materialInSlot.SetColor("_BaseColor",new Color(materialProperties.r, materialProperties.g, materialProperties.b, materialProperties.a));
            }

            targetLayer.UpdateLayerPrimaryColor();
        }

        /// <summary>
        /// Serialize a custom object its mesh, so we can save it
        /// </summary>
        /// <param name="customMeshIndex">The custom object index in our serialized scene</param>
        /// <param name="sceneId">The unique ID of our scene</param>
        /// <param name="meshToken">The unique token we received from the server for our custom mesh object</param>
        /// <returns></returns>
        public string SerializeCustomObject(int customMeshIndex, string sceneId, string meshToken){
            var targetMesh = customMeshObjects[customMeshIndex].GetComponent<MeshFilter>().mesh;
            var localBinaryMeshFile = Application.persistentDataPath + "/" + meshToken + ".bin";
            BinaryMeshConversion.SaveMeshAsBinaryFile(targetMesh, localBinaryMeshFile);
            return localBinaryMeshFile;
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

        /// <summary>
        /// Serialize the scene into a SerializableScene, so we can turn it into JSON data and share it.
        /// </summary>
        /// <param name="allowSceneEditAfterSharing">Should the user be able to edit the scene when viewing this scene?</param>
        /// <returns></returns>
        public SerializableScene SerializeScene(bool allowSceneEditAfterSharing = false)
        {
            var cameraPosition = CameraModeChanger.Instance.ActiveCamera.transform.position;
            var cameraRotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;

            var buildingsSubObjects = FindObjectOfType<SelectSubObjects>();
            string[] hiddenBuildings = { };
            string[] interactedTiles = { };
            if (buildingsSubObjects)
            {
                hiddenBuildings = buildingsSubObjects.HiddenIDs.ToArray();
                interactedTiles = buildingsSubObjects.TilesWithInteractedSubObjects.ToArray();
                Debug.Log("Hidden ids " + hiddenBuildings.Length);
                Debug.Log("Tiles with hidden ids " + interactedTiles.Length);
            }
            else
            {
                Debug.LogWarning("Cant find buildingsSubObjects");
            }

            var dataStructure = new SerializableScene
            {
                appVersion = Application.version, //Set in SceneSerializer
                timeStamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), //Should be overwritten/determined at serverside when possible
                buildType = Application.version,
                sunTimeStamp = sunSettings.GetDateTimeAsString(), //Will be our virtual world time, linked to the Sun
                allowSceneEdit = allowSceneEditAfterSharing,
                postProcessing = new SerializableScene.PostProcessing { },
                camera = new SerializableScene.Camera
                {
                    position = new SerializableScene.Vector3 { x = cameraPosition.x, y = cameraPosition.y, z = cameraPosition.z },
                    rotation = new SerializableScene.Quaternion { x = cameraRotation.x, y = cameraRotation.y, z = cameraRotation.z, w = cameraRotation.w },
                },
                annotations = GetAnnotations(),
                customLayers = GetCustomMeshLayers(),
                cameraPoints = GetCameras(),

                fixedLayers = new SerializableScene.FixedLayers {
                    buildings = new SerializableScene.FixedLayer {
                        active = buildingsLayer.Active,
                        materials = GetMaterialsAsData(buildingsLayer.UniqueLinkedObjectMaterials),
                        hiddenIds = hiddenBuildings,
                        interactedTiles = interactedTiles
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

        /// <summary>
        /// Gets all the annotations and turn in into serializable data
        /// </summary>
        /// <returns>Array containing serializeable data</returns>
        private SerializableScene.Annotation[] GetAnnotations()
        {
            var customLayer = customLayerContainer.GetComponentsInChildren<CustomLayer>(false);
            var annotationsData = new List<SerializableScene.Annotation>();
            
            foreach (var layer in customLayer)
            {
                var annotation = layer.LinkedObject.GetComponent<Annotation>();
                if (annotation)
                {
                    annotationsData.Add(new SerializableScene.Annotation
                    {
                        active = layer.Active,
                        position = new SerializableScene.Vector3
                        {
                            x = annotation.WorldPointerFollower.WorldPosition.x,
                            y = annotation.WorldPointerFollower.WorldPosition.y,
                            z = annotation.WorldPointerFollower.WorldPosition.z
                        },
                        bodyText = annotation.BodyText
                    });
                }
            }

            return annotationsData.ToArray();
        }


        private SerializableScene.CameraPoint[] GetCameras()
        {
              var customLayerChildren = customLayerContainer.GetComponentsInChildren<CustomLayer>(true);
              var cameraPointsData = new List<SerializableScene.CameraPoint>();
              
              foreach (var child in customLayerChildren)
              {
                    if (child.LayerType != LayerType.CAMERA) continue;

                    var firstPersonObject = child.LinkedObject.GetComponent<SavedCameraPosition>();
                    if (!firstPersonObject) continue;

                    cameraPointsData.Add(new SerializableScene.CameraPoint
                    {
                        position = child.LinkedObject.transform.position,
                        rotation = child.LinkedObject.transform.rotation,
                        name = child.LinkedObject.name
                    });
              }

              return cameraPointsData.ToArray(); 
        }

        /// <summary>
        /// Gets all the custom mesh layers, and turns them into a serializable data object.
        /// </summary>
        /// <returns>Array containing serializeable data</returns>
        private SerializableScene.CustomLayer[] GetCustomMeshLayers()
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

        /// <summary>
        /// Turns a list with materials into serializable data
        /// </summary>
        /// <param name="materialList">A list containing Materials</param>
        /// <returns>And array containing seriazable materials data</returns>
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
                });
            }
            return materialData.ToArray();
        }
    }
}