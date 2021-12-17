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
        public enum GeoJSONGeometryType
        {
            Point,
            MultiPoint,
            LineString,
            MultiLineString,
            Polygon,
            MultiPolygon,
            GeometryCollection,
            Undefined
        }

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
                if (geometryPointLocatorString == null)
                {

                    if (!geoJSONUsesSpaces)
                    {
                        geometryPointLocatorString = "\"geometry\":{\"type\":\"Point\",\"coordinates\":[";
                    }

                    else geometryPointLocatorString = "\"geometry\": { \"type\": \"Point\", \"coordinates\": [";
                }
                return geometryPointLocatorString;
            }
        }

        private string geometryMultiPointLocatorString = null;
        public string GeometryMultiPointLocatorString
        {
            get
            {
                if (geometryMultiPointLocatorString == null)
                {

                    if (!geoJSONUsesSpaces)
                    {
                        geometryMultiPointLocatorString = "\"geometry\":{\"type\":\"MultiPoint\",\"coordinates\":[";
                    }

                    else geometryMultiPointLocatorString = "\"geometry\": { \"type\": \"MultiPoint\", \"coordinates\": [";
                }
                return geometryMultiPointLocatorString;
            }
        }

        private string geometryLineStringLocatorString = null;
        public string GeometryLineStringLocatorString
        {
            get
            {
                if (geometryLineStringLocatorString == null)
                {
                    if (!geoJSONUsesSpaces)
                    {
                        geometryLineStringLocatorString = "\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[";
                    }
                    else geometryLineStringLocatorString = "\"geometry\": { \"type\": \"LineString\", \"coordinates\": [ [";
                }
                return geometryLineStringLocatorString;
            }
        }

        private string geometryMultiLineStringLocatorString = null;
        public string GeometryMultiLineStringLocatorString
        {
            get
            {
                if (geometryMultiLineStringLocatorString == null)
                {
                    if (!geoJSONUsesSpaces)
                    {
                        geometryMultiLineStringLocatorString = "\"geometry\":{\"type\":\"MultiLineString\",\"coordinates\":[[";
                    }
                    else geometryMultiLineStringLocatorString = "\"geometry\": { \"type\": \"MultiLineString\", \"coordinates\": [ [";
                }
                return geometryMultiLineStringLocatorString;
            }
        }

        private string geometryPolygonStringLocatorString = null;
        public string GeometryPolygonStringLocatorString
        {
            get
            {
                if (geometryPolygonStringLocatorString == null)
                {
                    if (!geoJSONUsesSpaces)
                    {
                        geometryPolygonStringLocatorString = "\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[";
                    }
                    else geometryPolygonStringLocatorString = "\"geometry\": { \"type\": \"Polygon\", \"coordinates\": [ [";
                }
                return geometryPolygonStringLocatorString;
            }
        }

        private string geometryMultiPolygonStringLocatorString = null;
        public string GeometryMultiPolygonStringLocatorString
        {
            get
            {
                if (geometryMultiPolygonStringLocatorString == null)
                {
                    if (!geoJSONUsesSpaces)
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
                if (geoJSONUsesSpaces)
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
                    if (!geoJSONUsesSpaces)
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
            if (geoJSONString.IndexOf("\"type\": \"FeatureCollection\"", 1, 50) == -1)
            {
                geoJSONUsesSpaces = false;
            }
        }

        public bool FindFirstFeature()
        {
            featureStartIndex = geoJSONString.IndexOf(FeatureString, 0);
            featureEndIndex = geoJSONString.IndexOf(FeatureString, featureStartIndex + FeatureString.Length);
            if (featureEndIndex == -1)
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

        public double[] GetGeometryPoint2DDouble()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryPointLocatorString, featureStartIndex, featureLength) + GeometryPointLocatorString.Length;
            double[] output = new double[2];
            int nextstartposition;
            GeoJSONPoint point = GetPoint(geometrystart, out nextstartposition);
            output[0] = point.x;
            output[1] = point.y;
            return output;
        }

        public List<GeoJSONPoint> GetGeometryLineString()
        {
                int geometrystart = geoJSONString.IndexOf(GeometryLineStringLocatorString, featureStartIndex, featureLength) + GeometryLineStringLocatorString.Length;
                int nextStartPoint;
                return GetPointList(geometrystart, out nextStartPoint);
        }

        public List<double> GetGeometryMultiPolygonString()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryMultiPolygonStringLocatorString, featureStartIndex, featureLength) + GeometryMultiPolygonStringLocatorString.Length;
            int geometryEnd = geoJSONString.IndexOf(GeometryStringLocatorEndString, geometrystart) - 1;

            if (geometryEnd == -1)
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

        public float GetPropertyFloatValue(string propertyName)
        {
            int propertyStartIndex = geoJSONString.IndexOf(propertyName, featureStartIndex, featureLength) + propertyName.Length + 1;
            if (propertyStartIndex == -1)
            {
                return 0.0f;
            }
            int nextstartposition;
            return (float)StringManipulation.ParseNextDouble(geoJSONString, ',', propertyStartIndex, out nextstartposition);
        }

        public string GetPropertyStringValue(string propertyName)
        {
            int propertyValueStartIndex = geoJSONString.IndexOf(propertyName, featureStartIndex, featureLength) + propertyName.Length;
            if (propertyValueStartIndex == -1)
            {
                return string.Empty;
            }
            propertyValueStartIndex = geoJSONString.IndexOf('"', propertyValueStartIndex + 1, 3);
            return geoJSONString.Substring(propertyValueStartIndex + 1, geoJSONString.IndexOf('"', propertyValueStartIndex + 1) - propertyValueStartIndex - 1);
        }

        /// <summary>
        /// get the geometryType of the current Feature
        /// </summary>
        /// <returns>GeoJSONGeometryType</returns>
        public GeoJSONGeometryType GetGeometryType()
        {

            int geometrystart = geoJSONString.IndexOf(GeometryPointLocatorString, featureStartIndex, featureLength);
            if (geometrystart > -1)
            {
                return GeoJSONGeometryType.Point;
            }
            geometrystart = geoJSONString.IndexOf(GeometryMultiPointLocatorString, featureStartIndex, featureLength);
            if (geometrystart > -1)
            {
                return GeoJSONGeometryType.MultiPoint;
            }
            geometrystart = geoJSONString.IndexOf(GeometryLineStringLocatorString, featureStartIndex, featureLength);
            if (geometrystart > -1)
            {
                return GeoJSONGeometryType.LineString;
            }
            geometrystart = geoJSONString.IndexOf(GeometryMultiLineStringLocatorString, featureStartIndex, featureLength);

            if (geometrystart > -1)
            {
                return GeoJSONGeometryType.MultiLineString;
            }
            geometrystart = geoJSONString.IndexOf(GeometryPolygonStringLocatorString, featureStartIndex, featureLength);
            if (geometrystart > -1)
            {
                return GeoJSONGeometryType.Polygon;
            }

            geometrystart = geoJSONString.IndexOf(GeometryMultiPolygonStringLocatorString, featureStartIndex, featureLength);
            if (geometrystart > -1)
            {
                return GeoJSONGeometryType.MultiPolygon;
            }

            // if nothingfound
            return GeoJSONGeometryType.Undefined;
        }

        public string GetFeatureString()
        {
            return geoJSONString.Substring(featureStartIndex - 1, featureLength);
        }

        private GeoJSONPoint GetPoint(int startIndex, out int endposition)
        {
            GeoJSONPoint point = new GeoJSONPoint();
            int nextstartposition;
            point.x = StringManipulation.ParseNextDouble(geoJSONString, ',', startIndex, out nextstartposition);
            point.y = StringManipulation.ParseNextDouble(geoJSONString, ',', nextstartposition, out nextstartposition);
            endposition = geoJSONString.IndexOf(']',startIndex);
            return point;

        }
        public List<GeoJSONPoint> GetMultiPoint()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryMultiPointLocatorString, featureStartIndex, featureLength) + GeometryMultiPointLocatorString.Length;
            int nextStartPoint;
            return GetPointList(geometrystart, out nextStartPoint);
        }
        public List<GeoJSONPoint> GetLine()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryMultiPointLocatorString, featureStartIndex, featureLength) + GeometryMultiPointLocatorString.Length;
            int nextStartPoint;
            return GetPointList(geometrystart, out nextStartPoint);
        }

        public List<List<GeoJSONPoint>> GetMultiLine()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryMultiLineStringLocatorString, featureStartIndex, featureLength) + GeometryMultiLineStringLocatorString.Length;
            int nextStartPoint;
            return GetMultiLine(geometrystart, out nextStartPoint);
        }
        public List<List<GeoJSONPoint>> GetPolygon()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryPolygonStringLocatorString, featureStartIndex, featureLength) + GeometryPolygonStringLocatorString.Length;
            int nextStartPoint;
            return GetMultiLine(geometrystart, out nextStartPoint);
        }

        public List<List<List<GeoJSONPoint>>> GetMultiPolygon()
        {
            int geometrystart = geoJSONString.IndexOf(GeometryPolygonStringLocatorString, featureStartIndex, featureLength) + GeometryPolygonStringLocatorString.Length;
            int nextStartPoint;
            return GetMultiPolygon(geometrystart, out nextStartPoint);
        }

        private List<List<List<GeoJSONPoint>>> GetMultiPolygon(int startIndex, out int endposition)
        {       
            List<List<List<GeoJSONPoint>>> polygons = new List<List<List<GeoJSONPoint>>>();
            bool keepGoing = true;

            int nextstartposition = startIndex;
            int pointEndPosition;
            int commaPosition;
            int bracketPosition;
            while (keepGoing)
            {
                polygons.Add(GetMultiLine(nextstartposition, out pointEndPosition));
                //pointEndposition = position of ] at end of point

                //findthe position of the next comma
                commaPosition = geoJSONString.IndexOf(',', pointEndPosition);
                if (commaPosition < 1)
                {
                    commaPosition = int.MaxValue;
                }
                // find the position of the next closing bracket
                bracketPosition = geoJSONString.IndexOf(']', pointEndPosition + 1);
                if (bracketPosition == -1)
                {
                    bracketPosition = int.MaxValue - 1;
                }

                if (bracketPosition < commaPosition)
                {
                    // if a closing-bracket appears before a comma, we have read all the points in the collection
                    keepGoing = false;
                    // the endposition will be the position of the endbracket after the endbracket of the point
                    endposition = geoJSONString.IndexOf(']', pointEndPosition + 1);
                    return polygons;
                }

                // there is another point, so we set the startpostion to the next opening-bracket
                nextstartposition = geoJSONString.IndexOf('[', pointEndPosition);
            }

            // the next two lines will never be reached, but have to be there just in case.
            endposition = nextstartposition + 1;
            return polygons;
        }

        private List<List<GeoJSONPoint>> GetMultiLine(int startIndex, out int endposition)
        {
            // reading [[[x,y],[x,y],[x,y]],[[x,y],[x,y],[x,y]]]
            List<List<GeoJSONPoint>> lines = new List<List<GeoJSONPoint>>();
            bool keepGoing = true;

            int nextstartposition = startIndex;
            int pointEndPosition;
            int commaPosition;
            int bracketPosition;
            while (keepGoing)
            {
                lines.Add(GetPointList(nextstartposition, out pointEndPosition));
                //pointEndposition = position of ] at end of point

                //findthe position of the next comma
                commaPosition = geoJSONString.IndexOf(',', pointEndPosition);
                if (commaPosition < 1)
                {
                    commaPosition = int.MaxValue;
                }
                // find the position of the next closing bracket
                bracketPosition = geoJSONString.IndexOf(']', pointEndPosition + 1);
                if (bracketPosition == -1)
                {
                    bracketPosition = int.MaxValue - 1;
                }

                if (bracketPosition < commaPosition)
                {
                    // if a closing-bracket appears before a comma, we have read all the points in the collection
                    keepGoing = false;
                    // the endposition will be the position of the endbracket after the endbracket of the point
                    endposition = geoJSONString.IndexOf(']', pointEndPosition + 1);
                    return lines;
                }

                // there is another point, so we set the startpostion to the next opening-bracket
                nextstartposition = geoJSONString.IndexOf('[', pointEndPosition);
            }

            // the next two lines will never be reached, but have to be there just in case.
            endposition = nextstartposition + 1;
            return lines;
        }

        private List<GeoJSONPoint> GetPointList(int startIndex, out int endposition)
        {
            // reading [[x,y],[x,y],[x,y]]
            List<GeoJSONPoint> points = new List<GeoJSONPoint>();
            bool keepGoing = true;

            int nextstartposition = startIndex;
            int pointEndPosition;
            int commaPosition;
            int bracketPosition;
            while (keepGoing)
            {
                points.Add(GetPoint(nextstartposition, out pointEndPosition));
                    //pointEndposition = position of ] at end of point
                
                //findthe position of the next comma
                commaPosition = geoJSONString.IndexOf(',', pointEndPosition);
                if (commaPosition<1)
                {
                    commaPosition = int.MaxValue;
                }
                // find the position of the next closing bracket
                bracketPosition = geoJSONString.IndexOf(']', pointEndPosition+1);
                if (bracketPosition ==-1)
                {
                    bracketPosition = int.MaxValue-1;
                }

                if (bracketPosition<commaPosition)
                {
                    // if a closing-bracket appears before a comma, we have read all the points in the collection
                    keepGoing = false;
                    // the endposition will be the position of the endbracket after the endbracket of the point
                    endposition = geoJSONString.IndexOf(']', pointEndPosition+1);
                    return points;
                }

                // there is another point, so we set the startpostion to the next opening-bracket
                nextstartposition = geoJSONString.IndexOf('[',pointEndPosition);
            }

            // the next two lines will never be reached, but have to be there just in case.
            endposition = nextstartposition+1;
            return points;

        }

        public Dictionary<string, object> GetProperties()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            string searchString = @"""properties"": {";
            int startread = geoJSONString.IndexOf(searchString, featureStartIndex) + searchString.Length ;
            int lastStartRead = 0;
            while (startread > lastStartRead)
            {
                lastStartRead = startread;

                int endprop = geoJSONString.IndexOf(",", startread);
                int endproperties = geoJSONString.IndexOf("}", startread);
                var sub = geoJSONString.Substring(startread, endprop - startread);

                var kv = sub.Split(':');
                var key = kv[0].Replace("\"", "").Trim();
                var val = kv[1].Replace("\"", "").Replace("}", "").Trim();
                
                properties.Add(key, val);

                if (endprop > endproperties)
                {
                    break;
                }

                startread = endprop+1;
            }

            return properties;
        }
    }
    public struct GeoJSONPoint
    {
        public double x;
        public double y;

        public GeoJSONPoint(double X, double Y)
        {
            x = X;
            y = Y;
        }
    }
}