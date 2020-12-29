using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Amsterdam3D.Rendering;
using UnityEngine.UI;

namespace Amsterdam3D.Performance
{
    public class DetectPlatform : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern bool IsMobile();

        [SerializeField]
        private int mobileQualitySettingSlot = 0;

        [SerializeField]
        private PostProcessingSettings postProcessingSettings;

        [SerializeField]
        private Toggle mapToggle;

		private void Start()
		{
            #if !UNITY_EDITOR
                if(IsMobile())
                {
                    MobileOverrides();
			    }
            #endif
        }



        [ContextMenu("Set mobile quality settings")]
        private void MobileOverrides()
        {
            QualitySettings.SetQualityLevel(mobileQualitySettingSlot, true);
            postProcessingSettings.SetRenderScale(0.5f);

            mapToggle.isOn = false; //Disable map by default on mobile ( lots of textures = lots of memory )
        }
    }
}