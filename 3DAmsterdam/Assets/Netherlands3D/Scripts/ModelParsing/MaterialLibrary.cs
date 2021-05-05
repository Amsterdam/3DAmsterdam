using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.ModelParsing
{
    public class MaterialLibrary : MonoBehaviour
    {
        [SerializeField]
        private Material[] materialLibrary;

        /// <summary>
        /// Remaps materials to this object based on material name / substrings
        /// </summary>
        /// <param name="renderer">The GameObject containing the renderer with the materials list</param>
        public void AutoRemap(GameObject gameObjectWithRenderer)
        {
            var renderer = gameObjectWithRenderer.GetComponent<MeshRenderer>();
            if(!renderer)
            {
                Debug.LogWarning("No meshrenderer found in this GameObject. Skipping auto remap.");
                return;
			}

            for (int i = 0; i < renderer.materials.Length; i++)
			{
                renderer.sharedMaterials[i] = FindMaterialReplacement(renderer.sharedMaterials[i]);
            }
		}

        /// <summary>
        /// Finds a material from the library with a similar name
        /// </summary>
        /// <param name="comparisonMaterial">The material to find a library material for</param>
        /// <returns></returns>
        public Material FindMaterialReplacement(Material comparisonMaterial)
		{
			foreach(var libraryMaterial in materialLibrary)
            {
                if(comparisonMaterial.name.ToLower().Contains(libraryMaterial.name.ToLower()))
                {
                    return libraryMaterial;
				}
			}

            //Didnt find a replacement? Just return myself.
            return comparisonMaterial;
		}
	}
}