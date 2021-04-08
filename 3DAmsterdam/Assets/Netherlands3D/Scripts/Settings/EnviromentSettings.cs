﻿using Netherlands3D;
using Netherlands3D.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnviromentSettings : MonoBehaviour
{
    public static EnviromentProfile activeEnviromentProfile;
    public static Light sun;

    [SerializeField]
    private EnviromentProfile[] selectableProfiles;

	public EnviromentProfile[] SelectableProfiles { get => selectableProfiles; private set => selectableProfiles = value; }

    [SerializeField]
    private Material proceduralSkyMaterial;
    [SerializeField]
    private Material texturedSkyMaterial;

    [SerializeField]
    private Light directionalLightSun;

    [SerializeField]
    private float sunUpAmount = 1.0f;
    [SerializeField]
    private float sunDownAmount = -1.0f;

    [SerializeField]
    private Material intensityMaterialTrees;

    private static bool visualsUpdateRequired = false;

    private static bool useSkyboxForReflections = true;

    private void Awake()
	{
        if (directionalLightSun)
        {
            sun = directionalLightSun;
        }
        else
        {
            sun = FindObjectOfType<Light>();
        }

        //Use first slot as default
        ApplyEnviromentProfile(0);
    }

    public static void SetSunAngle(Vector3 angles)
    {
        sun.transform.localRotation = Quaternion.Euler(angles);
        visualsUpdateRequired = true;
    }

	private void Update()
	{
        if (visualsUpdateRequired)
            UpdateSunBasedVisuals();
    }

	private void Start()
    {
        UpdateSunBasedVisuals();
    }

    public void ApplyEnviromentProfile(int profileIndex)
    {
        var profile = selectableProfiles[profileIndex];
        activeEnviromentProfile = profile;

        if(activeEnviromentProfile.skyMap)
        {
            texturedSkyMaterial.SetTexture("_Tex",activeEnviromentProfile.skyMap);
            //texturedSkyMaterial.SetColor();
            RenderSettings.skybox = texturedSkyMaterial;

            SetReflections(useSkyboxForReflections);
        }
        else
        {
            SetReflections(useSkyboxForReflections);
            RenderSettings.skybox = proceduralSkyMaterial;
        }
        UpdateSunBasedVisuals();
    }

    public static void SetReflections(bool realtimeReflectionsAreOn = false)
    {
        useSkyboxForReflections = realtimeReflectionsAreOn;

        if (!useSkyboxForReflections && activeEnviromentProfile.skyMap)
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflection = activeEnviromentProfile.skyMap;
        }
        else
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }
    }
    
    public void UpdateSunBasedVisuals()
    {
        //Reduce sun strength when we go down the horizon
        sun.intensity = Mathf.InverseLerp(sunDownAmount, sunUpAmount, Vector3.Dot(sun.transform.forward,Vector3.up));

        //Apply sunlight to tree darkness (who use a very simple unlit shader)
        intensityMaterialTrees.SetFloat("_Light", Mathf.Max(sun.intensity, 0.3f));

        //Change the fog and ambient color based on this intensity
        RenderSettings.fogColor = Color.Lerp(
            activeEnviromentProfile.fogColorNight,
            activeEnviromentProfile.fogColorDay,
            sun.intensity
        );

        //Sky colors
        RenderSettings.ambientSkyColor = Color.Lerp(
            activeEnviromentProfile.skyColorsNight[0],
            activeEnviromentProfile.skyColorsDay[0],
            sun.intensity
        );
        RenderSettings.ambientEquatorColor = Color.Lerp(
            activeEnviromentProfile.skyColorsNight[1],
            activeEnviromentProfile.skyColorsDay[1],
            sun.intensity
        );
        RenderSettings.ambientGroundColor = Color.Lerp(
            activeEnviromentProfile.skyColorsNight[2],
            activeEnviromentProfile.skyColorsDay[2],
            sun.intensity
        );

        if(activeEnviromentProfile.skyMap)
            RenderSettings.skybox.SetFloat("_Exposure", sun.intensity);

        visualsUpdateRequired = false;
    }
}
