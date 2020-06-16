using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ConvertCoordinates;

namespace Amsterdam3D.Interface
{
    public class Minimap : MonoBehaviour
    {
        private Vector3 direction;

        Vector3 bottomLeft, topRight, centerPosition, startPosition;

        private RectTransform rectTransform;

        private const float bottomLeftLat = 52.261480f;
        private const float bottomLeftLong = 4.727386f;
        private const float topRightLat = 52.454227f;
        private const float topRightLong = 5.108260f;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            startPosition = transform.localPosition;
            CalculateMapCoordinates();
        }

        private void CalculateMapCoordinates()
        {
            bottomLeft = CoordConvert.WGS84toUnity(bottomLeftLong, bottomLeftLat);
            topRight = CoordConvert.WGS84toUnity(topRightLong, topRightLat);
        }

        void LateUpdate()
        {
            var posX = Mathf.InverseLerp(bottomLeft.x,topRight.x,Camera.main.transform.position.x);
            var posY = Mathf.InverseLerp(bottomLeft.z, topRight.z, Camera.main.transform.position.z);

            rectTransform.anchorMin = rectTransform.anchorMax = new Vector3(posX, posY, 0);

            ChangeImageForwardToCameraForward();
        }

        private void ChangeImageForwardToCameraForward()
        {
            direction.z = Camera.main.transform.eulerAngles.y;
            transform.localEulerAngles = direction * -1.0f;
        }
    }
}