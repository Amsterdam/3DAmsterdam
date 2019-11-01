using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviroCloudsReflection : MonoBehaviour {

    public bool resetCameraProjection = true;
    public bool tonemapping = true;

    private Camera myCam;

    // Volume Clouds
    private Material mat;
    private Material blitMat;
    private Material weatherMapMat;
    private RenderTexture subFrameTex;
    private RenderTexture prevFrameTex;
    private Texture2D curlMap;
    private Texture3D noiseTexture = null;
    private Texture3D detailNoiseTexture = null;
    private Texture3D detailNoiseTextureHigh = null;

    //Cloud Rendering Matrix
    private Matrix4x4 projection;
    private Matrix4x4 projectionSPVR;
    private Matrix4x4 inverseRotation;
    private Matrix4x4 inverseRotationSPVR;
    private Matrix4x4 rotation;
    private Matrix4x4 rotationSPVR;
    private Matrix4x4 previousRotation;
    private Matrix4x4 previousRotationSPVR;
    [HideInInspector]
    public EnviroCloudSettings.ReprojectionPixelSize currentReprojectionPixelSize;
    private int reprojectionPixelSize;

    private bool isFirstFrame;

    private int subFrameNumber;
    private int[] frameList;
    private int renderingCounter;
    private int subFrameWidth;
    private int subFrameHeight;
    private int frameWidth;
    private int frameHeight;
    private bool textureDimensionChanged;


    void Start ()
    {
        //if (EnviroSkyMgr.instance != null && EnviroSkyMgr.instance.currentEnviroSkyVersion != EnviroSkyMgr.EnviroSkyVersion.Full)
        //    this.enabled = false;

        myCam = GetComponent<Camera>();
        CreateMaterialsAndTextures();

        if (EnviroSky.instance != null)
            SetReprojectionPixelSize(EnviroSky.instance.cloudsSettings.reprojectionPixelSize);
    }

    private void CreateMaterialsAndTextures()
    {

        if (mat == null)
            mat = new Material(Shader.Find("Enviro/RaymarchClouds"));

        if (blitMat == null)
            blitMat = new Material(Shader.Find("Enviro/Blit"));

        if (curlMap == null)
            curlMap = Resources.Load("tex_enviro_curl") as Texture2D;

        if (noiseTexture == null)
            noiseTexture = Resources.Load("enviro_clouds_base") as Texture3D;

        if (detailNoiseTexture == null)
            detailNoiseTexture = Resources.Load("enviro_clouds_detail_low") as Texture3D;

        if (detailNoiseTextureHigh == null)
            detailNoiseTextureHigh = Resources.Load("enviro_clouds_detail_high") as Texture3D;
    }

    private void SetCloudProperties()
    {
        mat.SetTexture("_Noise", noiseTexture);

        if (EnviroSky.instance.cloudsSettings.detailQuality == EnviroCloudSettings.CloudDetailQuality.Low)
            mat.SetTexture("_DetailNoise", detailNoiseTexture);
        else
            mat.SetTexture("_DetailNoise", detailNoiseTextureHigh);

        switch (myCam.stereoActiveEye)
        {
            case Camera.MonoOrStereoscopicEye.Mono:

               // if (resetCameraProjection)
               //     myCam.ResetProjectionMatrix();

                projection = myCam.projectionMatrix;

                Matrix4x4 inverseProjection = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjection);
                inverseRotation = myCam.cameraToWorldMatrix;
                mat.SetMatrix("_InverseRotation", inverseRotation);
                break;

            case Camera.MonoOrStereoscopicEye.Left:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                Matrix4x4 inverseProjectionLeft = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjectionLeft);
                inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
                mat.SetMatrix("_InverseRotation", inverseRotation);

                if (EnviroSky.instance.singlePassVR)
                {
                    Matrix4x4 inverseProjectionRightSP = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).inverse;
                    mat.SetMatrix("_InverseProjection_SP", inverseProjectionRightSP);

                    inverseRotationSPVR = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                    mat.SetMatrix("_InverseRotation_SP", inverseRotationSPVR);
                }
                break;

            case Camera.MonoOrStereoscopicEye.Right:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                Matrix4x4 inverseProjectionRight = projection.inverse;
                mat.SetMatrix("_InverseProjection", inverseProjectionRight);
                inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                mat.SetMatrix("_InverseRotation", inverseRotation);
                break;
        }

        mat.SetTexture("_CurlNoise", curlMap);
        mat.SetVector("_Steps", new Vector4(EnviroSky.instance.cloudsSettings.raymarchSteps * EnviroSky.instance.cloudsConfig.raymarchingScale, EnviroSky.instance.cloudsSettings.raymarchSteps * EnviroSky.instance.cloudsConfig.raymarchingScale, 0.0f, 0.0f));
        mat.SetFloat("_BaseNoiseUV", EnviroSky.instance.cloudsSettings.baseNoiseUV);
        mat.SetFloat("_DetailNoiseUV", EnviroSky.instance.cloudsSettings.detailNoiseUV);
        mat.SetFloat("_PrimAtt", EnviroSky.instance.cloudsSettings.primaryAttenuation);
        mat.SetFloat("_SecAtt", EnviroSky.instance.cloudsSettings.secondaryAttenuation);
        mat.SetFloat("_SkyBlending", EnviroSky.instance.cloudsConfig.skyBlending);
        mat.SetFloat("_HgPhaseFactor", EnviroSky.instance.cloudsSettings.hgPhase);
        mat.SetVector("_CloudsParameter", new Vector4(EnviroSky.instance.cloudsSettings.bottomCloudHeight, EnviroSky.instance.cloudsSettings.topCloudHeight, EnviroSky.instance.cloudsSettings.topCloudHeight - EnviroSky.instance.cloudsSettings.bottomCloudHeight, EnviroSky.instance.cloudsSettings.cloudsWorldScale * 10));
        mat.SetFloat("_AmbientLightIntensity", EnviroSky.instance.cloudsSettings.ambientLightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
        mat.SetFloat("_SunLightIntensity", EnviroSky.instance.cloudsSettings.directLightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
        mat.SetFloat("_AlphaCoef", EnviroSky.instance.cloudsConfig.alphaCoef);
        mat.SetFloat("_ExtinctionCoef", EnviroSky.instance.cloudsConfig.scatteringCoef);
        mat.SetFloat("_CloudDensityScale", EnviroSky.instance.cloudsConfig.density);
        mat.SetFloat("_CloudBaseColor", EnviroSky.instance.cloudsConfig.ambientbottomColorBrightness);
        mat.SetFloat("_CloudTopColor", EnviroSky.instance.cloudsConfig.ambientTopColorBrightness);
        mat.SetFloat("_CloudsType", EnviroSky.instance.cloudsConfig.cloudType);
        mat.SetFloat("_CloudsCoverage", EnviroSky.instance.cloudsConfig.coverageHeight);
        mat.SetVector("_CloudsAnimation", new Vector4(EnviroSky.instance.cloudAnim.x, EnviroSky.instance.cloudAnim.y, 0f, 0f));
        mat.SetFloat("_CloudsExposure", EnviroSky.instance.cloudsSettings.cloudsExposure);
        mat.SetFloat("_GlobalCoverage", EnviroSky.instance.cloudsConfig.coverage * EnviroSky.instance.cloudsSettings.globalCloudCoverage);
        mat.SetColor("_LightColor", EnviroSky.instance.cloudsSettings.volumeCloudsColor.Evaluate(EnviroSky.instance.GameTime.solarTime));
        mat.SetColor("_MoonLightColor", EnviroSky.instance.cloudsSettings.volumeCloudsMoonColor.Evaluate(EnviroSky.instance.GameTime.lunarTime));
        mat.SetFloat("_Tonemapping", tonemapping ? 0f : 1f);
        mat.SetFloat("_stepsInDepth", EnviroSky.instance.cloudsSettings.stepsInDepthModificator);
        mat.SetFloat("_LODDistance", EnviroSky.instance.cloudsSettings.lodDistance);
        mat.SetVector("_LightDir", -EnviroSky.instance.Components.DirectLight.transform.forward);
    }

    public void SetBlitmaterialProperties()
    {
        Matrix4x4 inverseProjection = projection.inverse;

        blitMat.SetMatrix("_PreviousRotation", previousRotation);
        blitMat.SetMatrix("_Projection", projection);
        blitMat.SetMatrix("_InverseRotation", inverseRotation);
        blitMat.SetMatrix("_InverseProjection", inverseProjection);

        if (EnviroSky.instance.singlePassVR)
        {
            Matrix4x4 inverseProjectionSPVR = projectionSPVR.inverse;
            blitMat.SetMatrix("_PreviousRotationSPVR", previousRotationSPVR);
            blitMat.SetMatrix("_ProjectionSPVR", projectionSPVR);
            blitMat.SetMatrix("_InverseRotationSPVR", inverseRotationSPVR);
            blitMat.SetMatrix("_InverseProjectionSPVR", inverseProjectionSPVR);
        }

        blitMat.SetFloat("_FrameNumber", subFrameNumber);
        blitMat.SetFloat("_ReprojectionPixelSize", reprojectionPixelSize);
        blitMat.SetVector("_SubFrameDimension", new Vector2(subFrameWidth, subFrameHeight));
        blitMat.SetVector("_FrameDimension", new Vector2(frameWidth, frameHeight));
    }

    public void RenderClouds(RenderTexture tex)
    {
        SetCloudProperties();
        //Render Clouds with downsampling tex
        Graphics.Blit(null, tex, mat);
    }


    private void CreateCloudsRenderTextures(RenderTexture source)
    {
        if (subFrameTex != null)
        {
            DestroyImmediate(subFrameTex);
            subFrameTex = null;
        }

        if (prevFrameTex != null)
        {
            DestroyImmediate(prevFrameTex);
            prevFrameTex = null;
        }

        RenderTextureFormat format = myCam.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;



        if (subFrameTex == null)
        {


#if UNITY_2017_1_OR_NEWER
            RenderTextureDescriptor desc = new RenderTextureDescriptor(subFrameWidth, subFrameHeight, format, 0);
            if (EnviroSky.instance.singlePassVR)
                desc.vrUsage = VRTextureUsage.TwoEyes;
            subFrameTex = new RenderTexture(desc);
#else
            subFrameTex = new RenderTexture(subFrameWidth, subFrameHeight, 0, format);
#endif
            subFrameTex.filterMode = FilterMode.Bilinear;
            subFrameTex.hideFlags = HideFlags.HideAndDontSave;

            isFirstFrame = true;
        }

        if (prevFrameTex == null)
        {


#if UNITY_2017_1_OR_NEWER
            RenderTextureDescriptor desc = new RenderTextureDescriptor(frameWidth, frameHeight, format, 0);
            if (EnviroSky.instance.singlePassVR)
                desc.vrUsage = VRTextureUsage.TwoEyes;
            prevFrameTex = new RenderTexture(desc);
#else
            prevFrameTex = new RenderTexture(frameWidth, frameHeight, 0, format);
#endif

            prevFrameTex.filterMode = FilterMode.Bilinear;
            prevFrameTex.hideFlags = HideFlags.HideAndDontSave;

            isFirstFrame = true;
        }
    }




    void Update ()
    {
        if (currentReprojectionPixelSize != EnviroSky.instance.cloudsSettings.reprojectionPixelSize)
        {
            currentReprojectionPixelSize = EnviroSky.instance.cloudsSettings.reprojectionPixelSize;
            SetReprojectionPixelSize(EnviroSky.instance.cloudsSettings.reprojectionPixelSize);
        }

        mat.SetTexture("_WeatherMap", EnviroSky.instance.weatherMap);
    }


    [ImageEffectOpaque]
    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (EnviroSky.instance == null)
        {
            Graphics.Blit(source, destination);
            return;
        }


        if (EnviroSky.instance.useVolumeClouds)
        {

            StartFrame();

            if (subFrameTex == null || prevFrameTex == null || textureDimensionChanged)
                CreateCloudsRenderTextures(source);

            //RenderingClouds
            RenderClouds(subFrameTex);

            if (isFirstFrame)
            {
                Graphics.Blit(subFrameTex, prevFrameTex);
                isFirstFrame = false;
            }

            //Blit clouds to final image
            blitMat.SetTexture("_MainTex", source);
            blitMat.SetTexture("_SubFrame", subFrameTex);
            blitMat.SetTexture("_PrevFrame", prevFrameTex);
            SetBlitmaterialProperties();

            Graphics.Blit(source, destination, blitMat);
            Graphics.Blit(subFrameTex, prevFrameTex);

            FinalizeFrame();
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    public void SetReprojectionPixelSize(EnviroCloudSettings.ReprojectionPixelSize pSize)
    {
        switch (pSize)
        {
            case EnviroCloudSettings.ReprojectionPixelSize.Off:
                reprojectionPixelSize = 1;
                break;

            case EnviroCloudSettings.ReprojectionPixelSize.Low:
                reprojectionPixelSize = 2;
                break;

            case EnviroCloudSettings.ReprojectionPixelSize.Medium:
                reprojectionPixelSize = 4;
                break;

            case EnviroCloudSettings.ReprojectionPixelSize.High:
                reprojectionPixelSize = 8;
                break;
        }

        frameList = CalculateFrames(reprojectionPixelSize);
    }


    public void StartFrame()
    {
        textureDimensionChanged = UpdateFrameDimensions();

        switch (myCam.stereoActiveEye)
        {
            case Camera.MonoOrStereoscopicEye.Mono:

                if (resetCameraProjection)
                    myCam.ResetProjectionMatrix();

                projection = myCam.projectionMatrix;
                rotation = myCam.worldToCameraMatrix;
                inverseRotation = myCam.cameraToWorldMatrix;
                break;

            case Camera.MonoOrStereoscopicEye.Left:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                rotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                inverseRotation = rotation.inverse;

                if (EnviroSky.instance.singlePassVR)
                {
                    projectionSPVR = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                    rotationSPVR = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                    inverseRotationSPVR = rotationSPVR.inverse;
                }
                break;

            case Camera.MonoOrStereoscopicEye.Right:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                rotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                inverseRotation = rotation.inverse;
                break;
        }
    }

    public void FinalizeFrame()
    {
        renderingCounter++;

        previousRotation = rotation;
        if (EnviroSky.instance.singlePassVR)
            previousRotationSPVR = rotationSPVR;

        int reproSize = reprojectionPixelSize * reprojectionPixelSize;
        subFrameNumber = frameList[renderingCounter % reproSize];
    }

    private bool UpdateFrameDimensions()
    {
        //Add downsampling
        int newFrameWidth = myCam.pixelWidth / EnviroSky.instance.cloudsSettings.cloudsRenderResolution;
        int newFrameHeight = myCam.pixelHeight / EnviroSky.instance.cloudsSettings.cloudsRenderResolution;

        //Calculate new frame width and height
        while (newFrameWidth % reprojectionPixelSize != 0)
        {
            newFrameWidth++;
        }

        while (newFrameHeight % reprojectionPixelSize != 0)
        {
            newFrameHeight++;
        }

        int newSubFrameWidth = newFrameWidth / reprojectionPixelSize;
        int newSubFrameHeight = newFrameHeight / reprojectionPixelSize;

        //Check if diemensions changed
        if (newFrameWidth != frameWidth || newSubFrameWidth != subFrameWidth || newFrameHeight != frameHeight || newSubFrameHeight != subFrameHeight)
        {
            //Cache new dimensions
            frameWidth = newFrameWidth;
            frameHeight = newFrameHeight;
            subFrameWidth = newSubFrameWidth;
            subFrameHeight = newSubFrameHeight;
            return true;
        }
        else
        {
            //Cache new dimensions
            frameWidth = newFrameWidth;
            frameHeight = newFrameHeight;
            subFrameWidth = newSubFrameWidth;
            subFrameHeight = newSubFrameHeight;
            return false;
        }
    }

    // Reprojection
    private int[] CalculateFrames(int reproSize)
    {
        subFrameNumber = 0;

        int i = 0;
        int reproCount = reproSize * reproSize;
        int[] frameNumbers = new int[reproCount];

        for (i = 0; i < reproCount; i++)
        {
            frameNumbers[i] = i;
        }

        while (i-- > 0)
        {
            int frame = frameNumbers[i];
            int count = (int)(UnityEngine.Random.Range(0, 1) * 1000.0f) % reproCount;
            frameNumbers[i] = frameNumbers[count];
            frameNumbers[count] = frame;
        }

        return frameNumbers;
    }


}
