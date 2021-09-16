using ConvertCoordinates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    public class Coordinate : WorldPointFollower
    {
        [SerializeField]
        private Text coordinateText;

        public void DrawCoordinate(Vector3 coordinate, bool drawHeight = false)
        {
            var rd = CoordConvert.UnitytoRD(coordinate);
            AlignWithWorldPosition(coordinate);

            coordinateText.text = FormattableString.Invariant($"x {rd.x}\ny {rd.y}");
            if (drawHeight) coordinateText.text += FormattableString.Invariant($"\n{rd.z}");
        }
    }
}
