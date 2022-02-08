using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class GradientImage : MonoBehaviour
    {
        [SerializeField]
        private GradientContainer gradientContainer;

        [SerializeField]
        private int textureWidthInPixels = 100;

        private RawImage rawImage;

        private void Awake()
        {
            rawImage = this.GetComponent<RawImage>();
        }

        void OnEnable()
        {
            rawImage.texture = gradientContainer.GetGradientTexture(textureWidthInPixels);
        }

        void OnDisable()
        {
            gradientContainer.ClearTexture();
        }
    }
}