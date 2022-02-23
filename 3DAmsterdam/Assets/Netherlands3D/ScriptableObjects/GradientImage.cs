using Netherlands3D.Core.Colors;
using System;
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

		public void SetGradient(GradientContainer gradientContainer)
        {
            this.gradientContainer = gradientContainer;

            DrawTexture();
        }

        private void DrawTexture()
        {
            rawImage = this.GetComponent<RawImage>();
            rawImage.texture = gradientContainer.GetGradientTexture(textureWidthInPixels);
        }

		public void ClearContainerTexture()
        {
            gradientContainer.ClearTexture();
        }
    }
}