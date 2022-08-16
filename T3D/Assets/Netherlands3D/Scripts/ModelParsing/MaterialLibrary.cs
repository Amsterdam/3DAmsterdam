using Netherlands3D.Masking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.ObjectInteraction;

namespace Netherlands3D.ModelParsing
{
    public class MaterialLibrary : MonoBehaviour, IUniqueService
    {
        [SerializeField]
        private Material[] materialLibrary;

        [SerializeField]
        private float materialColorMatchingThreshold = 0.01f;

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
            ServiceLocator.GetService<PropertiesPanel>().OpenObjectInformation("", true,10);
            ServiceLocator.GetService<PropertiesPanel>().AddTitle("Materialen gevonden");
            ServiceLocator.GetService<PropertiesPanel>().AddTextfield("Er zijn <b>" + matchedMaterialNames.Length + " materialen*</b> gevonden die overeenkomen met die uit de bibliotheek. Wil je deze overnemen?");
            ServiceLocator.GetService<PropertiesPanel>().AddActionButtonBig("Ja, neem over", (action) =>
            {
                ApplyMaterialOverrides(renderer);
                ServiceLocator.GetService<PropertiesPanel>().ClearGeneratedFields();
                ServiceLocator.GetService<PropertiesPanel>().OpenCustomObjects();
            });
            ServiceLocator.GetService<PropertiesPanel>().AddActionButtonBig("Nee", (action) =>
            {
                ServiceLocator.GetService<PropertiesPanel>().ClearGeneratedFields();
                ServiceLocator.GetService<PropertiesPanel>().OpenCustomObjects(renderer.GetComponent<Transformable>());
            });

            ServiceLocator.GetService<PropertiesPanel>().AddLabel("<i>*Het gaat om de volgende materialen:</i>");
            foreach(var materialName in matchedMaterialNames)
            {
                ServiceLocator.GetService<PropertiesPanel>().AddTextfield($"<i>- {materialName}</i>");
            }
            
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