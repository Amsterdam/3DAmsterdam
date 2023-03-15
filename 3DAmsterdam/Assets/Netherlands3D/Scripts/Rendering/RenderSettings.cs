using Netherlands3D.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Netherlands3D.Rendering
{
    public class RenderSettings : MonoBehaviour
    {
        private bool antiAliasing = true;
        private bool postEffects = true;

        private Volume[] postProcessingVolumes;

        [SerializeField]
        private GameObject realtimeReflectionsObject;

        [SerializeField]
        private ScriptableRendererFeature aoRenderFeature;

        public static RenderSettings Instance;

        [Header("SSAO forward rendering settings")]
        [SerializeField] private bool switchLayerMaskOnSSAO = false;
        [SerializeField] private UniversalRendererData forwardRendererData;
        [SerializeField] private LayerMask aoEnabledLayerMask;
        [SerializeField] private LayerMask aoDisabledLayerMask;

        private void Awake()
		{
            Instance = this;

            postProcessingVolumes = FindObjectsOfType<Volume>();
        }

		/// <summary>
		/// Toggles antialiasing on or off.
		/// </summary>
		/// <param name="antiAliasingOn"></param>
		public void ToggleAA(bool antiAliasingOn)
        {
            antiAliasing = antiAliasingOn;

            UniversalAdditionalCameraData universalCameraData = CameraModeChanger.Instance.ActiveCamera.GetComponent<UniversalAdditionalCameraData>();
            
            //Enables or disables antialiasing on the camera, depending on what kind of camera we use.
            if (universalCameraData)
            {
                universalCameraData.antialiasing = (antiAliasing) ? AntialiasingMode.FastApproximateAntialiasing : AntialiasingMode.None;
            }
            else
            {
                QualitySettings.antiAliasing = (antiAliasing) ? 2 : 0;
            }

            SetPostProcessing();
        }

        public void ToggleReflections(bool reflectionsOn)
        {
            realtimeReflectionsObject.SetActive(reflectionsOn);
            EnviromentSettings.SetReflections(reflectionsOn);
        }


        public void TogglePostEffects(bool effectsOn)
        {
            postEffects = effectsOn;

            foreach(Volume volume in postProcessingVolumes)
                volume.enabled = effectsOn;

            SetPostProcessing();
        }

		public void DetectOptimalQualitySettings()
		{
			throw new NotImplementedException();
		}

		private void SetPostProcessing()
        {
            //Post processing can be disabled entirely if there are no AA or effects enabled
            UniversalAdditionalCameraData universalCameraData = CameraModeChanger.Instance.ActiveCamera.GetComponent<UniversalAdditionalCameraData>();
            universalCameraData.renderPostProcessing = (antiAliasing || postEffects);
        }

        public void SetShadowQuality(float shadowQuality)
        {
            //TODO: Probably need to swap render profiles here..Directly changing the res. as a value is not in URP yet.
            var urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            //urp.mainLightShadowmapResolution = calculatehere; //This is only a getter now. Its on the unity backlog to be able to be set as well
        }

        public void ToggleAO(bool enabled)
        {
            aoRenderFeature.SetActive(enabled);

            if (switchLayerMaskOnSSAO)
            {
                forwardRendererData.opaqueLayerMask = (enabled) ? aoEnabledLayerMask : aoDisabledLayerMask;
                forwardRendererData.SetDirty();
            }
        }

        public void SetRenderScale(float renderScale)
        {
            var urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            urp.renderScale = renderScale;
        }

        public void ChangeCameraFOV(float cameraFov)
        {
            CameraModeChanger.Instance.ActiveCamera.fieldOfView = cameraFov;
        }
    }
}