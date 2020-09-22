using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LayerSystem;

    public class getBAGIDs : MonoBehaviour
    {
    public TileHandler tileHandler;
        public GameObject BuildingContainer;
        public Material HighlightMaterial;
        public Material defaultMaterial;
        public bool isBusy = false;
        private Ray ray;
        public string id = "";
        public GameObject selectedTile;
            
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

            BuildingContainer.GetComponent<LayerSystem.Layer>().Highlight(id);
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
        isBusy = true;
        tileHandler.pauseLoading = true;
        id = "";
            selectedTile = null;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            StartCoroutine(LoadMeshColliders());

        }

        IEnumerator LoadMeshColliders()
        {
            // add meshcolliders
            MeshCollider meshCollider;
            MeshFilter[] meshFilters = BuildingContainer.GetComponentsInChildren<MeshFilter>();
        if (meshFilters == null)
        {
            tileHandler.pauseLoading = false;
            yield break;
        }
            foreach (MeshFilter meshFilter in meshFilters)
            {
            if (meshFilter == null)
            {
                tileHandler.pauseLoading = false;
                yield break;
            }
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
            tileHandler.pauseLoading = false;
            yield break;

            }
            selectedTile = Hit.collider.gameObject;
            string name = Hit.collider.gameObject.GetComponent<MeshFilter>().mesh.name;
            Debug.Log(name);
            string dataName = name.Replace(" Instance","");
            dataName = dataName.Replace("mesh", "building");
            dataName = dataName.Replace("-", "_") +"-data";
            string dataURL = "https://acc.3d.amsterdam.nl/web/data/feature-Link-BAGid/buildings/objectdata/" +dataName;
            Debug.Log(dataURL);
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
                    int vertexIndex = Hit.triangleIndex * 3;
                    int idIndex = data.vectorMap[vertexIndex];
                    id = data.ids[idIndex];
                    objectMapping.highlightIDs.Clear();
                    objectMapping.highlightIDs.Add(id);
                    objectMapping.ids = data.ids;
                    objectMapping.uvs = data.uvs;
                    objectMapping.vectorMap = data.vectorMap;
                    objectMapping.mappedUVs = data.mappedUVs;

                    newAssetBundle.Unload(true);

                }
            }

            yield return null;
        tileHandler.pauseLoading = false;
        isBusy = false;
        }

        //private void CreateTexture()
        //{
        //    ObjectData objectData = selectedTile.GetComponent<ObjectData>();
        //    Vector2[] highlightUVs = objectData.GetUVs();
        //    selectedTile.GetComponent<MeshRenderer>().sharedMaterial = HighlightMaterial;
        //    selectedTile.GetComponent<MeshFilter>().mesh.uv2 = highlightUVs;
        //}
    }
