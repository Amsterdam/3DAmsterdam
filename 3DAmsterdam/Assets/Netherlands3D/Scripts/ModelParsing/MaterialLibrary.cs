﻿using Netherlands3D.Underground;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.ObjectInteraction;

namespace Netherlands3D.ModelParsing
{
    public class MaterialLibrary : MonoBehaviour
    {
        [SerializeField]
        private Material[] materialLibrary;

        [SerializeField]
        private float materialColorMatchingThreshold = 0.01f;

        public static MaterialLibrary Instance;

		private void Awake()
		{
            Instance = this;
        }

		/// <summary>
		/// Remaps materials to this object based on material name / substrings
		/// </summary>
		/// <param name="renderer">The GameObject containing the renderer with the materials list</param>
		public bool AutoRemap(GameObject gameObjectWithRenderer)
		{
			var renderer = gameObjectWithRenderer.GetComponent<MeshRenderer>();
			if (!renderer)
			{
				Debug.LogWarning("No meshrenderer found in this GameObject. Skipping auto remap.");
				return false;
			}

            var matchedMaterialNames = FoundMatch(renderer);
            if (matchedMaterialNames.Count > 0)
            {
                RequestConfirmationInSidePanel(renderer, matchedMaterialNames.ToArray());
                return true;
            }
            return false;
		}

        private void RequestConfirmationInSidePanel(MeshRenderer renderer, string[] matchedMaterialNames)
        {
            PropertiesPanel.Instance.OpenObjectInformation("", true,10);
            PropertiesPanel.Instance.AddTitle("Materialen gevonden");
            PropertiesPanel.Instance.AddTextfield("Er zijn <b>" + matchedMaterialNames.Length + " materialen*</b> gevonden die overeenkomen met die uit de bibliotheek. Wil je deze overnemen?");
            PropertiesPanel.Instance.AddActionButtonBig("Ja, neem over", (action) =>
            {
                ApplyMaterialOverrides(renderer);
                PropertiesPanel.Instance.OpenCustomObjects();
                UpdateBounds();
            });
            PropertiesPanel.Instance.AddActionButtonBig("Nee", (action) =>
            {
                PropertiesPanel.Instance.OpenCustomObjects(renderer.GetComponent<Transformable>());
                UpdateBounds();
            });

            PropertiesPanel.Instance.AddLabel("<i>*Het gaat om de volgende materialen:</i>");
            foreach(var materialName in matchedMaterialNames)
            {
                PropertiesPanel.Instance.AddTextfield($"<i>- {materialName}</i>");
            }
            
        }
        /// <summary>
		/// Method allowing the triggers for when this object bounds were changed so the thumbnail will be rerendered.
		/// </summary>
		public void UpdateBounds()
        {
            int objectOriginalLayer = this.gameObject.layer;
            this.gameObject.layer = PropertiesPanel.Instance.ThumbnailExclusiveLayer;

            //Render transformable using the bounds of all the nested renderers (allowing for complexer models with subrenderers)
            Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            PropertiesPanel.Instance.RenderThumbnailContaining(bounds);
            this.gameObject.layer = objectOriginalLayer;
        }
        private List<string> FoundMatch(MeshRenderer renderer)
        {
            var materialArray = renderer.materials;
            List<string> materialNames = new List<string>();
            for (int i = 0; i < materialArray.Length; i++)
            {
                if (materialArray[i] != FindMaterialReplacement(materialArray[i]))
                    materialNames.Add(materialArray[i].name.Replace("(Clone)", "").Replace("(Instance)", ""));
            }
            return materialNames;
        }

		private void ApplyMaterialOverrides(MeshRenderer renderer)
		{
			var materialArray = renderer.materials;
			for (int i = 0; i < materialArray.Length; i++)
			{
				var replacement = FindMaterialReplacement(materialArray[i], true);
				replacement.name = replacement.name.Replace("(Clone)", "");
				ClearMask(replacement);
				materialArray[i] = replacement;
			}
			renderer.materials = materialArray;
		}

		private void ClearMask(Material targetMaterialWithMask)
        {
            //Our materials plucked from library might have some masking set. Clear those
            targetMaterialWithMask.SetTexture(RuntimeMask.clippingMaskTexture, null);
        }

        /// <summary>
        /// Finds a material from the library with a similar name
        /// </summary>
        /// <param name="comparisonMaterial">The material to find a library material for</param>
        /// <returns></returns>
        public Material FindMaterialReplacement(Material comparisonMaterial, bool returnAsInstance = false)
		{
			foreach(var libraryMaterial in materialLibrary)
            {
                if(comparisonMaterial.name.ToLower().Contains(libraryMaterial.name.ToLower()))
                {
                    Debug.Log("Found library material with matching name: " + libraryMaterial.name);
                    if (returnAsInstance) return Instantiate(libraryMaterial);
                    return libraryMaterial;
				}
                else if (ColorsAreSimilar(comparisonMaterial.GetColor("_BaseColor"),libraryMaterial.GetColor("_BaseColor"), materialColorMatchingThreshold))
                {
                    Debug.Log("Found library material with matching color: " + libraryMaterial.name);
                    if (returnAsInstance) return Instantiate(libraryMaterial);
                    return libraryMaterial;
                }
            }

            //Didnt find a replacement? Just return myself.
            return comparisonMaterial;
		}

        private bool ColorsAreSimilar(Color colorA, Color colorB, float threshold)
        {
            Vector3 colorAVector = new Vector3(colorA.r, colorA.g, colorA.b);
            Vector3 colorBVector = new Vector3(colorB.r, colorB.g, colorB.b);

            if(Vector3.Distance(colorAVector,colorBVector) < threshold)
            {
                return true;
			}
            return false;
		}
	}
}