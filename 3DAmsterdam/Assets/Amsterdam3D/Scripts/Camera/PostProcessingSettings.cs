using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Amsterdam3D.Rendering
{
    public class PostProcessingSettings : MonoBehaviour
    {
        private bool antiAliasing = true;
        private bool postEffects = true;

        private UniversalAdditionalCameraData universalCameraData;
        [SerializeField]
        private Volume postProcessingVolume;

        private void Start()
        {
            universalCameraData = CameraModeChanger.Instance.ActiveCamera.GetComponent<UniversalAdditionalCameraData>();
        }

        /// <summary>
        /// Toggles antialiasing on or off.
        /// </summary>
        /// <param name="antiAliasingOn"></param>
        public void ToggleAA(bool antiAliasingOn)
        {
            antiAliasing = antiAliasingOn;

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


        public void TogglePostEffects(bool effectsOn)
        {
            postEffects = effectsOn;
            postProcessingVolume.enabled = postEffects;

            SetPostProcessing();
        }

        private void SetPostProcessing()
        {
            //Post processing can be disabled entirely if there are no AA or effects enabled
            universalCameraData.renderPostProcessing = (antiAliasing || postEffects);
        }
    }
}