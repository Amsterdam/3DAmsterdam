using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.AssetGeneration.CityJSON
{
    public class LoadUVData : MonoBehaviour
    {
        public ObjectMappingClass objectMapping;
        void Start()
        {
            GetComponent<MeshFilter>().mesh.uv = objectMapping.uvs;
        }
    }
}