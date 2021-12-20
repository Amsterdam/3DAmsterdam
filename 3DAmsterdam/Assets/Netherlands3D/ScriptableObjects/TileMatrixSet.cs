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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Interface.Minimap
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileMatrixSet", order = 0)]
    [System.Serializable]
    public class TileMatrixSet : ScriptableObject
    {
        public enum OriginAlignment
        {
            TopLeft,
            BottomLeft
        }
        public int TileSize = 256;
        public OriginAlignment minimapOriginAlignment = OriginAlignment.TopLeft;
        public Vector2RD Origin = new Vector2RD(-285401.92, 903401.92);
        public double PixelInMeters = 0.00028;
        public double ScaleDenominator = 12288000;
    }
}