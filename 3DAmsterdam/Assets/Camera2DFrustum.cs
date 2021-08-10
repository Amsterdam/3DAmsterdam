using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using Netherlands3D.Interface.Minimap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netherlands3D.Interface
{
    public class Camera2DFrustum : MonoBehaviour
    {
        private UIQuad uiQuad;
        private WMTSMap wmtsMap;
        void Start()
        {
            uiQuad = GetComponent<UIQuad>();
            wmtsMap = GetComponentInParent<WMTSMap>();
        }

        void Update()
        {
            //Get corners
            var cameraCorners = CameraModeChanger.Instance.CurrentCameraExtends.GetWorldSpaceCorners();

            //Align quad with camera extent points
            uiQuad.QuadVertices[0] = wmtsMap.DeterminePositionOnMap(CoordConvert.UnitytoRD(cameraCorners[3]));
            uiQuad.QuadVertices[1] = wmtsMap.DeterminePositionOnMap(CoordConvert.UnitytoRD(cameraCorners[2]));
            uiQuad.QuadVertices[2] = wmtsMap.DeterminePositionOnMap(CoordConvert.UnitytoRD(cameraCorners[1]));
            uiQuad.QuadVertices[3] = wmtsMap.DeterminePositionOnMap(CoordConvert.UnitytoRD(cameraCorners[0]));

            //Make sure our graphic width/height is set to the max distance of our verts, so culling works properly
            uiQuad.Redraw();
        }
    }
}