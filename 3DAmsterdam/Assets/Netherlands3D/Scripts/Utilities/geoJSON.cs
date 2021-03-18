using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Netherlands3D.Utilities
{
    public class GeoJSON
    {
        public string geoJSONString;
        private int featureStartIndex;
        private int featureEndIndex;
        private int featureLength;
        private string geometryPointLocatorString = "\"geometry\":{\"type\":\"Point\",\"coordinates\":[";
        private string geometryLineStringLocatorString = "\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[";
        private string geometryLineStringLocatorEndString = "]]";
        private List<double> doubleOutputList = new List<double>();

        private string featureString = "{\"type\":\"Feature\"";

        public GeoJSON(string geoJSON)
        {
            geoJSONString = geoJSON;
            featureStartIndex = -1;
            featureEndIndex = 0;
            featureLength = 0;
        }

        public bool FindFirstFeature()
        {
            featureStartIndex = geoJSONString.IndexOf(featureString, 0);
            featureEndIndex = geoJSONString.IndexOf(featureString, featureStartIndex + featureString.Length);
            featureLength = featureEndIndex - featureStartIndex;
            if (featureStartIndex > -1)
            {
                return true;
            }
            return false;
        }

        public bool GotoNextFeature()
        {
            if (featureStartIndex == -1)
            {
                return FindFirstFeature();
            }
            featureStartIndex = featureEndIndex;
            if (featureStartIndex == geoJSONString.Length)
            {
                return false;
            }
            if (featureStartIndex > -1)
            {
                featureEndIndex = geoJSONString.IndexOf(featureString, featureStartIndex + featureString.Length);
                if (featureEndIndex < 0)
                {
                    featureEndIndex = geoJSONString.Length;
                }
                featureLength = featureEndIndex - featureStartIndex;
                return true;
            }
            return false;
        }

        public bool PropertyValueStringEquals(string propertyName, string propertyValue)
        {

            int propertyStartIndex = geoJSONString.IndexOf(propertyName, featureStartIndex, featureLength) + 2;
            if (propertyStartIndex == -1)
            {
                return false;
            }
            int searchlength = featureEndIndex - propertyStartIndex - propertyName.Length;
            int valuestartIndex = geoJSONString.IndexOf(propertyValue, propertyStartIndex + propertyName.Length, searchlength);
            if (valuestartIndex > -1)
            {
                return true;
            }
            return false;
        }
        public double[] getGeometryPoint2DDouble()
        {
            int geometrystart = geoJSONString.IndexOf(geometryPointLocatorString, featureStartIndex, featureLength) + geometryPointLocatorString.Length;
            double[] output = new double[2];
            int nextstartpostion;
            output[0] = StringManipulation.ParseNextDouble(geoJSONString, ',', geometrystart, out nextstartpostion);
            output[1] = StringManipulation.ParseNextDouble(geoJSONString, ',', nextstartpostion, out nextstartpostion);
            return output;
        }

        public List<double> getGeometryLineString()
        {
            int geometrystart = geoJSONString.IndexOf(geometryLineStringLocatorString, featureStartIndex, featureLength) + geometryLineStringLocatorString.Length;
            int geometryEnd = geoJSONString.IndexOf(geometryLineStringLocatorEndString, geometrystart) - 1;
            doubleOutputList.Clear();
            int counter = 1;
            for (int i = geometrystart; i < geometryEnd; i++)
            {
                if (geoJSONString[i] == ',')
                {
                    counter++;
                }
            }
            if (doubleOutputList.Capacity < counter)
            {
                doubleOutputList.Capacity = counter;
            }
            int nextstartposition = geometrystart;
            counter = 0;
            while (nextstartposition < geometryEnd)
            {
                doubleOutputList.Add((float)StringManipulation.ParseNextDouble(geoJSONString, ',', nextstartposition, out nextstartposition));
                counter++;
            }

            return doubleOutputList;
        }

        public float getPropertyFloatValue(string propertyName)
        {
            int propertyStartIndex = geoJSONString.IndexOf(propertyName, featureStartIndex, featureLength) + propertyName.Length + 1;
            if (propertyStartIndex == -1)
            {
                return float.MaxValue;
            }
            int nextstartposition;
            return (float)StringManipulation.ParseNextDouble(geoJSONString, ',', propertyStartIndex, out nextstartposition);
        }
    }
}