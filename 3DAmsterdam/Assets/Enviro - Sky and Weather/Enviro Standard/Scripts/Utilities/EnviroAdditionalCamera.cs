using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[AddComponentMenu("Enviro/AddionalCamera")]
public class EnviroAdditionalCamera : MonoBehaviour {

    private Camera myCam;
    private EnviroSkyRendering skyRender;
    private EnviroPostProcessing enviroPostProcessing;

    private void OnEnable()
    {
        myCam = GetComponent<Camera>();

        if (myCam != null)
            InitImageEffects();
    }


	void Update ()
    {
        UpdateCameraComponents();
    }

    private void InitImageEffects()
    {
        skyRender = myCam.gameObject.GetComponent<EnviroSkyRendering>();

        if (skyRender == null)
            skyRender = myCam.gameObject.AddComponent<EnviroSkyRendering>();

        skyRender.isAddionalCamera = true;

#if UNITY_EDITOR
        string[] assets = UnityEditor.AssetDatabase.FindAssets("enviro_spot_cookie", null);
        for (int idx = 0; idx < assets.Length; idx++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[idx]);
            if (path.Length > 0)
            {
                skyRender.DefaultSpotCookie = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);
            }
        }
#endif
        enviroPostProcessing = EnviroSky.instance.PlayerCamera.gameObject.GetComponent<EnviroPostProcessing>();

        if (enviroPostProcessing == null)
            enviroPostProcessing = EnviroSky.instance.PlayerCamera.gameObject.AddComponent<EnviroPostProcessing>();

    }


    private void UpdateCameraComponents()
    {
        if (EnviroSky.instance == null)
            return;

        //Update Fog
        if (skyRender != null)
        {
            skyRender.dirVolumeLighting = EnviroSky.instance.volumeLightSettings.dirVolumeLighting;
            skyRender.volumeLighting = EnviroSky.instance.useVolumeLighting;
            skyRender.distanceFog = EnviroSky.instance.fogSettings.distanceFog;
            skyRender.heightFog = EnviroSky.instance.fogSettings.heightFog;
            skyRender.height = EnviroSky.instance.fogSettings.height;
            skyRender.heightDensity = EnviroSky.instance.fogSettings.heightDensity;
            skyRender.useRadialDistance = EnviroSky.instance.fogSettings.useRadialDistance;
            skyRender.startDistance = EnviroSky.instance.fogSettings.startDistance;
        }
    }
}
