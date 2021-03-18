using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering.Universal;

namespace Netherlands3D.Rendering
{
    public class DynamicShadowDistance : MonoBehaviour
    {
        [SerializeField]
        UniversalRenderPipelineAsset universalRenderPipelineAsset;

        [SerializeField]
        private float range = 6.5f;

        [SerializeField]
        private float maxShadowDistance = 4000;

        [SerializeField]
        private bool useFixedValue = false;

        private void OnEnable()
        {
            if (useFixedValue)
            {
                universalRenderPipelineAsset.shadowDistance = range;
                this.enabled = false;
            }
        }

        void Update()
        {
            universalRenderPipelineAsset.shadowDistance = Mathf.Min(this.transform.position.y * range, maxShadowDistance);
        }
    }
}