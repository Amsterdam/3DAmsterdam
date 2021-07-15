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

        public void DrawCoordinate(Vector3 coordinate)
        {
            var rd = CoordConvert.UnitytoRD(coordinate);
            AlignWithWorldPosition(coordinate);

            coordinateText.text = rd.x.ToString(CultureInfo.InvariantCulture) + "," + rd.y.ToString(CultureInfo.InvariantCulture) + "," + rd.z.ToString(CultureInfo.InvariantCulture);
        }
    }
}
