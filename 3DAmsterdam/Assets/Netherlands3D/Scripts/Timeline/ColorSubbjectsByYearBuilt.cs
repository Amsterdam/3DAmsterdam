using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSubbjectsByYearBuilt : MonoBehaviour
{
    [Header("Listen to")]
    [SerializeField] private ObjectEvent gotIdsAndYears;

    Dictionary<string, float> keyValuePairs = new Dictionary<string, float>();
    Dictionary<string, Color> bagIdAndColors = new Dictionary<string, Color>();

    [Header("Invoke events")]
    [SerializeField] private BoolEvent workingOnColoring;
    [SerializeField] private ObjectEvent applyColorsToIds;

    [Header("Color lerp")]
    [SerializeField] LayerMask buildingsLayerMask;
    [SerializeField] private Color buildYearBeforeColor;
    [SerializeField] private Color buildYearAfterColor;
    [SerializeField] private Material buildingsMaterial;

    [SerializeField] private float startRange = 1900;
    [SerializeField] private float endRange = 2022;

    [SerializeField] private string shaderThreshold = "_Threshold";
    [SerializeField] private string shaderStart = "_ColorBefore";
    [SerializeField] private string shaderEnd = "_ColorAfter";

    [SerializeField]
    private Material buildingMaterialWithMultiplyRange;
    private Shader defaultBuildingShader;


    private void OnEnable()
    {
        if (defaultBuildingShader == null)
            defaultBuildingShader = buildingsMaterial.shader;

        ApplyBuildingShaderWithThreshold(1);

        gotIdsAndYears.started.AddListener(GotListIdsAndYears);
    }

    private void ApplyBuildingShaderWithThreshold(float threshold)
    {
        buildingsMaterial.shader = buildingMaterialWithMultiplyRange.shader;
        buildingsMaterial.SetColor(shaderStart, buildYearBeforeColor);
        buildingsMaterial.SetColor(shaderEnd, buildYearAfterColor);
        buildingsMaterial.SetFloat(shaderThreshold, threshold);

        var meshRenderers = FindObjectsOfType<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            if (meshRenderer.renderingLayerMask == buildingsLayerMask.value)
            {
                meshRenderer.sharedMaterial = buildingsMaterial;
            }
        }
    }

    private void RevertDefaultBuildingsShader()
    {
        buildingsMaterial.shader = defaultBuildingShader;
        var meshRenderers = FindObjectsOfType<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers)
        {
            if (meshRenderer.renderingLayerMask == buildingsLayerMask.value)
            {
                meshRenderer.sharedMaterial = buildingsMaterial;
            }
        }
    }

    private void OnDisable()
    {
        if (defaultBuildingShader != null)
            buildingsMaterial.shader = defaultBuildingShader;

        RevertDefaultBuildingsShader();

        keyValuePairs.Clear();
        bagIdAndColors.Clear();

        gotIdsAndYears.started.RemoveListener(GotListIdsAndYears);
        workingOnColoring.started.Invoke(false);
    }

    private void GotListIdsAndYears(object idsAndYears)
    {
        keyValuePairs = (Dictionary<string, float>)idsAndYears;
        ApplyYearsToVertexColors();
    }

    public void ApplyYearsToVertexColors()
    {
        if (keyValuePairs != null)
        {
            workingOnColoring.started.Invoke(true);

            foreach (KeyValuePair<string, float> idAndYear in keyValuePairs) {
                if (!bagIdAndColors.ContainsKey(idAndYear.Key))
                {
                    var normalizedYear = NormalizeYearInRange(idAndYear.Value);
                    bagIdAndColors.Add(idAndYear.Key, Color.Lerp(Color.black, Color.white, normalizedYear));
                }
            }
            applyColorsToIds.started.Invoke(bagIdAndColors);
            workingOnColoring.started.Invoke(false);
        }
    }
    public void SetYearAsShaderThreshold(DateTime dateTime)
    {
        if (!gameObject.activeSelf) return;

        var normalizedYear = NormalizeYearInRange(dateTime.Year);
        ApplyBuildingShaderWithThreshold(normalizedYear);
    }

    private float NormalizeYearInRange(float year)
    {
        return Mathf.InverseLerp(startRange, endRange, year);
    }
}
