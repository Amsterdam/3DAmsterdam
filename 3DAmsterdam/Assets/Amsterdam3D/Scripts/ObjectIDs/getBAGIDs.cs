using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace objectIDs
{
    public class getBAGIDs : MonoBehaviour
    {
        public GameObject BuildingContainer;
        public bool isBusy = false;
        private Ray ray;
        public string id = "";
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (id != "")
            {
                Debug.Log(id);
                id = "";
                return;
            }

            if (isBusy)
            {

                return;
            }

            if (Input.GetMouseButtonDown(0) == false)
            {
                return;
            }
 
            id = "";
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            StartCoroutine(LoadMeshColliders());

        }

        IEnumerator LoadMeshColliders()
        {
            // add meshcolliders
            MeshCollider meshCollider;
            MeshFilter[] meshFilters = BuildingContainer.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in meshFilters)
            {
                meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    DestroyImmediate(meshCollider);
                }
                meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;

                yield return null;
            }

            Debug.Log("MeshColliders attached");
            StartCoroutine(getIDData());
        }

        IEnumerator getIDData()
        {

            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit, 10000) == false)
            {
                isBusy = false;
                yield break;

            }
            string name = Hit.collider.gameObject.GetComponent<MeshFilter>().mesh.name;
            Debug.Log(name);
            string dataName = name + "data";
            string dataURL = "file://E:/UnityData/Assetbundles/WebGL/BuildingData/building_118000_485000_data";
            ObjectMappingClass data;
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(dataURL))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {

                }
                else
                {

                    ObjectData objectMapping = Hit.collider.gameObject.GetComponent<ObjectData>();
                    if (objectMapping is null)
                    {
                        objectMapping = Hit.collider.gameObject.AddComponent<ObjectData>();
                    }

                    AssetBundle newAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    data = newAssetBundle.LoadAllAssets<ObjectMappingClass>()[0];
                    Debug.Log(data.ids);
                    int vertexIndex = Hit.triangleIndex * 3;
                    int idIndex = data.vectorMap[vertexIndex];
                    id = data.ids[idIndex];

                    objectMapping.ids = data.ids;
                    objectMapping.uvs = data.uvs;
                    objectMapping.vectorMap = data.vectorMap;
                    objectMapping.mappedUVs = data.mappedUVs;

                    newAssetBundle.Unload(true);

                }
            }

            yield return null;
            isBusy = false;
        }

    }
}