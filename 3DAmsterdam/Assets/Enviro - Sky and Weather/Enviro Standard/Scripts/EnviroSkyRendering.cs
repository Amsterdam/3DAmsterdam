using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class EnviroSkyRendering : MonoBehaviour
{
    [HideInInspector]
    public bool isAddionalCamera = false;
    private Camera myCam;
	private RenderTexture spSatTex;
	private Camera spSatCam;

    ////////////////////// Clouds //////////////////////
    private Material cloudsMat;
    private Material blitMat;
    private RenderTexture subFrameTex;
    private RenderTexture prevFrameTex;
    private Texture2D curlMap;
    private Texture2D dither;
   // private Texture2D bayer4x4;
    private Texture3D noiseTexture = null;
    private Texture3D noiseTextureHigh = null;
    private Texture3D detailNoiseTexture = null;
	private Texture3D detailNoiseTextureHigh = null;
	private Matrix4x4 projection;
    private Matrix4x4 projectionSPVR;
    private Matrix4x4 inverseRotation;
    private Matrix4x4 inverseRotationSPVR;
    private Matrix4x4 rotation;
    private Matrix4x4 rotationSPVR;
    private Matrix4x4 previousRotation;
    private Matrix4x4 previousRotationSPVR;
    [HideInInspector]public EnviroCloudSettings.ReprojectionPixelSize currentReprojectionPixelSize;
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

    ///////////////// Fog //////////////////////
    [HideInInspector]
    public bool simpleFog = false;
    private bool currentSimpleFog = false;
    [HideInInspector]
    public bool volumeLighting = true;
    [HideInInspector]
    public bool dirVolumeLighting = true;
    [HideInInspector]
    public bool distanceFog = true;
    [HideInInspector]
    public bool useRadialDistance = false;
    [HideInInspector]
    public bool heightFog = true;
    [HideInInspector]
    public float height = 1.0f;
    [Range(0.001f, 10.0f)]
    [HideInInspector]
    public float heightDensity = 2.0f;
    [HideInInspector]
    public float startDistance = 0.0f;

    ///////////////// Volume Lighting //////////////////////
    public enum VolumtericResolution
    {
        Full,
        Half,
        Quarter
    };
    public static event Action<EnviroSkyRendering, Matrix4x4, Matrix4x4> PreRenderEvent;
    private static Mesh _pointLightMesh;
    private static Mesh _spotLightMesh;
    private static Material _lightMaterial;
    private CommandBuffer _preLightPass;
    public CommandBuffer _afterLightPass;
    private Matrix4x4 _viewProj;
    private Matrix4x4 _viewProjSP;
    [HideInInspector]
    public Material fogMat;
    private Material _bilateralBlurMaterial;
    private RenderTexture _volumeLightTexture;
    private RenderTexture _halfVolumeLightTexture;
    private RenderTexture _quarterVolumeLightTexture;
    private static Texture _defaultSpotCookie;
    private RenderTexture _halfDepthBuffer;
    private RenderTexture _quarterDepthBuffer;
    private Texture2D _ditheringTexture;
    private Texture2D blackTexture;
    private Texture3D _noiseTexture;
    [HideInInspector]
    public VolumtericResolution Resolution = VolumtericResolution.Quarter;
    private VolumtericResolution _currentResolution = VolumtericResolution.Quarter;
    [HideInInspector]
    public Texture DefaultSpotCookie;
    private Material volumeLightMat;
    public CommandBuffer GlobalCommandBuffer { get { return _preLightPass; } }
    public CommandBuffer GlobalCommandBufferForward { get { return _afterLightPass; } }
    public static Material GetLightMaterial()
    {
        return _lightMaterial;
    }
    public static Mesh GetPointLightMesh()
    {
        return _pointLightMesh;
    }
    public static Mesh GetSpotLightMesh()
    {
        return _spotLightMesh;
    }
    public RenderTexture GetVolumeLightBuffer()
    {
#if ENVIRO_HD
        if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
            return _quarterVolumeLightTexture;
        else if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
            return _halfVolumeLightTexture;
        else
            return _volumeLightTexture;
#else
        return null;
#endif
    }
    public RenderTexture GetVolumeLightDepthBuffer()
    {
#if ENVIRO_HD
        if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Quarter)
            return _quarterDepthBuffer;
        else if (EnviroSky.instance.volumeLightSettings.Resolution == VolumtericResolution.Half)
            return _halfDepthBuffer;
        else
            return null;
#else
        return null;
#endif
    }
    public static Texture GetDefaultSpotCookie()
    {
        return _defaultSpotCookie;
    }

    /////////////////// Blur //////////////////////
    private Material postProcessMat;
    private const int kMaxIterations = 16;
    private RenderTexture[] _blurBuffer1 = new RenderTexture[kMaxIterations];
    private  RenderTexture[] _blurBuffer2 = new RenderTexture[kMaxIterations];
    private Texture2D distributionTexture;
    private float _threshold = 0f;
    public float thresholdGamma
    {
        get { return Mathf.Max(0f, 0); }
        set { _threshold = value; }
    }

    public float thresholdLinear
    {
        get { return Mathf.GammaToLinearSpace(thresholdGamma); }
        set { _threshold = Mathf.LinearToGammaSpace(value); }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////



    void OnEnable()
    {
       if (myCam == null)
           myCam = GetComponent<Camera>();

        if (myCam.actualRenderingPath == RenderingPath.Forward)
            myCam.depthTextureMode = DepthTextureMode.Depth;

        CreateMaterialsAndTextures();

        if (EnviroSky.instance != null)
            SetReprojectionPixelSize(EnviroSky.instance.cloudsSettings.reprojectionPixelSize);

        _currentResolution = Resolution;

        volumeLightMat = new Material(Shader.Find("Enviro/VolumeLight"));

        postProcessMat = new Material(Shader.Find("Hidden/EnviroDistanceBlur"));

        Shader shaderBlur = Shader.Find("Hidden/EnviroBilateralBlur");
        if (shaderBlur == null)
            throw new Exception("Critical Error: \"Hidden/EnviroBilateralBlur\" shader is missing.");
        _bilateralBlurMaterial = new Material(shaderBlur);

#if UNITY_2017_2
        _bilateralBlurMaterial.EnableKeyword("UNITY2017_2_SP");
#endif

        ChangeResolution();

        if (_pointLightMesh == null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _pointLightMesh = go.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(go);
        }

        if (_spotLightMesh == null)
        {
            _spotLightMesh = CreateSpotLightMesh();
        }

        if (_lightMaterial == null)
        {
            Shader shaderLight = Shader.Find("Enviro/VolumeLight");
            if (shaderLight == null)
                throw new Exception("Critical Error: \"Enviro/VolumeLight\" shader is missing.");
            _lightMaterial = new Material(shaderLight);
        }

        if (_defaultSpotCookie == null)
        {
            _defaultSpotCookie = DefaultSpotCookie;
        }

        LoadNoise3dTexture();
        GenerateDitherTexture();

        _preLightPass = new CommandBuffer();
        _preLightPass.name = "PreLight";

        _afterLightPass = new CommandBuffer();
        _afterLightPass.name = "AfterLight";

        if (myCam.actualRenderingPath == RenderingPath.Forward)
        {
            myCam.AddCommandBuffer(CameraEvent.AfterDepthTexture, _preLightPass);
            myCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, _afterLightPass);
        }
        else
        {
            myCam.AddCommandBuffer(CameraEvent.BeforeLighting, _preLightPass);
        }

        CreateFogMaterial();
    }

    void OnDisable()
    {
        if (myCam.actualRenderingPath == RenderingPath.Forward)
        {
            myCam.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _preLightPass);
            myCam.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, _afterLightPass);
        }
        else
        {
            myCam.RemoveCommandBuffer(CameraEvent.BeforeLighting, _preLightPass);
        }


        //Cleanup
        if (postProcessMat != null)
            DestroyImmediate(postProcessMat);

        if (volumeLightMat != null)
            DestroyImmediate(volumeLightMat);

        if (_bilateralBlurMaterial != null)
            DestroyImmediate(_bilateralBlurMaterial);

        if (_bilateralBlurMaterial != null)
            DestroyImmediate(_bilateralBlurMaterial);

        if (cloudsMat != null)
            DestroyImmediate(cloudsMat);      
    }

    private void ChangeResolution()
    {
        int width = myCam.pixelWidth;
        int height = myCam.pixelHeight;

        if (_volumeLightTexture != null)
            DestroyImmediate(_volumeLightTexture);

        _volumeLightTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
        _volumeLightTexture.name = "VolumeLightBuffer";
        _volumeLightTexture.filterMode = FilterMode.Bilinear;

#if UNITY_2017_2
        if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
        {
            _volumeLightTexture.vrUsage = VRTextureUsage.TwoEyes;
        }
#endif

#if UNITY_2017_3_OR_NEWER || UNITY_2017_1
           if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
               {
            if (Resolution == VolumtericResolution.Half || Resolution == VolumtericResolution.Quarter)
                        _volumeLightTexture.vrUsage = VRTextureUsage.None;
               else
                   _volumeLightTexture.vrUsage = VRTextureUsage.TwoEyes;
        }
#endif

        if (_halfDepthBuffer != null)
            DestroyImmediate(_halfDepthBuffer);
        if (_halfVolumeLightTexture != null)
            DestroyImmediate(_halfVolumeLightTexture);

        if (Resolution == VolumtericResolution.Half || Resolution == VolumtericResolution.Quarter)
        {
            _halfVolumeLightTexture = new RenderTexture(width / 2, height / 2, 0, RenderTextureFormat.ARGBHalf);
            _halfVolumeLightTexture.name = "VolumeLightBufferHalf";
            _halfVolumeLightTexture.filterMode = FilterMode.Bilinear;
#if UNITY_2017_1_OR_NEWER
            if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
                _halfVolumeLightTexture.vrUsage = VRTextureUsage.TwoEyes;
#endif
            _halfDepthBuffer = new RenderTexture(width / 2, height / 2, 0, RenderTextureFormat.RFloat);
            _halfDepthBuffer.name = "VolumeLightHalfDepth";
            _halfDepthBuffer.Create();
            _halfDepthBuffer.filterMode = FilterMode.Point;
        }

        if (_quarterVolumeLightTexture != null)
            DestroyImmediate(_quarterVolumeLightTexture);
        if (_quarterDepthBuffer != null)
            DestroyImmediate(_quarterDepthBuffer);

        if (Resolution == VolumtericResolution.Quarter)
        {
            _quarterVolumeLightTexture = new RenderTexture(width / 4, height / 4, 0, RenderTextureFormat.ARGBHalf);
            _quarterVolumeLightTexture.name = "VolumeLightBufferQuarter";
            _quarterVolumeLightTexture.filterMode = FilterMode.Bilinear;
#if UNITY_2017_1_OR_NEWER
            if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
                _quarterVolumeLightTexture.vrUsage = VRTextureUsage.TwoEyes;
#endif
            _quarterDepthBuffer = new RenderTexture(width / 4, height / 4, 0, RenderTextureFormat.RFloat);
            _quarterDepthBuffer.name = "VolumeLightQuarterDepth";
            _quarterDepthBuffer.Create();
            _quarterDepthBuffer.filterMode = FilterMode.Point;
        }
    }

    private void CreateFogMaterial ()
    {
        if (fogMat != null)
            DestroyImmediate(fogMat);

        if (!simpleFog)
        {
            Shader shader = Shader.Find("Enviro/EnviroFogRendering");
            if (shader == null)
                throw new Exception("Critical Error: \"Enviro/EnviroFogRendering\" shader is missing.");
            fogMat = new Material(shader);
        }
        else
        {
            Shader shader = Shader.Find("Enviro/EnviroFogRenderingSimple");
            if (shader == null)
                throw new Exception("Critical Error: \"Enviro/EnviroFogRendering\" shader is missing.");
            fogMat = new Material(shader);
        }
    }

    private void CreateMaterialsAndTextures ()
    {

        if (cloudsMat == null)
            cloudsMat = new Material(Shader.Find("Enviro/RaymarchClouds"));

        if (blitMat == null)
            blitMat = new Material(Shader.Find("Enviro/Blit"));

        if (curlMap == null)
            curlMap = Resources.Load("tex_enviro_curl") as Texture2D;

        if (blackTexture == null)
            blackTexture = Resources.Load("tex_enviro_black") as Texture2D;

        if (noiseTextureHigh == null)
            noiseTextureHigh = Resources.Load("enviro_clouds_base") as Texture3D;

        if (noiseTexture == null)
            noiseTexture = Resources.Load("enviro_clouds_base_low") as Texture3D;

        if (detailNoiseTexture == null)
            detailNoiseTexture = Resources.Load("enviro_clouds_detail_low") as Texture3D;

        if (detailNoiseTextureHigh == null)
            detailNoiseTextureHigh = Resources.Load("enviro_clouds_detail_high") as Texture3D;

        if (dither == null)
            dither = Resources.Load("tex_enviro_dither") as Texture2D;

       // if(bayer4x4 == null)
       //     bayer4x4 = Resources.Load("tex_enviro_bayer4x4") as Texture2D;

        if (distributionTexture == null)
            distributionTexture = Resources.Load("tex_enviro_linear", typeof(Texture2D)) as Texture2D;
    }

    void OnPreRender ()
	{
        //Volume Lighting
        if (volumeLighting)
        {
            Matrix4x4 projLeft = Matrix4x4.Perspective(myCam.fieldOfView, myCam.aspect, 0.01f, myCam.farClipPlane);
            Matrix4x4 projRight = Matrix4x4.Perspective(myCam.fieldOfView, myCam.aspect, 0.01f, myCam.farClipPlane);

            if (myCam.stereoEnabled)
            {
                projLeft = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                projLeft = GL.GetGPUProjectionMatrix(projLeft, true);
                projRight = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                projRight = GL.GetGPUProjectionMatrix(projRight, true);
            }
            else
            {
                projLeft = Matrix4x4.Perspective(myCam.fieldOfView, myCam.aspect, 0.01f, myCam.farClipPlane);
                projLeft = GL.GetGPUProjectionMatrix(projLeft, true);
            }

            // use very low value for near clip plane to simplify cone/frustum intersection 
            if (myCam.stereoEnabled)
            {
                _viewProj = projLeft * myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                _viewProjSP = projRight * myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
            }
            else
            {
                _viewProj = projLeft * myCam.worldToCameraMatrix;
                _viewProjSP = projRight * myCam.worldToCameraMatrix;
            }

            _preLightPass.Clear();
            _afterLightPass.Clear();

            bool dx11 = SystemInfo.graphicsShaderLevel > 40;

            if (Resolution == VolumtericResolution.Quarter)
            {
                Texture nullTexture = null;
                // down sample depth to half res
                _preLightPass.Blit(nullTexture, _halfDepthBuffer, _bilateralBlurMaterial, dx11 ? 4 : 10);
                // down sample depth to quarter res
                _preLightPass.Blit(nullTexture, _quarterDepthBuffer, _bilateralBlurMaterial, dx11 ? 6 : 11);

                _preLightPass.SetRenderTarget(_quarterVolumeLightTexture);
            }
            else if (Resolution == VolumtericResolution.Half)
            {
                Texture nullTexture = null;
                // down sample depth to half res
                _preLightPass.Blit(nullTexture, _halfDepthBuffer, _bilateralBlurMaterial, dx11 ? 4 : 10);

                _preLightPass.SetRenderTarget(_halfVolumeLightTexture);
            }
            else
            {
                _preLightPass.SetRenderTarget(_volumeLightTexture);
            }

            _preLightPass.ClearRenderTarget(false, true, new Color(0, 0, 0, 1));

            UpdateMaterialParameters();

            if (PreRenderEvent != null)
                PreRenderEvent(this, _viewProj, _viewProjSP);
        }


        ///////////////////Matrix Information
        if (myCam.stereoEnabled)
        {
            // Both stereo eye inverse view matrices
            Matrix4x4 left_world_from_view = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
            Matrix4x4 right_world_from_view = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;

            // Both stereo eye inverse projection matrices, plumbed through GetGPUProjectionMatrix to compensate for render texture
            Matrix4x4 left_screen_from_view = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            Matrix4x4 right_screen_from_view = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            Matrix4x4 left_view_from_screen = GL.GetGPUProjectionMatrix(left_screen_from_view, true).inverse;
            Matrix4x4 right_view_from_screen = GL.GetGPUProjectionMatrix(right_screen_from_view, true).inverse;

            // Negate [1,1] to reflect Unity's CBuffer state
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
            {
                left_view_from_screen[1, 1] *= -1;
                right_view_from_screen[1, 1] *= -1;
            }

            Shader.SetGlobalMatrix("_LeftWorldFromView", left_world_from_view);
            Shader.SetGlobalMatrix("_RightWorldFromView", right_world_from_view);
            Shader.SetGlobalMatrix("_LeftViewFromScreen", left_view_from_screen);
            Shader.SetGlobalMatrix("_RightViewFromScreen", right_view_from_screen);
        }
        else
        {
            // Main eye inverse view matrix
            Matrix4x4 left_world_from_view = myCam.cameraToWorldMatrix;

            // Inverse projection matrices, plumbed through GetGPUProjectionMatrix to compensate for render texture
            Matrix4x4 screen_from_view = myCam.projectionMatrix;
            Matrix4x4 left_view_from_screen = GL.GetGPUProjectionMatrix(screen_from_view, true).inverse;

            // Negate [1,1] to reflect Unity's CBuffer state
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
                left_view_from_screen[1, 1] *= -1;

            // Store matrices
            Shader.SetGlobalMatrix("_LeftWorldFromView", left_world_from_view);
            Shader.SetGlobalMatrix("_LeftViewFromScreen", left_view_from_screen);
        }
        //////////////////////////////


        if (EnviroSky.instance == null)
            return;

        //Satellites
        if (myCam != null) {
			switch (myCam.stereoActiveEye) {
			case Camera.MonoOrStereoscopicEye.Mono:
				if (EnviroSky.instance.satCamera != null)
					RenderCamera (EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Mono);
				break;

			case Camera.MonoOrStereoscopicEye.Left:
				if (EnviroSky.instance.satCamera != null)
					RenderCamera (EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Left);
				break;

			case Camera.MonoOrStereoscopicEye.Right:
				if (EnviroSky.instance.satCamera != null)
					RenderCamera (EnviroSky.instance.satCamera, Camera.MonoOrStereoscopicEye.Right);
				break;
			}
				
			if (EnviroSky.instance.satCamera != null)
				RenderSettings.skybox.SetTexture ("_SatTex", EnviroSky.instance.satCamera.targetTexture);
		}
	}

    void RenderCamera(Camera targetCam, Camera.MonoOrStereoscopicEye eye)
	{
		targetCam.fieldOfView = EnviroSky.instance.PlayerCamera.fieldOfView;	
		targetCam.aspect = EnviroSky.instance.PlayerCamera.aspect;

		switch (eye) 
		{
		case Camera.MonoOrStereoscopicEye.Mono:
			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.worldToCameraMatrix;
			targetCam.Render ();
			break;

		case Camera.MonoOrStereoscopicEye.Left:

			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.projectionMatrix = EnviroSky.instance.PlayerCamera.GetStereoProjectionMatrix (Camera.StereoscopicEye.Left);
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.GetStereoViewMatrix (Camera.StereoscopicEye.Left);
			targetCam.Render ();

		/*	if (EnviroSky.instance.singlePassVR == true) 
			{
			   if (targetCam == EnviroSky.instance.skyCamera && spSkyCam != null) {
					spSkyCam.fieldOfView = EnviroSky.instance.PlayerCamera.fieldOfView;	
					spSkyCam.aspect = EnviroSky.instance.PlayerCamera.aspect;
					spSkyCam.projectionMatrix = EnviroSky.instance.PlayerCamera.GetStereoProjectionMatrix (Camera.StereoscopicEye.Right);
					spSkyCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.GetStereoViewMatrix (Camera.StereoscopicEye.Right);
					spSkyCam.Render ();
					material.SetTexture ("_SkySPSR", spSkytex);
				}
			}*/
			break;

		case Camera.MonoOrStereoscopicEye.Right:
			targetCam.transform.position = EnviroSky.instance.PlayerCamera.transform.position;
			targetCam.transform.rotation = EnviroSky.instance.PlayerCamera.transform.rotation;
			targetCam.projectionMatrix = EnviroSky.instance.PlayerCamera.GetStereoProjectionMatrix (Camera.StereoscopicEye.Right);
			targetCam.worldToCameraMatrix = EnviroSky.instance.PlayerCamera.GetStereoViewMatrix (Camera.StereoscopicEye.Right);
			targetCam.Render ();
			break;
		}
	}

    void Update()
    {
        if (EnviroSky.instance == null || myCam == null)
            return;

        if (_currentResolution != Resolution)
        {
            _currentResolution = Resolution;
            ChangeResolution();
        }

        if(currentReprojectionPixelSize != EnviroSky.instance.cloudsSettings.reprojectionPixelSize)
        {
            currentReprojectionPixelSize = EnviroSky.instance.cloudsSettings.reprojectionPixelSize;
            SetReprojectionPixelSize(EnviroSky.instance.cloudsSettings.reprojectionPixelSize);
        }

        if (_volumeLightTexture != null && (_volumeLightTexture.width != myCam.pixelWidth || _volumeLightTexture.height != myCam.pixelHeight))
            ChangeResolution();

        if (currentSimpleFog != simpleFog)
        {
            CreateFogMaterial();
            currentSimpleFog = simpleFog;
        }
    }

    private void SetCloudProperties ()
    {
        cloudsMat.SetTexture("_Noise", noiseTextureHigh);
        cloudsMat.SetTexture("_NoiseLow", noiseTexture);

        if (EnviroSky.instance.cloudsSettings.detailQuality == EnviroCloudSettings.CloudDetailQuality.Low)
            cloudsMat.SetTexture("_DetailNoise", detailNoiseTexture);
        else
            cloudsMat.SetTexture("_DetailNoise", detailNoiseTextureHigh);

        switch (myCam.stereoActiveEye)
        {
            case Camera.MonoOrStereoscopicEye.Mono:
                projection = myCam.projectionMatrix;
                Matrix4x4 inverseProjection = projection.inverse;
                cloudsMat.SetMatrix("_InverseProjection", inverseProjection);
                inverseRotation = myCam.cameraToWorldMatrix;
                cloudsMat.SetMatrix("_InverseRotation", inverseRotation);
                break;

            case Camera.MonoOrStereoscopicEye.Left:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                Matrix4x4 inverseProjectionLeft = projection.inverse;
                cloudsMat.SetMatrix("_InverseProjection", inverseProjectionLeft);
                inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
                cloudsMat.SetMatrix("_InverseRotation", inverseRotation);

                if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
                {
                    Matrix4x4 inverseProjectionRightSP = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).inverse;
                    cloudsMat.SetMatrix("_InverseProjection_SP", inverseProjectionRightSP);

                    inverseRotationSPVR = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                    cloudsMat.SetMatrix("_InverseRotation_SP", inverseRotationSPVR);
                }
                break;

            case Camera.MonoOrStereoscopicEye.Right:
                projection = myCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                Matrix4x4 inverseProjectionRight = projection.inverse;
                cloudsMat.SetMatrix("_InverseProjection", inverseProjectionRight);
                inverseRotation = myCam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;
                cloudsMat.SetMatrix("_InverseRotation", inverseRotation);
                break;
        }

        if(EnviroSky.instance.cloudsSettings.customWeatherMap == null)
            cloudsMat.SetTexture("_WeatherMap", EnviroSky.instance.weatherMap);
        else
            cloudsMat.SetTexture("_WeatherMap", EnviroSky.instance.cloudsSettings.customWeatherMap);

        cloudsMat.SetTexture("_CurlNoise", curlMap);
        cloudsMat.SetVector("_Steps", new Vector4(EnviroSky.instance.cloudsSettings.raymarchSteps* EnviroSky.instance.cloudsConfig.raymarchingScale, EnviroSky.instance.cloudsSettings.raymarchSteps * EnviroSky.instance.cloudsConfig.raymarchingScale, 0.0f, 0.0f));
        cloudsMat.SetFloat("_BaseNoiseUV", EnviroSky.instance.cloudsSettings.baseNoiseUV);
        cloudsMat.SetFloat("_DetailNoiseUV", EnviroSky.instance.cloudsSettings.detailNoiseUV);
        cloudsMat.SetFloat("_PrimAtt", EnviroSky.instance.cloudsSettings.primaryAttenuation);
        cloudsMat.SetFloat("_SecAtt", EnviroSky.instance.cloudsSettings.secondaryAttenuation);
        cloudsMat.SetFloat("_SkyBlending", EnviroSky.instance.cloudsConfig.skyBlending);
        cloudsMat.SetFloat("_HgPhaseFactor", EnviroSky.instance.cloudsSettings.hgPhase);

        if(myCam.transform.position.y < EnviroSky.instance.cloudsSettings.bottomCloudHeight - 250)
            cloudsMat.SetVector("_CloudsParameter", new Vector4(EnviroSky.instance.cloudsSettings.bottomCloudHeight, EnviroSky.instance.cloudsSettings.topCloudHeight, EnviroSky.instance.cloudsSettings.topCloudHeight - EnviroSky.instance.cloudsSettings.bottomCloudHeight, EnviroSky.instance.cloudsSettings.cloudsWorldScale*10));
        else
            cloudsMat.SetVector("_CloudsParameter", new Vector4(myCam.transform.position.y + 250, EnviroSky.instance.cloudsSettings.topCloudHeight + ((myCam.transform.position.y + 250) - EnviroSky.instance.cloudsSettings.bottomCloudHeight), (EnviroSky.instance.cloudsSettings.topCloudHeight + ((myCam.transform.position.y + 250) - EnviroSky.instance.cloudsSettings.bottomCloudHeight)) - (myCam.transform.position.y + 250), EnviroSky.instance.cloudsSettings.cloudsWorldScale * 10));

        cloudsMat.SetFloat("_AmbientLightIntensity", EnviroSky.instance.cloudsSettings.ambientLightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
        cloudsMat.SetFloat("_SunLightIntensity", EnviroSky.instance.cloudsSettings.directLightIntensity.Evaluate(EnviroSky.instance.GameTime.solarTime));
        cloudsMat.SetFloat("_AlphaCoef", EnviroSky.instance.cloudsConfig.alphaCoef);
        cloudsMat.SetFloat("_ExtinctionCoef", EnviroSky.instance.cloudsConfig.scatteringCoef);
        cloudsMat.SetFloat("_CloudDensityScale", EnviroSky.instance.cloudsConfig.density);
        cloudsMat.SetFloat("_CloudBaseColor", EnviroSky.instance.cloudsConfig.ambientbottomColorBrightness);
        cloudsMat.SetFloat("_CloudTopColor", EnviroSky.instance.cloudsConfig.ambientTopColorBrightness);
        cloudsMat.SetFloat("_CloudsType", EnviroSky.instance.cloudsConfig.cloudType);
        cloudsMat.SetFloat("_CloudsCoverage", EnviroSky.instance.cloudsConfig.coverageHeight);
        cloudsMat.SetVector("_CloudsAnimation", new Vector4(EnviroSky.instance.cloudAnim.x, EnviroSky.instance.cloudAnim.y, 0f, 0f));
        cloudsMat.SetFloat("_CloudsExposure", EnviroSky.instance.cloudsSettings.cloudsExposure);
        cloudsMat.SetFloat("_GlobalCoverage", EnviroSky.instance.cloudsConfig.coverage * EnviroSky.instance.cloudsSettings.globalCloudCoverage);
        cloudsMat.SetColor("_LightColor", EnviroSky.instance.cloudsSettings.volumeCloudsColor.Evaluate(EnviroSky.instance.GameTime.solarTime));
        cloudsMat.SetColor("_MoonLightColor", EnviroSky.instance.cloudsSettings.volumeCloudsMoonColor.Evaluate(EnviroSky.instance.GameTime.lunarTime));
        if(!Application.isPlaying && EnviroSky.instance.showVolumeCloudsInEditor)
            cloudsMat.SetFloat("_Tonemapping", 0f);
        else
            cloudsMat.SetFloat("_Tonemapping", EnviroSky.instance.cloudsSettings.tonemapping ? 0f : 1f);
        cloudsMat.SetFloat("_stepsInDepth", EnviroSky.instance.cloudsSettings.stepsInDepthModificator);
        cloudsMat.SetFloat("_LODDistance", EnviroSky.instance.cloudsSettings.lodDistance);
        cloudsMat.SetVector("_LightDir", -EnviroSky.instance.Components.DirectLight.transform.forward);
       // cloudsMat.SetTexture("_bayerNoise", bayer4x4);
    }

    private void SetBlitmaterialProperties()
    {
        Matrix4x4 inverseProjection = projection.inverse;

        blitMat.SetMatrix("_PreviousRotation", previousRotation);
        blitMat.SetMatrix("_Projection", projection);
        blitMat.SetMatrix("_InverseRotation", inverseRotation);
        blitMat.SetMatrix("_InverseProjection", inverseProjection);

        if(myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
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

    private void RenderClouds (RenderTexture tex)
	{
        if(cloudsMat == null)
            cloudsMat = new Material(Shader.Find("Enviro/RaymarchClouds"));

        SetCloudProperties();
        //Render Clouds with downsampling tex
		Graphics.Blit(null, tex, cloudsMat);
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
            if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
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
            if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
                desc.vrUsage = VRTextureUsage.TwoEyes;
            prevFrameTex = new RenderTexture(desc);
#else
            prevFrameTex = new RenderTexture(frameWidth, frameHeight,0, format);
#endif

            prevFrameTex.filterMode = FilterMode.Bilinear;
            prevFrameTex.hideFlags = HideFlags.HideAndDontSave;

            isFirstFrame = true;
        }
    }

    [ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
        if (EnviroSky.instance == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (fogMat == null)
            CreateFogMaterial();

        // Set Camera to render depth in forward
        if (myCam.actualRenderingPath == RenderingPath.Forward)
            myCam.depthTextureMode |= DepthTextureMode.Depth;

        int rtDepth = source.depth;

#if UNITY_2017_3_OR_NEWER
         if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
            rtDepth = 0;
#else
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D9 || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
        {
            rtDepth = 0;
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D9)
                volumeLighting = false;
        }
#endif
        ////////////// Temp RenderTexture ///////////////////
#if UNITY_2017_1_OR_NEWER
        RenderTextureDescriptor desc = new RenderTextureDescriptor(source.width, source.height, source.format, rtDepth);
        desc.msaaSamples = source.antiAliasing;

        if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
            desc.vrUsage = VRTextureUsage.TwoEyes;

        RenderTexture tempTexture = RenderTexture.GetTemporary(desc);
#else
        RenderTexture tempTexture = RenderTexture.GetTemporary(source.width, source.height, rtDepth, source.format, RenderTextureReadWrite.Default, source.antiAliasing);
#endif

        ////////////// Clouds ///////////////////////////

        if (EnviroSky.instance.useVolumeClouds)
        {
            if (!Application.isPlaying && EnviroSky.instance.showVolumeCloudsInEditor || Application.isPlaying)
            {
                if (blitMat == null)
                    blitMat = new Material(Shader.Find("Enviro/Blit"));

                StartFrame();

                if (subFrameTex == null || prevFrameTex == null || textureDimensionChanged)
                    CreateCloudsRenderTextures(source);

                if (!isAddionalCamera)
                    EnviroSky.instance.cloudsRenderTarget = subFrameTex;

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

                Graphics.Blit(source, tempTexture, blitMat);
                Graphics.Blit(subFrameTex, prevFrameTex);

                FinalizeFrame();
            }
            else
            {
                Graphics.Blit(source, tempTexture);
            }
        }
        else
        {
            Graphics.Blit(source, tempTexture);
        }

        //////////////// FOG
        float FdotC = myCam.transform.position.y - height;
        float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
        var sceneMode = RenderSettings.fogMode;
        var sceneDensity = RenderSettings.fogDensity;
        var sceneStart = RenderSettings.fogStartDistance;
        var sceneEnd = RenderSettings.fogEndDistance;
        Vector4 sceneParams;
        bool linear = (sceneMode == FogMode.Linear);
        float diff = linear ? sceneEnd - sceneStart : 0.0f;
        float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
        sceneParams.x = sceneDensity * 1.2011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
        sceneParams.y = sceneDensity * 1.4426950408f; // density / ln(2), used by Exp fog mode
        sceneParams.z = linear ? -invDiff : 0.0f;
        sceneParams.w = linear ? sceneEnd * invDiff : 0.0f;
        //////////////////

        if(!EnviroSky.instance.fogSettings.useSimpleFog)
        {
            Shader.SetGlobalVector("_FogNoiseVelocity", new Vector4(-EnviroSky.instance.Components.windZone.transform.forward.x * EnviroSky.instance.windStrenght * 10, -EnviroSky.instance.Components.windZone.transform.forward.z * EnviroSky.instance.windStrenght * 10) * EnviroSky.instance.fogSettings.noiseScale);
            Shader.SetGlobalVector("_FogNoiseData", new Vector4(EnviroSky.instance.fogSettings.noiseScale, EnviroSky.instance.fogSettings.noiseIntensity, EnviroSky.instance.fogSettings.noiseIntensityOffset));
            Shader.SetGlobalTexture("_FogNoiseTexture", _noiseTexture);
        }

        if (volumeLighting)
        {
            if (!Application.isPlaying && EnviroSky.instance.showVolumeLightingInEditor || Application.isPlaying)
            {

                if (volumeLightMat == null)
                    volumeLightMat = new Material(Shader.Find("Enviro/VolumeLight"));

                if (_volumeLightTexture == null)
                    ChangeResolution();

                //Dir volume
                if (dirVolumeLighting)
                {
                    Light _light = EnviroSky.instance.Components.DirectLight.GetComponent<Light>();
                    int pass = 4;

                    volumeLightMat.SetPass(pass);

                    if (EnviroSky.instance.volumeLightSettings.directLightNoise)
                        volumeLightMat.EnableKeyword("NOISE");
                    else
                        volumeLightMat.DisableKeyword("NOISE");

                    volumeLightMat.SetVector("_LightDir", new Vector4(_light.transform.forward.x, _light.transform.forward.y, _light.transform.forward.z, 1.0f / (_light.range * _light.range)));
                    volumeLightMat.SetVector("_LightColor", _light.color * _light.intensity);
                    volumeLightMat.SetFloat("_MaxRayLength", EnviroSky.instance.volumeLightSettings.MaxRayLength);

                    if (_light.cookie == null)
                    {
                        volumeLightMat.EnableKeyword("DIRECTIONAL");
                        volumeLightMat.DisableKeyword("DIRECTIONAL_COOKIE");
                    }
                    else
                    {
                        volumeLightMat.EnableKeyword("DIRECTIONAL_COOKIE");
                        volumeLightMat.DisableKeyword("DIRECTIONAL");
                        volumeLightMat.SetTexture("_LightTexture0", _light.cookie);
                    }

                    volumeLightMat.SetInt("_SampleCount", EnviroSky.instance.volumeLightSettings.SampleCount);
                    volumeLightMat.SetVector("_NoiseVelocity", new Vector4(-EnviroSky.instance.Components.windZone.transform.forward.x * EnviroSky.instance.windStrenght * 10, -EnviroSky.instance.Components.windZone.transform.forward.z * EnviroSky.instance.windStrenght *10) * EnviroSky.instance.volumeLightSettings.noiseScale);
                    volumeLightMat.SetVector("_NoiseData", new Vector4(EnviroSky.instance.volumeLightSettings.noiseScale, EnviroSky.instance.volumeLightSettings.noiseIntensity, EnviroSky.instance.volumeLightSettings.noiseIntensityOffset));
                    volumeLightMat.SetVector("_MieG", new Vector4(1 - (EnviroSky.instance.volumeLightSettings.Anistropy * EnviroSky.instance.volumeLightSettings.Anistropy), 1 + (EnviroSky.instance.volumeLightSettings.Anistropy * EnviroSky.instance.volumeLightSettings.Anistropy), 2 * EnviroSky.instance.volumeLightSettings.Anistropy, 1.0f / (4.0f * Mathf.PI)));
                    volumeLightMat.SetVector("_VolumetricLight", new Vector4(EnviroSky.instance.volumeLightSettings.ScatteringCoef.Evaluate(EnviroSky.instance.GameTime.solarTime), EnviroSky.instance.volumeLightSettings.ExtinctionCoef, _light.range, 1.0f));// - SkyboxExtinctionCoef));
                    volumeLightMat.SetTexture("_CameraDepthTexture", GetVolumeLightDepthBuffer());

                    //Texture tex = null;
                    if (_light.shadows != LightShadows.None)
                    {
                        volumeLightMat.EnableKeyword("SHADOWS_DEPTH");
                        Graphics.Blit(null, GetVolumeLightBuffer(), volumeLightMat, pass);
                    }
                    else
                    {
                        volumeLightMat.DisableKeyword("SHADOWS_DEPTH");
                        Graphics.Blit(null, GetVolumeLightBuffer(), volumeLightMat, pass);
                    }
                }

                if (Resolution == VolumtericResolution.Quarter)
                {
                    RenderTexture temp = RenderTexture.GetTemporary(_quarterDepthBuffer.width, _quarterDepthBuffer.height, 0, RenderTextureFormat.ARGBHalf);
                    temp.filterMode = FilterMode.Bilinear;
#if UNITY_2017_1_OR_NEWER
              if (myCam.stereoEnabled &&EnviroSky.instance.singlePassVR)
                  temp.vrUsage = VRTextureUsage.TwoEyes;
#endif
                    // horizontal bilateral blur at quarter res
                    Graphics.Blit(_quarterVolumeLightTexture, temp, _bilateralBlurMaterial, 8);
                    // vertical bilateral blur at quarter res
                    Graphics.Blit(temp, _quarterVolumeLightTexture, _bilateralBlurMaterial, 9);

                    // upscale to full res
                    Graphics.Blit(_quarterVolumeLightTexture, _volumeLightTexture, _bilateralBlurMaterial, 7);

                    RenderTexture.ReleaseTemporary(temp);
                }
                else if (Resolution == VolumtericResolution.Half)
                {
                    RenderTexture temp = RenderTexture.GetTemporary(_halfVolumeLightTexture.width, _halfVolumeLightTexture.height, 0, RenderTextureFormat.ARGBHalf);
                    temp.filterMode = FilterMode.Bilinear;
#if UNITY_2017_1_OR_NEWER
              if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
                  temp.vrUsage = VRTextureUsage.TwoEyes;
#endif
                    // horizontal bilateral blur at half res
                    Graphics.Blit(_halfVolumeLightTexture, temp, _bilateralBlurMaterial, 2);

                    // vertical bilateral blur at half res
                    Graphics.Blit(temp, _halfVolumeLightTexture, _bilateralBlurMaterial, 3);

                    // upscale to full res
                    Graphics.Blit(_halfVolumeLightTexture, _volumeLightTexture, _bilateralBlurMaterial, 5);
                    RenderTexture.ReleaseTemporary(temp);
                }
                else
                {
                    RenderTexture temp = RenderTexture.GetTemporary(_volumeLightTexture.width, _volumeLightTexture.height, 0, RenderTextureFormat.ARGBHalf);
                    temp.filterMode = FilterMode.Bilinear;
#if UNITY_2017_1_OR_NEWER
              if (myCam.stereoEnabled && EnviroSky.instance.singlePassVR)
                  temp.vrUsage = VRTextureUsage.TwoEyes;
#endif
                    // horizontal bilateral blur at full res
                    Graphics.Blit(_volumeLightTexture, temp, _bilateralBlurMaterial, 0);
                    // vertical bilateral blur at full res
                    Graphics.Blit(temp, _volumeLightTexture, _bilateralBlurMaterial, 1);
                    RenderTexture.ReleaseTemporary(temp);
                }
                Shader.EnableKeyword("ENVIROVOLUMELIGHT");
                Shader.SetGlobalTexture("_EnviroVolumeLightingTex", _volumeLightTexture);
            }
            else
            {
                Shader.DisableKeyword("ENVIROVOLUMELIGHT");
                Shader.SetGlobalTexture("_EnviroVolumeLightingTex", blackTexture);
            }
        }
        else
        {
            Shader.DisableKeyword("ENVIROVOLUMELIGHT");
            Shader.SetGlobalTexture("_EnviroVolumeLightingTex", blackTexture);
        }

        Shader.SetGlobalFloat("_EnviroVolumeDensity", EnviroSky.instance.globalVolumeLightIntensity);
        Shader.SetGlobalVector("_SceneFogParams", sceneParams);
        Shader.SetGlobalVector("_SceneFogMode", new Vector4((int)sceneMode, useRadialDistance ? 1 : 0, 0, 0));
        Shader.SetGlobalVector("_HeightParams", new Vector4(height, FdotC, paramK, heightDensity * 0.5f));
        Shader.SetGlobalVector("_DistanceParams", new Vector4(-Mathf.Max(startDistance, 0.0f), 0, 0, 0));

        if (dither != null && EnviroSkyMgr.instance.FogSettings.dithering)
        {
            fogMat.SetTexture("_DitheringTex", dither);
            fogMat.SetInt("_UseDithering", 1);
        }
        else
        {
            fogMat.SetInt("_UseDithering", 0);
        }

        fogMat.SetTexture("_MainTex", tempTexture);

        ////////// Finalalize

        if (EnviroSky.instance.useDistanceBlur)
        {
            if (!Application.isPlaying && EnviroSky.instance.showDistanceBlurInEditor || Application.isPlaying)
            {
                RenderTexture tempTexture2 = RenderTexture.GetTemporary(source.width, source.height, rtDepth, source.format, RenderTextureReadWrite.Default, source.antiAliasing);
#if UNITY_2017_1_OR_NEWER
              if (myCam.stereoEnabled &&EnviroSky.instance.singlePassVR)
                  tempTexture2.vrUsage = VRTextureUsage.TwoEyes;
#endif
                Graphics.Blit(tempTexture, tempTexture2, fogMat);
                PostProcess(tempTexture2, destination);
                RenderTexture.ReleaseTemporary(tempTexture);
                RenderTexture.ReleaseTemporary(tempTexture2);
            }
            else
            {

                Graphics.Blit(tempTexture, destination, fogMat);
                RenderTexture.ReleaseTemporary(tempTexture);
            }
        }
        else
        {
            Graphics.Blit(tempTexture, destination, fogMat);
            RenderTexture.ReleaseTemporary(tempTexture);
        }
    }

    private void PostProcess(RenderTexture source, RenderTexture destination)
    {
        var useRGBM = myCam.allowHDR;

        // source texture size
        var tw = source.width;
        var th = source.height;

        // halve the texture size for the low quality mode
        if (!EnviroSky.instance.distanceBlurSettings.highQuality)
        {
            tw /= 2;
            th /= 2;
        }

        if(postProcessMat == null)
           postProcessMat = new Material(Shader.Find("Hidden/EnviroDistanceBlur"));

        postProcessMat.SetTexture("_DistTex", distributionTexture);
        postProcessMat.SetFloat("_Distance", EnviroSky.instance.blurDistance);
        postProcessMat.SetFloat("_Radius", EnviroSky.instance.distanceBlurSettings.radius);

        // blur buffer format
        var rtFormat = useRGBM ?
            RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
       
       // determine the iteration count
       var logh = Mathf.Log(th, 2) + EnviroSky.instance.distanceBlurSettings.radius - 8;
        var logh_i = (int)logh;
        var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

        // update the shader properties
        var lthresh = thresholdLinear;
        postProcessMat.SetFloat("_Threshold", lthresh);

        var knee = lthresh * 0.5f + 1e-5f;
        var curve = new Vector3(lthresh - knee, knee * 2, 0.25f / knee);
        postProcessMat.SetVector("_Curve", curve);

        var pfo = !EnviroSky.instance.distanceBlurSettings.highQuality && EnviroSky.instance.distanceBlurSettings.antiFlicker;
        postProcessMat.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

        postProcessMat.SetFloat("_SampleScale", 0.5f + logh - logh_i);
        postProcessMat.SetFloat("_Intensity", EnviroSky.instance.blurIntensity);
        postProcessMat.SetFloat("_SkyBlurring", EnviroSky.instance.blurSkyIntensity);

        // prefilter pass
        var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);
        var pass = EnviroSky.instance.distanceBlurSettings.antiFlicker ? 1 : 0;
        Graphics.Blit(source, prefiltered, postProcessMat, pass);

        // construct a mip pyramid
        var last = prefiltered;
        for (var level = 0; level < iterations; level++)
        {
            _blurBuffer1[level] = RenderTexture.GetTemporary(
                last.width / 2, last.height / 2, 0, rtFormat
            );

            pass = (level == 0) ? (EnviroSky.instance.distanceBlurSettings.antiFlicker ? 3 : 2) : 4;
            Graphics.Blit(last, _blurBuffer1[level], postProcessMat, pass);

            last = _blurBuffer1[level];
        }

        // upsample and combine loop
        for (var level = iterations - 2; level >= 0; level--)
        {
            var basetex = _blurBuffer1[level];
            postProcessMat.SetTexture("_BaseTex", basetex);

            _blurBuffer2[level] = RenderTexture.GetTemporary(
                basetex.width, basetex.height, 0, rtFormat
            );

            pass = EnviroSky.instance.distanceBlurSettings.highQuality ? 6 : 5;
            Graphics.Blit(last, _blurBuffer2[level], postProcessMat, pass);
            last = _blurBuffer2[level];
        }

        // finish process
        postProcessMat.SetTexture("_BaseTex", source);
        pass = EnviroSky.instance.distanceBlurSettings.highQuality ? 8 : 7;
        Graphics.Blit(last, destination, postProcessMat, pass);

        // release the temporary buffers
        for (var i = 0; i < kMaxIterations; i++)
        {
            if (_blurBuffer1[i] != null)
                RenderTexture.ReleaseTemporary(_blurBuffer1[i]);

            if (_blurBuffer2[i] != null)
                RenderTexture.ReleaseTemporary(_blurBuffer2[i]);

            _blurBuffer1[i] = null;
            _blurBuffer2[i] = null;
        }

        RenderTexture.ReleaseTemporary(prefiltered);
    }

    private void UpdateMaterialParameters()
    {
        if (_bilateralBlurMaterial == null)
            _bilateralBlurMaterial = new Material(Shader.Find("Hidden/EnviroBilateralBlur"));

        _bilateralBlurMaterial.SetTexture("_HalfResDepthBuffer", _halfDepthBuffer);
        _bilateralBlurMaterial.SetTexture("_HalfResColor", _halfVolumeLightTexture);
        _bilateralBlurMaterial.SetTexture("_QuarterResDepthBuffer", _quarterDepthBuffer);
        _bilateralBlurMaterial.SetTexture("_QuarterResColor", _quarterVolumeLightTexture);

        Shader.SetGlobalTexture("_DitherTexture", _ditheringTexture);
        Shader.SetGlobalTexture("_NoiseTexture", _noiseTexture);
    }

    private void LoadNoise3dTexture()
    {
        // basic dds loader for 3d texture - !not very robust!

        TextAsset data = Resources.Load("NoiseVolume") as TextAsset;

        byte[] bytes = data.bytes;

        uint height = BitConverter.ToUInt32(data.bytes, 12);
        uint width = BitConverter.ToUInt32(data.bytes, 16);
        uint pitch = BitConverter.ToUInt32(data.bytes, 20);
        uint depth = BitConverter.ToUInt32(data.bytes, 24);
        uint formatFlags = BitConverter.ToUInt32(data.bytes, 20 * 4);
        //uint fourCC = BitConverter.ToUInt32(data.bytes, 21 * 4);
        uint bitdepth = BitConverter.ToUInt32(data.bytes, 22 * 4);
        if (bitdepth == 0)
            bitdepth = pitch / width * 8;


        // doesn't work with TextureFormat.Alpha8 for some reason
        _noiseTexture = new Texture3D((int)width, (int)height, (int)depth, TextureFormat.RGBA32, false);
        _noiseTexture.name = "3D Noise";

        Color[] c = new Color[width * height * depth];

        uint index = 128;
        if (data.bytes[21 * 4] == 'D' && data.bytes[21 * 4 + 1] == 'X' && data.bytes[21 * 4 + 2] == '1' && data.bytes[21 * 4 + 3] == '0' &&
            (formatFlags & 0x4) != 0)
        {
            uint format = BitConverter.ToUInt32(data.bytes, (int)index);
            if (format >= 60 && format <= 65)
                bitdepth = 8;
            else if (format >= 48 && format <= 52)
                bitdepth = 16;
            else if (format >= 27 && format <= 32)
                bitdepth = 32;

            //Debug.Log("DXGI format: " + format);
            // dx10 format, skip dx10 header
            //Debug.Log("DX10 format");
            index += 20;
        }

        uint byteDepth = bitdepth / 8;
        pitch = (width * bitdepth + 7) / 8;

        for (int d = 0; d < depth; ++d)
        {
            //index = 128;
            for (int h = 0; h < height; ++h)
            {
                for (int w = 0; w < width; ++w)
                {
                    float v = (bytes[index + w * byteDepth] / 255.0f);
                    c[w + h * width + d * width * height] = new Color(v, v, v, v);
                }

                index += pitch;
            }
        }

        _noiseTexture.SetPixels(c);
        _noiseTexture.Apply();
    }
    private void GenerateDitherTexture()
    {
        if (_ditheringTexture != null)
        {
            return;
        }

        int size = 8;
#if DITHER_4_4
        size = 4;
#endif
        _ditheringTexture = new Texture2D(size, size, TextureFormat.Alpha8, false, true);
        _ditheringTexture.filterMode = FilterMode.Point;
        Color32[] c = new Color32[size * size];

        byte b;
#if DITHER_4_4
        b = (byte)(0.0f / 16.0f * 255); c[0] = new Color32(b, b, b, b);
        b = (byte)(8.0f / 16.0f * 255); c[1] = new Color32(b, b, b, b);
        b = (byte)(2.0f / 16.0f * 255); c[2] = new Color32(b, b, b, b);
        b = (byte)(10.0f / 16.0f * 255); c[3] = new Color32(b, b, b, b);

        b = (byte)(12.0f / 16.0f * 255); c[4] = new Color32(b, b, b, b);
        b = (byte)(4.0f / 16.0f * 255); c[5] = new Color32(b, b, b, b);
        b = (byte)(14.0f / 16.0f * 255); c[6] = new Color32(b, b, b, b);
        b = (byte)(6.0f / 16.0f * 255); c[7] = new Color32(b, b, b, b);

        b = (byte)(3.0f / 16.0f * 255); c[8] = new Color32(b, b, b, b);
        b = (byte)(11.0f / 16.0f * 255); c[9] = new Color32(b, b, b, b);
        b = (byte)(1.0f / 16.0f * 255); c[10] = new Color32(b, b, b, b);
        b = (byte)(9.0f / 16.0f * 255); c[11] = new Color32(b, b, b, b);

        b = (byte)(15.0f / 16.0f * 255); c[12] = new Color32(b, b, b, b);
        b = (byte)(7.0f / 16.0f * 255); c[13] = new Color32(b, b, b, b);
        b = (byte)(13.0f / 16.0f * 255); c[14] = new Color32(b, b, b, b);
        b = (byte)(5.0f / 16.0f * 255); c[15] = new Color32(b, b, b, b);
#else
        int i = 0;
        b = (byte)(1.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(49.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(13.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(61.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(4.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(52.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(16.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(64.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(33.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(17.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(45.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(29.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(36.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(20.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(48.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(32.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(9.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(57.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(5.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(53.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(12.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(60.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(8.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(56.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(41.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(25.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(37.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(21.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(44.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(28.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(40.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(24.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(3.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(51.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(15.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(63.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(2.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(50.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(14.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(62.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(35.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(19.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(47.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(31.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(34.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(18.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(46.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(30.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(11.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(59.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(7.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(55.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(10.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(58.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(6.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(54.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

        b = (byte)(43.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(27.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(39.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(23.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(42.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(26.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(38.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
        b = (byte)(22.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
#endif

        _ditheringTexture.SetPixels32(c);
        _ditheringTexture.Apply();
    }
    private Mesh CreateSpotLightMesh()
    {
        // copy & pasted from other project, the geometry is too complex, should be simplified
        Mesh mesh = new Mesh();

        const int segmentCount = 16;
        Vector3[] vertices = new Vector3[2 + segmentCount * 3];
        Color32[] colors = new Color32[2 + segmentCount * 3];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, 1);

        float angle = 0;
        float step = Mathf.PI * 2.0f / segmentCount;
        float ratio = 0.9f;

        for (int i = 0; i < segmentCount; ++i)
        {
            vertices[i + 2] = new Vector3(-Mathf.Cos(angle) * ratio, Mathf.Sin(angle) * ratio, ratio);
            colors[i + 2] = new Color32(255, 255, 255, 255);
            vertices[i + 2 + segmentCount] = new Vector3(-Mathf.Cos(angle), Mathf.Sin(angle), 1);
            colors[i + 2 + segmentCount] = new Color32(255, 255, 255, 0);
            vertices[i + 2 + segmentCount * 2] = new Vector3(-Mathf.Cos(angle) * ratio, Mathf.Sin(angle) * ratio, 1);
            colors[i + 2 + segmentCount * 2] = new Color32(255, 255, 255, 255);
            angle += step;
        }

        mesh.vertices = vertices;
        mesh.colors32 = colors;

        int[] indices = new int[segmentCount * 3 * 2 + segmentCount * 6 * 2];
        int index = 0;

        for (int i = 2; i < segmentCount + 1; ++i)
        {
            indices[index++] = 0;
            indices[index++] = i;
            indices[index++] = i + 1;
        }

        indices[index++] = 0;
        indices[index++] = segmentCount + 1;
        indices[index++] = 2;

        for (int i = 2; i < segmentCount + 1; ++i)
        {
            indices[index++] = i;
            indices[index++] = i + segmentCount;
            indices[index++] = i + 1;

            indices[index++] = i + 1;
            indices[index++] = i + segmentCount;
            indices[index++] = i + segmentCount + 1;
        }

        indices[index++] = 2;
        indices[index++] = 1 + segmentCount;
        indices[index++] = 2 + segmentCount;

        indices[index++] = 2 + segmentCount;
        indices[index++] = 1 + segmentCount;
        indices[index++] = 1 + segmentCount + segmentCount;

        //------------
        for (int i = 2 + segmentCount; i < segmentCount + 1 + segmentCount; ++i)
        {
            indices[index++] = i;
            indices[index++] = i + segmentCount;
            indices[index++] = i + 1;

            indices[index++] = i + 1;
            indices[index++] = i + segmentCount;
            indices[index++] = i + segmentCount + 1;
        }

        indices[index++] = 2 + segmentCount;
        indices[index++] = 1 + segmentCount * 2;
        indices[index++] = 2 + segmentCount * 2;

        indices[index++] = 2 + segmentCount * 2;
        indices[index++] = 1 + segmentCount * 2;
        indices[index++] = 1 + segmentCount * 3;

        ////-------------------------------------
        for (int i = 2 + segmentCount * 2; i < segmentCount * 3 + 1; ++i)
        {
            indices[index++] = 1;
            indices[index++] = i + 1;
            indices[index++] = i;
        }

        indices[index++] = 1;
        indices[index++] = 2 + segmentCount * 2;
        indices[index++] = segmentCount * 3 + 1;

        mesh.triangles = indices;
        mesh.RecalculateBounds();

        return mesh;
    }

    ////////// Clouds Functions ///////////////
    private void SetReprojectionPixelSize(EnviroCloudSettings.ReprojectionPixelSize pSize)
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
    private void StartFrame()
    {
        textureDimensionChanged = UpdateFrameDimensions();

        switch (myCam.stereoActiveEye)
        {
            case Camera.MonoOrStereoscopicEye.Mono:
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
    private void FinalizeFrame()
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

        //Reset temporal reprojection size when zero. Needed if SkyManager starts deactivated
        if (EnviroSky.instance != null && reprojectionPixelSize == 0)
            SetReprojectionPixelSize(EnviroSky.instance.cloudsSettings.reprojectionPixelSize);

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
             int count = (int)(UnityEngine.Random.Range(0,1) * 1000.0f) % reproCount;
             frameNumbers[i] = frameNumbers[count];
             frameNumbers[count] = frame;
         }

        return frameNumbers;
    }
}