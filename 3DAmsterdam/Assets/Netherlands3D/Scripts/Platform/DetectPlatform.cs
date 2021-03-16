using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Netherlands3D.Rendering;
using UnityEngine.UI;
using LayerSystem;

namespace Netherlands3D.Performance
{
    public class DetectPlatform : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern bool IsMobile();

        [SerializeField]
        private int mobileQualitySettingSlot = 0;

        [SerializeField]
        private Rendering.RenderSettings postProcessingSettings;

        [SerializeField]
        private Toggle mapToggle;

        [SerializeField]
        private Layer[] lodTargetLayers;

        [SerializeField]
        private float mobileLodDistanceMultiplier = 0.5f;

		private void Start()
		{
            #if !UNITY_EDITOR
                Debug.unityLogger.logEnabled=false;
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

            //Reduce all LOD ranges
            foreach(Layer layer in lodTargetLayers){
                foreach(DataSet dataSet in layer.Datasets)
                {
                    dataSet.maximumDistance *= mobileLodDistanceMultiplier;
                    dataSet.maximumDistanceSquared = dataSet.maximumDistance * dataSet.maximumDistance;
                }
			}
        }
    }
}