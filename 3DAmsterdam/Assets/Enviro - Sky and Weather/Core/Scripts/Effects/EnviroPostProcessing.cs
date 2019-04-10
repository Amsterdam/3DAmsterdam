using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class EnviroPostProcessing : MonoBehaviour {

    private Camera cam;

    /// LightShafts
    public enum SunShaftsResolution
    {
        Low = 0,
        Normal = 1,
        High = 2,
    }

    public enum ShaftsScreenBlendMode
    {
        Screen = 0,
        Add = 1,
    }

    [HideInInspector]
    public int radialBlurIterations = 2; 

    private Material sunShaftsMaterial;
    private Material moonShaftsMaterial;
    private Material simpleSunClearMaterial;
    private Material simpleMoonClearMaterial;
    /////////////////////////////////////////////////////////////////////////

    void OnEnable ()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        CreateMaterialsAndTextures();
    }

    void OnDisable()
    {
        CleanupMaterials();
    }

    void CreateMaterialsAndTextures()
    {
        sunShaftsMaterial = new Material(Shader.Find("Enviro/Effects/LightShafts"));
        moonShaftsMaterial = new Material(Shader.Find("Enviro/Effects/LightShafts"));
        simpleSunClearMaterial = new Material(Shader.Find("Enviro/Effects/ClearLightShafts"));
        simpleMoonClearMaterial = new Material(Shader.Find("Enviro/Effects/ClearLightShafts"));
    }

    void CleanupMaterials ()
    {
        if (sunShaftsMaterial != null)
            DestroyImmediate(sunShaftsMaterial);
        if (moonShaftsMaterial != null)
            DestroyImmediate(moonShaftsMaterial);
        if (simpleSunClearMaterial != null)
            DestroyImmediate(simpleSunClearMaterial);
        if (simpleMoonClearMaterial != null)
            DestroyImmediate(simpleMoonClearMaterial);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (EnviroSkyMgr.instance == null || !EnviroSkyMgr.instance.IsAvailable())
        {
            Graphics.Blit(source, destination);
            return;
        }

        // we actually need to check this every frame
        if (cam.actualRenderingPath == RenderingPath.Forward)
        {
            cam.depthTextureMode |= DepthTextureMode.Depth;
        }

        ////////////// Temp RenderTextures ///////////////////
#if UNITY_2017_1_OR_NEWER
        RenderTextureDescriptor desc = new RenderTextureDescriptor(source.width, source.height, source.format, source.depth);
        desc.msaaSamples = source.antiAliasing;

      //  if (cam.stereoEnabled && EnviroSky.instance.singlePassVR)
      //      desc.vrUsage = VRTextureUsage.TwoEyes;

        RenderTexture tempTexture = RenderTexture.GetTemporary(desc);
#else
        RenderTexture tempTexture = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format, RenderTextureReadWrite.Default, source.antiAliasing);
#endif

        if (sunShaftsMaterial == null)
        {
            CleanupMaterials();
            CreateMaterialsAndTextures();
        }

        /////////// Light Shafts ///////////////////////
        if (EnviroSkyMgr.instance.useSunShafts && EnviroSkyMgr.instance.useMoonShafts)
        {
            //Sun Shafts
            RenderLightShaft(source, tempTexture, sunShaftsMaterial, simpleSunClearMaterial, EnviroSkyMgr.instance.Components.Sun.transform, EnviroSkyMgr.instance.LightShaftsSettings.thresholdColorSun.Evaluate(EnviroSkyMgr.instance.Time.solarTime), EnviroSkyMgr.instance.LightShaftsSettings.lightShaftsColorSun.Evaluate(EnviroSkyMgr.instance.Time.solarTime));
            //Moon Shafts
            RenderLightShaft(tempTexture, destination, moonShaftsMaterial, simpleMoonClearMaterial, EnviroSkyMgr.instance.Components.Moon.transform, EnviroSkyMgr.instance.LightShaftsSettings.thresholdColorMoon.Evaluate(EnviroSkyMgr.instance.Time.solarTime), EnviroSkyMgr.instance.LightShaftsSettings.lightShaftsColorMoon.Evaluate(EnviroSkyMgr.instance.Time.solarTime));
        }
        else if (EnviroSkyMgr.instance.useSunShafts)
        {
            //Sun Shafts
            RenderLightShaft(source, destination, sunShaftsMaterial, simpleSunClearMaterial, EnviroSkyMgr.instance.Components.Sun.transform, EnviroSkyMgr.instance.LightShaftsSettings.thresholdColorSun.Evaluate(EnviroSkyMgr.instance.Time.solarTime), EnviroSkyMgr.instance.LightShaftsSettings.lightShaftsColorSun.Evaluate(EnviroSkyMgr.instance.Time.solarTime));
        }
        else if(EnviroSkyMgr.instance.useMoonShafts)
        {
            //Moon Shafts
            RenderLightShaft(source, destination, moonShaftsMaterial, simpleMoonClearMaterial, EnviroSkyMgr.instance.Components.Moon.transform, EnviroSkyMgr.instance.LightShaftsSettings.thresholdColorMoon.Evaluate(EnviroSkyMgr.instance.Time.solarTime), EnviroSkyMgr.instance.LightShaftsSettings.lightShaftsColorMoon.Evaluate(EnviroSkyMgr.instance.Time.solarTime));
        }
        else
        {
            Graphics.Blit(source, destination);
        }
        //////////////////////////////////////////////
        RenderTexture.ReleaseTemporary(tempTexture);
    }

    void RenderLightShaft(RenderTexture source, RenderTexture destination, Material mat, Material clearMat, Transform lightSource, Color treshold, Color clr)
    {
        int divider = 4;
        if (EnviroSkyMgr.instance.LightShaftsSettings.resolution == SunShaftsResolution.Normal)
            divider = 2;
        else if (EnviroSkyMgr.instance.LightShaftsSettings.resolution == SunShaftsResolution.High)
            divider = 1;

        Vector3 v = Vector3.one * 0.5f;

        if (lightSource)
            v = cam.WorldToViewportPoint(lightSource.position);
        else
            v = new Vector3(0.5f, 0.5f, 0.0f);

        int rtW = source.width / divider;
        int rtH = source.height / divider;

        RenderTexture lrColorB;
        RenderTexture lrDepthBuffer;

#if UNITY_5_6
        lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_0
       lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_1 || UNITY_2017_1_2 || UNITY_2017_1_3|| UNITY_2017_1_4
       lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default,1, RenderTextureMemoryless.None, source.vrUsage);
#endif

#if UNITY_2017_2_OR_NEWER
        lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

        // mask out everything except the skybox
        // we have 2 methods, one of which requires depth buffer support, the other one is just comparing images

        mat.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * EnviroSkyMgr.instance.LightShaftsSettings.blurRadius);
        mat.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, EnviroSkyMgr.instance.LightShaftsSettings.maxRadius));
        mat.SetVector("_SunThreshold", treshold);

        if (!EnviroSkyMgr.instance.LightShaftsSettings.useDepthTexture)
        {
            var format = EnviroSkyMgr.instance.Camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            RenderTexture tmpBuffer = RenderTexture.GetTemporary(source.width, source.height, 0, format);
            RenderTexture.active = tmpBuffer;
            GL.ClearWithSkybox(false, cam);

            mat.SetTexture("_Skybox", tmpBuffer);
            Graphics.Blit(source, lrDepthBuffer, mat, 3);
            RenderTexture.ReleaseTemporary(tmpBuffer);
        }
        else
        {
            Graphics.Blit(source, lrDepthBuffer, mat, 2);
        }

        // paint a small black small border to get rid of clamping problems
        if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Mono)
            DrawBorder(lrDepthBuffer, clearMat);

        // radial blur:

        radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);

        float ofs = EnviroSkyMgr.instance.LightShaftsSettings.blurRadius * (1.0f / 768.0f);

        mat.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        mat.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, EnviroSkyMgr.instance.LightShaftsSettings.maxRadius));

        for (int it2 = 0; it2 < radialBlurIterations; it2++)
        {
            // each iteration takes 2 * 6 samples
            // we update _BlurRadius each time to cheaply get a very smooth look

#if UNITY_5_6
            lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_0
       lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_1 || UNITY_2017_1_2 || UNITY_2017_1_3 || UNITY_2017_1_4
        lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

#if UNITY_2017_2_OR_NEWER
        lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

            Graphics.Blit(lrDepthBuffer, lrColorB, mat, 1);
            RenderTexture.ReleaseTemporary(lrDepthBuffer);
            ofs = EnviroSkyMgr.instance.LightShaftsSettings.blurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
            mat.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

#if UNITY_5_6
            lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_0
       lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_1 || UNITY_2017_1_2 || UNITY_2017_1_3 || UNITY_2017_1_4
       lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

#if UNITY_2017_2_OR_NEWER
        lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

            Graphics.Blit(lrColorB, lrDepthBuffer, mat, 1);
            RenderTexture.ReleaseTemporary(lrColorB);
            ofs = EnviroSkyMgr.instance.LightShaftsSettings.blurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
            mat.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        }

        // put together:

        if (v.z >= 0.0f)
            mat.SetVector("_SunColor", new Vector4(clr.r, clr.g, clr.b, clr.a) * EnviroSkyMgr.instance.LightShaftsSettings.intensity);
        else
            mat.SetVector("_SunColor", Vector4.zero); // no backprojection !

        mat.SetTexture("_ColorBuffer", lrDepthBuffer);

        Graphics.Blit(source, destination, mat, (EnviroSkyMgr.instance.LightShaftsSettings.screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);
        RenderTexture.ReleaseTemporary(lrDepthBuffer);
    }

    void DrawBorder(RenderTexture dest, Material material)
    {
        float x1;
        float x2;
        float y1;
        float y2;

        RenderTexture.active = dest;
        bool invertY = true; // source.texelSize.y < 0.0ff;
                             // Set up the simple Matrix
        GL.PushMatrix();
        GL.LoadOrtho();

        for (int i = 0; i < material.passCount; i++)
        {
            material.SetPass(i);

            float y1_; float y2_;
            if (invertY)
            {
                y1_ = 1.0f; y2_ = 0.0f;
            }
            else
            {
                y1_ = 0.0f; y2_ = 1.0f;
            }

            // left
            x1 = 0.0f;
            x2 = 0.0f + 1.0f / (dest.width * 1.0f);
            y1 = 0.0f;
            y2 = 1.0f;
            GL.Begin(GL.QUADS);

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // right
            x1 = 1.0f - 1.0f / (dest.width * 1.0f);
            x2 = 1.0f;
            y1 = 0.0f;
            y2 = 1.0f;

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // top
            x1 = 0.0f;
            x2 = 1.0f;
            y1 = 0.0f;
            y2 = 0.0f + 1.0f / (dest.height * 1.0f);

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            // bottom
            x1 = 0.0f;
            x2 = 1.0f;
            y1 = 1.0f - 1.0f / (dest.height * 1.0f);
            y2 = 1.0f;

            GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

            GL.End();
        }

        GL.PopMatrix();
    }
}
