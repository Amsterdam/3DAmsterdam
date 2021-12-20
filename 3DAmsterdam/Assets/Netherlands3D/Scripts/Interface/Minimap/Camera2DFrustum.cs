/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using Netherlands3D.Core;
using Netherlands3D.Cameras;
using UnityEngine;

namespace Netherlands3D.Interface.Minimap
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
			DrawCameraFrustumOnMap();
		}

		private void DrawCameraFrustumOnMap()
		{
			//Get corners
			var cameraCorners = Camera.main.GetWorldSpaceCorners();

			if (cameraCorners != null)
			{
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
}