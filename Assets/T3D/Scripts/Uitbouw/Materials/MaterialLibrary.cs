using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    public class MaterialLibrary : MonoBehaviour
    {
        [SerializeField]
        private static Material[] materials;

        public delegate void MaterialLibraryLoadedEventHandler(Material[] materials);
        public static event MaterialLibraryLoadedEventHandler MaterialLibraryLoaded;

        private void Start()
        {
            var selectMaterials = GetComponentsInChildren<SelectMaterial>();
            materials = new Material[selectMaterials.Length];

            for (int i = 0; i < selectMaterials.Length; i++)
            {
                materials[i] = selectMaterials[i].dragMaterial;
            }

            MaterialLibraryLoaded?.Invoke(materials);
        }

        public static Material GetMaterial(int index)
        {
            return materials[index];
        }

        public static int GetMaterialIndex(Material m)
        {
            return Array.IndexOf(materials, m);
        }
    }
}