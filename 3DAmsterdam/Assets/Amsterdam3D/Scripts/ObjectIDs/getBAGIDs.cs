using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace objectIDs
{
    public class getBAGIDs : MonoBehaviour
    {
        public GameObject BuildingContainer;
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
            if (!DisplayBAGData.Instance.ui.activeSelf)
            {
                if (id != "")
                {
                    Debug.Log(id);

                    CreateTexture();
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
                selectedTile = null;
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                StartCoroutine(LoadMeshColliders());
            }

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
                    Debug.Log(data.ids);
                    int vertexIndex = Hit.triangleIndex * 3;
                    int idIndex = data.vectorMap[vertexIndex];
                    id = data.ids[idIndex];
                    Debug.Log(id);
                    StartCoroutine(ImportBAG.Instance.CallAPI("https://api.data.amsterdam.nl/bag/v1.1/pand/", id, RetrieveType.Pand));

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

        private void CreateTexture()
        {
            ObjectData objectData = selectedTile.GetComponent<ObjectData>();
            Vector2Int textureSize = ObjectIDMapping.GetTextureSize(objectData.ids.Count);
            Texture2D texture = new Texture2D(textureSize.x, textureSize.y);
            Color defaultColor = Color.white;
            defaultColor.a = 0;

            Color highlightColor = Color.red;


            Color idColor;

            Vector2 highlightUV = new Vector2(0.33f, 0.5f);
            Vector2 defaultUV = new Vector2(0.66f, 0.5f);

            Texture2D highlightTexture = new Texture2D(4, 2);
            Vector2Int pixelPosition;
            Vector2[] highlightUVs = new Vector2[objectData.vectorMap.Count];

            highlightTexture.SetPixel(0, 0, highlightColor);
            highlightTexture.SetPixel(1, 0, highlightColor);
            highlightTexture.SetPixel(0, 1, highlightColor);
            highlightTexture.SetPixel(1, 1, highlightColor);

            highlightTexture.SetPixel(2, 0, defaultColor);
            highlightTexture.SetPixel(3, 0, defaultColor);
            highlightTexture.SetPixel(2, 1, defaultColor);
            highlightTexture.SetPixel(3, 1, defaultColor);

            highlightTexture.Apply();

            int objectindex = 0;
            for (int i = 0; i < objectData.ids.Count; i++)
            {
                if (objectData.ids[i] == id)
                {
                    objectindex = i;
                }
            }

            for (int i = 0; i < objectData.vectorMap.Count; i++)
            {
                if (objectData.vectorMap[i] == objectindex)
                {
                    highlightUVs[i] = highlightUV;
                }
                else
                {
                    highlightUVs[i] = defaultUV;
                }
            }

            //for (int i = 0; i < objectData.ids.Count; i++)
            //{
            //    pixelPosition = ObjectIDMapping.GetBottomLeftPixel(textureSize, i);
            //    if (objectData.ids[i]==id)
            //    {
                    
            //        idColor = highlightColor;
            //    }
            //    else
            //    {
                    
            //        idColor = defaultColor;
            //    }
            //    texture.SetPixel(pixelPosition.x, pixelPosition.y,idColor);
            //    texture.SetPixel(pixelPosition.x, pixelPosition.y+1, idColor);
            //    texture.SetPixel(pixelPosition.x+1, pixelPosition.y, idColor);
            //    texture.SetPixel(pixelPosition.x+1, pixelPosition.y+1, idColor);
            //}
            //texture.Apply();
            selectedTile.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap",highlightTexture);
            selectedTile.GetComponent<MeshFilter>().mesh.uv = highlightUVs;
        }
    }
}