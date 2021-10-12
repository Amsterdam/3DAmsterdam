using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleJSON;
using Amsterdam3D.Sewerage;
using System;

namespace Netherlands3D.Utilities
{
    /// <summary>
    /// This class uses a custom 'no garbage collection' way of parsing JSON data
    /// </summary>
    public class GeoJSON
    {
        public string geoJSONString;
        private bool geoJSONUsesSpaces = true;
        private int featureStartIndex;
        private int featureEndIndex;
        private int featureLength;

        private string geometryPointLocatorString = null;
        public string GeometryPointLocatorString
        {
            get
            {       
                if(geometryPointLocatorString == null)
                {

                        if (geoJSONString.IndexOf("\"geometry\":{\"type\":\"Point\",\"coordinates\":[")>0)
                        {
                            geometryPointLocatorString = "\"geometry\":{\"type\":\"Point\",\"coordinates\":[";
                        }
 
                    else geometryPointLocatorString = "\"geometry\": { \"type\": \"Point\", \"coordinates\": [";
                }
                return geometryPointLocatorString;
            }
        }

        private string geometryLineStringLocatorString = null;
        public string GeometryLineStringLocatorString
        {
            get
            {
                if (geometryLineStringLocatorString == null)
                {
                    if (geoJSONString.IndexOf("\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[") > 0)
                    {
                        geometryLineStringLocatorString = "\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[";
                    }
                    else geometryLineStringLocatorString = "\"geometry\": { \"type\": \"LineString\", \"coordinates\": [ [";
                }
                return geometryLineStringLocatorString;
            }
        }

        private string geometryMultiPolygonStringLocatorString = null;
        public string GeometryMultiPolygonStringLocatorString
        {
            get
            {
                if (geometryMultiPolygonStringLocatorString == null)
                {
                    if (geoJSONString.IndexOf("\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[") > 0)
                    {
                        geometryMultiPolygonStringLocatorString = "\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[";
                    }
                    else geometryMultiPolygonStringLocatorString = "\"geometry\": { \"type\": \"MultiPolygon\", \"coordinates\": [ [";
                }
                return geometryMultiPolygonStringLocatorString;
            }
        }

        public string GeometryStringLocatorEndString
        {
            get
            {
                if (geoJSONString.Contains(" ] ]"))
                {
                    return " ] ]";
                }
                else
                {
                    return "]]";
                }
            }
        }

        private List<double> doubleOutputList = new List<double>();

        private string featureString = null;
        public string FeatureString
        {
            get
            {
                if (featureString == null)
                {
                    if (geoJSONString.IndexOf("\"type\":\"Feature\"") > 0)
                    {
                        featureString = "\"type\":\"Feature\"";
                    }
                    else featureString = " \"type\": \"Feature\"";
                }
                return featureString;
            }
        }

        public GeoJSON(string geoJSON)
        {
            geoJSONString = geoJSON;
            
            featureStartIndex = -1;
            featureEndIndex = 0;
            featureLength = 0;

            //check if geoJSON uses spaces
            if (geoJSONString.IndexOf("\"type\": \"FeatureCollection\"",1,50)==-1)
            {
                geoJSONUsesSpaces = false;
            }
        }

        public bool FindFirstFeature()
        {
            featureStartIndex = geoJSONString.IndexOf(FeatureString, 0);
            featureEndIndex = geoJSONString.IndexOf(FeatureString, featureStartIndex + FeatureString.Length);    
            if(featureEndIndex == -1)
            {
                featureEndIndex = geoJSONString.Length - 1;
            }

            featureLength = featureEndIndex - featureStartIndex;
            if (featureStartIndex > -1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the string pointer to the next feature item in the JSON string
        /// Remark: This custom JSON parsing is created because it doesn't create garbage collection
        /// </summary>
        /// <returns>Return false if no new feature element is found</returns>
        public bool GotoNextFeature()
        {
            if (featureStartIndex == -1)
            {
                return FindFirstFeature();
            }
            featureStartIndex = geoJSONString.IndexOf(FeatureString, featureEndIndex);

            if (featureStartIndex == geoJSONString.Length)
            {
                return false;
            }
            if (featureStartIndex > -1)
            {
                featureEndIndex = geoJSONString.IndexOf(FeatureString, featureStartIndex + FeatureString.Length);
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
            int geometrystart = geoJSONString.IndexOf(GeometryPointLocatorString, featureStartIndex, featureLength) + GeometryPointLocatorString.Length;
            double[] output = new double[2];
            int nextstartpostion;
            output[0] = StringManipulation.ParseNextDouble(geoJSONString, ',', geometrystart, out nextstartpostion);
            output[1] = StringManipulation.ParseNextDouble(geoJSONString, ',', nextstartpostion, out nextstartpostion);
            return output;
        }

        public List<double> getGeometryLineString()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryLineStringLocatorString, featureStartIndex, featureLength) + GeometryLineStringLocatorString.Length;
            int geometryEnd = geoJSONString.IndexOf(GeometryStringLocatorEndString, geometrystart) - 1;
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

        public List<double> getGeometryMultiPolygonString()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryMultiPolygonStringLocatorString, featureStartIndex, featureLength) + GeometryMultiPolygonStringLocatorString.Length;
            int geometryEnd = geoJSONString.IndexOf(GeometryStringLocatorEndString, geometrystart) - 1;

            if(geometryEnd == -1)
            {
                geometryEnd = featureLength;
            }

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
                return 0.0f;
            }
            int nextstartposition;
            return (float)StringManipulation.ParseNextDouble(geoJSONString, ',', propertyStartIndex, out nextstartposition);
        }

        public string getPropertyStringValue(string propertyName)
        {
            int propertyValueStartIndex = geoJSONString.IndexOf(propertyName, featureStartIndex, featureLength) + propertyName.Length;
            if (propertyValueStartIndex == -1) 
            {
                return string.Empty;
            }
            propertyValueStartIndex = geoJSONString.IndexOf('"', propertyValueStartIndex+1, 3);
            return geoJSONString.Substring(propertyValueStartIndex+1, geoJSONString.IndexOf('"', propertyValueStartIndex+1) - propertyValueStartIndex-1);
        }
    }
}