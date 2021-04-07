using Netherlands3D.Cameras;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class Pointer : WorldPointFollower
    {
        private Image pointerImage;

        [SerializeField]
        private float fadeOutTime = 0.3f;

        void Start()
        {
            GodViewCamera.focusingOnTargetPoint += Show;
            pointerImage = GetComponent<Image>();
            pointerImage.color = Color.clear;
        }

        /// <summary>
        /// Makes the 2D pointer appear at this world location with full opacity, and start fading out
        /// </summary>
        /// <param name="atPosition">World position to follow</param>
        public void Show(Vector3 atPosition)
        {
            AlignWithWorldPosition(atPosition);

            pointerImage.color = Color.white;

            StopAllCoroutines();
            StartCoroutine(FadeOutImage());
        }

        IEnumerator FadeOutImage()
        {
            var elapsedTime = 0.0f;
            while (elapsedTime < fadeOutTime)
            {
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
                var newColor = pointerImage.color;
                newColor.a = 1.0f - Mathf.Clamp01(elapsedTime / fadeOutTime);
                pointerImage.color = newColor;
            }
        }
    }
}