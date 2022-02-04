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
                    if (Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam)
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
                    if (Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam)
                    {
                        geometryLineStringLocatorString = "\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[";
                    }
                    else geometryLineStringLocatorString = "\"geometry\": { \"type\": \"LineString\", \"coordinates\": [ [";
                }
                return geometryLineStringLocatorString;
            }
        }


        //private string geometryLineStringLocatorEndString = null;
        public string GeometryLineStringLocatorEndString
        {
            get
            {
                if (Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam)
                {
                    return "]]";
                }
                else return " ] ]";
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
                    if (Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam)
                    {
                        featureString = "{\"type\":\"Feature\"";
                    }
                    else featureString = "{ \"type\": \"Feature\"";
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
        }

        public bool FindFirstFeature()
        {
            featureStartIndex = geoJSONString.IndexOf(FeatureString, 0);
            featureEndIndex = geoJSONString.IndexOf("\n", featureStartIndex + FeatureString.Length);           
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
            int geometryEnd = geoJSONString.IndexOf(GeometryLineStringLocatorEndString, geometrystart) - 1;
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
            int propertyValueStartIndex = geoJSONString.IndexOf(propertyName, featureStartIndex, featureLength) + propertyName.Length + ((Config.activeConfiguration.sewerageApiType == SewerageApiType.Amsterdam) ? 3 : 4);
            if (propertyValueStartIndex == -1) 
            {
                return string.Empty;
            }
            return geoJSONString.Substring(propertyValueStartIndex, geoJSONString.IndexOf(',', propertyValueStartIndex) - propertyValueStartIndex - 1);
        }
    }
}