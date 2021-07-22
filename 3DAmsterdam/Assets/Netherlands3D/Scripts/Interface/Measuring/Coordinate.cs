using ConvertCoordinates;
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

            coordinateText.text = "x " + rd.x.ToString(CultureInfo.InvariantCulture) + "\ny " + rd.y.ToString(CultureInfo.InvariantCulture);
            if (drawHeight) coordinateText.text += "\n" + rd.z.ToString(CultureInfo.InvariantCulture);
        }
    }
}
