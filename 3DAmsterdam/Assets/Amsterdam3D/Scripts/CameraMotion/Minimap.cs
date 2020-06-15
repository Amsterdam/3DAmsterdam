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

        Vector3 bottomleft, topRight, centerPosition, startPosition;
        private float mapWidth, mapLength, mapImageWidth, mapImageLength, widthRatio, lengthRatio;

        private const float bottomLeftLat = 52.261480f;
        private const float bottomLeftLong = 4.727386f;
        private const float topRightLat = 52.454227f;
        private const float topRightLong = 5.108260f;

        private void Start()
        {
            startPosition = transform.localPosition;
            CalculateMapCoordinates();
            CalculateRatio();
        }

        private void CalculateRatio()
        {
            mapImageWidth = transform.parent.GetComponent<RectTransform>().sizeDelta.x;
            mapImageLength = transform.parent.GetComponent<RectTransform>().sizeDelta.y;

            widthRatio = mapImageWidth / mapWidth;
            lengthRatio = mapImageLength / mapLength;
        }

        private void CalculateMapCoordinates()
        {
            bottomleft = CoordConvert.WGS84toUnity(bottomLeftLong, bottomLeftLat);
            topRight = CoordConvert.WGS84toUnity(topRightLong, topRightLat);
            centerPosition = (bottomleft + topRight) / 2;

            mapWidth = topRight.x - bottomleft.x;
            mapLength = topRight.z - bottomleft.z;
        }

        void LateUpdate()
        {
            var posX = (Camera.main.transform.position.x - centerPosition.x) * widthRatio;
            var posY = (Camera.main.transform.position.z - centerPosition.z) * lengthRatio;

            transform.localPosition = new Vector3(posX, posY, 0);
            ChangeImageForwardToCameraForward();
        }

        private void ChangeImageForwardToCameraForward()
        {
            direction.z = Camera.main.transform.eulerAngles.y;
            transform.localEulerAngles = direction * -1.0f;
        }
    }
}