using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public abstract class CityPolygon : MonoBehaviour
    {
        public abstract Vector3[] Polygon { get; }
    }
}
