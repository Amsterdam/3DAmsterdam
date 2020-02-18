using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CityGML;
using UnityEngine.Networking;
using System;


namespace CityGML
{
    public class CreateGameObjects: MonoBehaviour
    {
        public GameObject CreateBuilding(BuildingFeature building, Vector3Double offset, GameObject parent, Material Defaultmaterial, bool LoadTextures)
        {
            List<Surface> surfaces = building.Surfaces;
            GameObject go = new GameObject(building.id);
            go.transform.parent = parent.transform;
            Attributes att = go.AddComponent<Attributes>();
            foreach (AttributeData item in building.Attributes)
            {
                att.items.Add(item);
            }

            att.id = building.id;
            foreach (Surface surf in surfaces)
            {
                
                GameObject subobject = new GameObject(surf.SurfaceTheme);
                subobject.transform.parent = go.transform;
                Attributes subatt = subobject.AddComponent<Attributes>();
                foreach (AttributeData item in surf.Attributes)
                {
                    subatt.items.Add(item);
                }
                subatt.id = surf.id;
                Mesh ms = CreateMesh(surf,offset);
                subobject.AddComponent<MeshFilter>().mesh = ms;
                subobject.AddComponent<MeshRenderer>().sharedMaterial = Defaultmaterial;
                AppearanceMember appmember = building.Appearancemembers;
                
                if (surf.parameterizedTexture != null && LoadTextures)
                {
                    StartCoroutine(CreateMaterial(surf.parameterizedTexture, Defaultmaterial, subobject));
                }

                MeshCollider mc = subobject.AddComponent<MeshCollider>();
            }
            return go;
        }

        #region create meshes
        /// <summary>
        /// create a unitymesh from a 3D-polygon, the polygon can have interior and exterior rings
        /// ring-input is given in world-coordinates (z is up)
        /// </summary>
        /// <param name="surf">Surface-class with interior and exterior rings</param>
        /// <param name="origin"></param>
        /// <returns></returns>
        private Mesh CreateMesh(Surface surf, Vector3Double origin)
        {
            List<Vector2> totalUVs = new List<Vector2>();
            Mesh ms = new Mesh();
            List<CombineInstance> submeshes = new List<CombineInstance>();
            CombineInstance ci;
            if (surf.ExteriorRings.Count > 1) // multiple External rings, treat as seperate meshes
            {
                foreach (LinearRing ring in surf.ExteriorRings)
                {
                    Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
                    List<Vector3> subvectors = CreateVectorlist(ring.Vertices, origin); // unitystyle coordinates
                    poly.outside = subvectors;
                    if (surf.ExteriorRings[0].uvs != null)
                    {
                        List<Vector2> uvs = createUVList(surf.ExteriorRings[0].uvs);
                        poly.outsideUVs = uvs;
                    }
                    Mesh submesh = Poly2Mesh.CreateMesh(poly);
                    ci = new CombineInstance();
                    ci.mesh = submesh;
                    submeshes.Add(ci);
                }
            }
            else //single exteriorring with possibly multiple exteriorRings
            {
                Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
                List<Vector3> subvectors = CreateVectorlist(surf.ExteriorRings[0].Vertices, origin); // unitystyle coordinates outerring
                poly.outside = subvectors;
                // add uv-coordinates
                List<Vector2> outsideUVs = new List<Vector2>();
                if (surf.ExteriorRings[0].uvs !=null)
                {
                    outsideUVs = createUVList(surf.ExteriorRings[0].uvs);
                    
                }

                List<List<Vector2>> HoleUVList = new List<List<Vector2>>();
                foreach (LinearRing ring in surf.InteriorRings)
                {
                    List<Vector3> subvectorsint = CreateVectorlist(ring.Vertices, origin); // unitystyle coordinates
                    poly.holes.Add(subvectorsint);
                    List<Vector2> holeuvs = new List<Vector2>();
                    if (surf.ExteriorRings[0].uvs != null)
                    {
                        holeuvs = createUVList(surf.ExteriorRings[0].uvs);
                        
                    }
                    if (holeuvs.Count == subvectorsint.Count)
                    {
                        HoleUVList.Add(holeuvs);
                    }
                }
                if (HoleUVList.Count == poly.holes.Count && outsideUVs.Count == poly.outside.Count)
                {
                    poly.holesUVs = HoleUVList;
                    poly.outsideUVs = outsideUVs;

                }
                Mesh submesh = Poly2Mesh.CreateMesh(poly);
                if (submesh != null)
                {
                    ci = new CombineInstance();
                    ci.mesh = submesh;
                    submeshes.Add(ci);
                }
            }

            
            ms.CombineMeshes(submeshes.ToArray(), true, false);
            ms.RecalculateBounds();
            ms.RecalculateNormals();
            return ms;
        }
        private List<Vector3> CreateVectorlist(List<Vector3Double> vectors, Vector3Double origin)
        {
            List<Vector3> output = new List<Vector3>();
            Vector3 vect;
            for (int i = 0; i < vectors.Count-1; i++)
            {
                vect = new Vector3();
                vect.x = (float)(vectors[i].x-origin.x);
                vect.y = (float)(vectors[i].z - origin.z);
                vect.z = (float)(vectors[i].y - origin.y);
                output.Add(vect);
            }

            output.Reverse();
            return output;
        }

        private List<Vector2> createUVList(List<Vector2> input)
        {
            
            if (input is null)
            {
                return null;
            }
            input.Reverse();
            if (input[0] == input[input.Count-1])
            {
                input.RemoveAt(0);
            }
            
            return input;

        }
        #endregion

        IEnumerator CreateMaterial(ParameterizedTexture texturedata, Material defaultmaterial, GameObject TargetObject)
        {
            Material mat = new Material(defaultmaterial);
            mat.color = new Color(1, 1, 1);
            
            Texture2D texture;
            string texturepath = "file://"+texturedata.imageURI;
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(texturepath))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    // Get downloaded asset bundle
                    texture = DownloadHandlerTexture.GetContent(uwr);
                    texture.wrapMode = TextureWrapMode.Repeat;
                    mat.SetTexture("_MainTex", texture);
                    TargetObject.GetComponent<MeshRenderer>().material = mat;
                }
            }

        }
    }
}