using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class TerrainFlattener : MonoBehaviour
    {
        private Material[] terrainMaterials;
        private BuildingMeshGenerator building;

        private const string groundLevelPropertyKey = "_GroundLevel";

        private void Awake()
        {
            terrainMaterials = GetComponent<MeshRenderer>().materials;
        }

        private void OnEnable()
        {
            building = RestrictionChecker.ActiveBuilding;
            building.BuildingDataProcessed += Building_BuildingDataProcessed;
        }

        private void OnDisable()
        {
            building.BuildingDataProcessed -= Building_BuildingDataProcessed;
        }

        private void Start()
        {
            SetShaderParameters();
        }

        private void Building_BuildingDataProcessed(BuildingMeshGenerator building)
        {
            SetShaderParameters();
        }

        private void SetShaderParameters()
        {
            foreach (var mat in terrainMaterials)
            {
                mat.SetFloat(groundLevelPropertyKey, building.GroundLevel);
            }
        }
    }
}