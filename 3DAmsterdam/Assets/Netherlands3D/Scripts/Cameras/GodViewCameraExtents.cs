using UnityEngine;
using ConvertCoordinates;
using UnityEngine.Rendering.Universal;

namespace Netherlands3D.Cameras
{
    /// <summary>
    /// bepaalt elke frame welk deel van amsterdam in beeld is en stelt in als public Brutile.Extent in WGS84 beschikbaar
    /// </summary>
    public class GodViewCameraExtents : MonoBehaviour, ICameraExtents
    {
        private enum Corners
        {
            TOP_LEFT,
            TOP_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_RIGHT
        }

        [SerializeField]
        private float maximumViewDistance = 5000;
        [SerializeField]
        private bool drawDebugLines = true;

        private Vector3[] corners;

        public Extent cameraExtent;

        void Start()
        {
            cameraExtent = CameraExtent();
        }

        void Update()
        {
            cameraExtent = CameraExtent();
        }

        private Extent CameraExtent()
        {
            // Determine what world coordinates are in the corners of our view
            corners = new Vector3[4];
            corners[0] = GetCornerPoint(Corners.TOP_LEFT);
            corners[1] = GetCornerPoint(Corners.TOP_RIGHT);
            corners[2] = GetCornerPoint(Corners.BOTTOM_RIGHT);
            corners[3] = GetCornerPoint(Corners.BOTTOM_LEFT);

            // Determine the min and max X- en Z-value of the visible coordinates
            var unityMax = new Vector3(-9999999, -9999999, -99999999);
            var unityMin = new Vector3(9999999, 9999999, 9999999);
            for (int i = 0; i < 4; i++)
            {
                unityMin.x = Mathf.Min(unityMin.x, corners[i].x);
                unityMin.z = Mathf.Min(unityMin.z, corners[i].z);
                unityMax.x = Mathf.Max(unityMax.x, corners[i].x);
                unityMax.z = Mathf.Max(unityMax.z, corners[i].z);
            }

            // Convert min and max to WGS84 coordinates
            var wGSMin = CoordConvert.UnitytoWGS84(unityMin);
            var wGSMax = CoordConvert.UnitytoWGS84(unityMax);

            // Area that should be loaded
            var extent = new Extent(wGSMin.lon, wGSMin.lat, wGSMax.lon, wGSMax.lat);
            return extent;
        }

        private Vector3 GetCornerPoint(Corners corner)
        {
            var screenPosition = new Vector2();

            switch (corner)
            {
                case Corners.TOP_LEFT:
                    screenPosition.x = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.pixelRect.xMin;
                    screenPosition.y = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.pixelRect.yMax;
                    break;
                case Corners.TOP_RIGHT:
                    screenPosition.x = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.pixelRect.xMax;
                    screenPosition.y = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.pixelRect.yMax;
                    break;
                case Corners.BOTTOM_LEFT:
                    screenPosition.x = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.pixelRect.xMin;
                    screenPosition.y = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.pixelRect.yMin;
                    break;
                case Corners.BOTTOM_RIGHT:
                    screenPosition.x = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.pixelRect.xMax;
                    screenPosition.y = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.pixelRect.yMin;
                    break;
                default:
                    break;
            }
            var output = new Vector3();

            var topLeftCornerStart = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position;
            var topLeftCornerFar = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 3010));

            // Calculate direction vector
            Vector3 direction = topLeftCornerStart - topLeftCornerFar;
            float factor; //factor waarmee de Richtingvector vermenigvuldigd moet worden om op het maaiveld te stoppen
            if (direction.y < 0) //wanneer de Richtingvector omhooggaat deze factor op 1 instellen
            {
                factor = 1;
            }
            else
            {
                factor = ((ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.localPosition.y - 40) / direction.y); //factor bepalen t.o.v. maaiveld (aanname maaiveld op 0 NAP = ca 40 Unityeenheden in Y-richting)
            }

            // Determine the X, Y, en Z location where the viewline ends
            output.x = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.localPosition.x - Mathf.Clamp((factor * direction.x), -1 * maximumViewDistance, maximumViewDistance);
            output.y = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.localPosition.y - Mathf.Clamp((factor * direction.y), -1 * maximumViewDistance, maximumViewDistance);
            output.z = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.localPosition.z - Mathf.Clamp((factor * direction.z), -1 * maximumViewDistance, maximumViewDistance);

            return output;
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugLines) return;

            Gizmos.color = Color.green;
            foreach (var corner in corners)
                Gizmos.DrawLine(ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position, corner);
        }

        public Extent GetExtent()
        {
            return cameraExtent;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}