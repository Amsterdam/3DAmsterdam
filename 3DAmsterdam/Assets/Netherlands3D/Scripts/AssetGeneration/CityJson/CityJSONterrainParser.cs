#if UNITY_EDITOR

using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Netherlands3D.Utilities;

namespace Netherlands3D.AssetGeneration.CityJSON
{


public class CityJSONterrainParser : MonoBehaviour
{
        public string filepath = "E:/brondata/TerreinModel Kadaster/nieuwe versie/wachtrij/25fz1_04_2019_volledig.json";
        private string jsonString;
        private Vector3 vertexTransformScale = new Vector3(1,1,1);
        private Vector3RD vertexTransformTranslate = new Vector3RD(0, 0, 0);

        public List<Vector3RD> vertices = new List<Vector3RD>();
        public List<CityObject> cityobjects = new List<CityObject>();


        private string transformStartString = "\"transform\":{";
        private string transformScaleStartString = "\"scale\":";
        private string transformTranslateStartString = "\"translate\":";
        private string verticesStartString = "\"vertices\":[";

        private string cityObjectsStartString = "\"CityObjects\":{";
        private string boundariesStartString = "\"boundaries\":[";
        private string boundariesEndString = "]]]";
        // CityObjectTypeStrings
        private string waterbodyTypeString = "\"type\":\"WaterBody\"";
        private string plantcoverTypeString = "\"type\":\"PlantCover\"";
        private string genericCityObjectTypeString = "\"type\":\"GenericCityObject\"";
        private string bridgeTypeString = "\"type\":\"Bridge\"";
        private string roadsTypeString = "\"type\":\"Road\"";
            private List<string> roadsVoetpadFilter = new List<string> { "\"bgt_functie\":\"voetpad\"", "\"bgt_functie\":\"voetgangersgebied\"", "\"bgt_functie\":\"ruiterpad\"", "\"bgt_functie\":\"voetpad op trap\"" };
            private List<string> roadsFietspadFilter = new List<string> { "\"bgt_functie\":\"fietspad\"" };
            private List<string> roadsParkeervakFilter = new List<string> { "\"bgt_functie\":\"parkeervlak\"" };
            private List<string> roadsSpoorbaanFilter = new List<string> { "\"bgt_functie\":\"spoorbaan\"" };
            private List<string> roadsWoonerfFilter = new List<string> { "\"bgt_functie\":\"transitie\"", "\"bgt_functie\":\"woonerf\"" };
        private string landUseTypeString = "\"type\":\"LandUse\"";
            private List<string> landUseVoetpadFilter = new List<string> { "\"bgt_fysiekvoorkomen\":\"open verharding\"" };
            private List<string> landUseRoadsFilter = new List<string> { "\"bgt_fysiekvoorkomen\":\"gesloten verharding\"" };
            private List<string> landUseGroenFilter = new List<string> { "\"bgt_fysiekvoorkomen\":\"groenvoorziening\"" };
            private List<string> landUseErfFilter = new List<string> { "\"bgt_fysiekvoorkomen\":\"erf\"" };
        private char startBracket = '{';
        private char endBracket = '}';

    // Start is called before the first frame update
        void Start()
    {
            

        }

        public void Parse(string filepath)
        {
            jsonString = File.ReadAllText(filepath);
            readTransform();
            ReadVertices();
            ReadCityObjects();
            //find vertexTransform
            jsonString = null;
            
        }

        private void readTransform()
        {
            int transformstart = jsonString.IndexOf(transformStartString);
            if (transformstart == -1)
            {
                Debug.Log("geen transform in the file");
                return;
            }
            int scalestart = jsonString.IndexOf(transformScaleStartString, transformstart) + transformScaleStartString.Length;
            double value;
            value = StringManipulation.ParseNextDouble(jsonString, ',', scalestart, out scalestart);
            vertexTransformScale.x = (float)value;
            value = StringManipulation.ParseNextDouble(jsonString, ',', scalestart, out scalestart);
            vertexTransformScale.y = (float)value;
            value = StringManipulation.ParseNextDouble(jsonString, ',', scalestart, out scalestart);
            vertexTransformScale.z = (float)value;
            scalestart = jsonString.IndexOf(transformTranslateStartString, transformstart) + transformTranslateStartString.Length;
            value = StringManipulation.ParseNextDouble(jsonString, ',', scalestart, out scalestart);
            vertexTransformTranslate.x = value;
            value = StringManipulation.ParseNextDouble(jsonString, ',', scalestart, out scalestart);
            vertexTransformTranslate.y = value;
            value = StringManipulation.ParseNextDouble(jsonString, ',', scalestart, out scalestart);
            vertexTransformTranslate.z = value;
        }
        private void ReadVertices()
        {
            int verticesStart = jsonString.IndexOf(verticesStartString) + verticesStartString.Length;
            int verticesEnd = jsonString.IndexOf("]]",verticesStart);

            double value;
            while (verticesStart<verticesEnd)
            {
                Vector3RD vertex = new Vector3RD();
                value = StringManipulation.ParseNextDouble(jsonString, ',', verticesStart, out verticesStart);
                vertex.x = (value * vertexTransformScale.x) + vertexTransformTranslate.x;
                value = StringManipulation.ParseNextDouble(jsonString, ',', verticesStart, out verticesStart);
                vertex.y = (value * vertexTransformScale.y) + vertexTransformTranslate.y;
                value = StringManipulation.ParseNextDouble(jsonString, ',', verticesStart, out verticesStart);
                vertex.z = (value * vertexTransformScale.z) + vertexTransformTranslate.z;
                vertices.Add(vertex);
            }
        }
        private void ReadCityObjects()
        {
            // find the start of the CityObjects
            int cityobjectsstart = jsonString.IndexOf(cityObjectsStartString) + cityObjectsStartString.Length;
            int cityobjectsEnd = FindEndOfSet(cityobjectsstart-1);
            
            // find the start of the first cityObject
            int cityObjectStart = cityobjectsstart+1;
            // find the end of the cityobject
            int cityObjectEnd = cityObjectStart;

            
            while (cityObjectEnd < cityobjectsEnd - 1)
            {
                cityObjectStart = cityObjectEnd + 1;
                cityObjectEnd = FindEndOfSet(cityObjectStart);
                ReadCityObject(cityObjectStart, cityObjectEnd);
                cityObjectStart = cityObjectEnd + 1;
            }
    }

        private void ReadCityObject(int startIndex, int endIndex)
        {
            CityObject cityObject = new CityObject();
            int cityobjectLength = endIndex - startIndex+1;
            cityObject.type = getTerrainType(startIndex, cityobjectLength);
            if (cityObject.type == terrainType.anders)
            {
                cityObject = null;
                return;
            }
            cityObject.vertices = getCoordinateList(startIndex, cityobjectLength);
            cityObject.vertices.Reverse();
           // Debug.Log("vertcount:"+cityObject.vertices.Count);
            //read the geometry
            cityobjects.Add(cityObject);
        }

        private List<Vector3RD> getCoordinateList(int startIndex, int Length)
        {
            List<Vector3RD> coordinates = new List<Vector3RD>();
            int position = jsonString.IndexOf(boundariesStartString, startIndex) + boundariesStartString.Length;
            int endposition = jsonString.IndexOf(boundariesEndString, position);
            int value;
            while (position<endposition)
            {
                value = StringManipulation.ParseNextInt(jsonString, ',', position, out position);
                coordinates.Add(vertices[value]);
            }

            return coordinates;
        }

        private terrainType getTerrainType(int startIndex, int length)
        {
            if (jsonString.IndexOf(waterbodyTypeString, startIndex, length) > -1)
            {
                return terrainType.water;
            }
            if (jsonString.IndexOf(plantcoverTypeString, startIndex, length) > -1)
            {
                return terrainType.begroeid;
            }
            if (jsonString.IndexOf(genericCityObjectTypeString, startIndex, length) > -1)
            {
                return terrainType.constructies;
            }
            if (jsonString.IndexOf(bridgeTypeString, startIndex, length) > -1)
            {
                return terrainType.bruggen;
            }
            if (jsonString.IndexOf(roadsTypeString, startIndex, length) > -1)
            {
                if (FindFilterValue(ref roadsVoetpadFilter, startIndex, length))
                {
                    return terrainType.voetpad;
                }
                if (FindFilterValue(ref roadsFietspadFilter,startIndex,length))
                {
                    return terrainType.fietspad;
                }
                if (FindFilterValue(ref roadsParkeervakFilter, startIndex, length))
                {
                    return terrainType.parkeervakken;
                }
                if (FindFilterValue(ref roadsSpoorbaanFilter, startIndex, length))
                {
                    return terrainType.spoorbanen;
                }
                if (FindFilterValue(ref roadsWoonerfFilter, startIndex, length))
                {
                    return terrainType.woonerven;
                }
                return terrainType.wegen;
            }
            if (jsonString.IndexOf(landUseTypeString, startIndex, length) > -1)
            {
                if (FindFilterValue(ref landUseVoetpadFilter, startIndex, length))
                {
                    return terrainType.voetpad;
                }
                if (FindFilterValue(ref landUseRoadsFilter, startIndex, length))
                {
                    return terrainType.wegen;
                }
                if (FindFilterValue(ref landUseGroenFilter, startIndex, length))
                {
                    return terrainType.begroeid;
                }
                if (FindFilterValue(ref landUseErfFilter, startIndex, length))
                {
                    return terrainType.erven;
                }
                return terrainType.onbegroeid;
            }


            return terrainType.anders;
        }

        private bool FindFilterValue(ref List<string> filterlist, int startIndex,int length)
        {
            for (int i = 0; i < filterlist.Count; i++)
            {
                if (jsonString.IndexOf(filterlist[i], startIndex, length) > -1)
                {
                    return true;
                }
            }


            return false;
        }

        private int FindEndOfSet(int cityObjectStart)
        {
            int startBracketposition;
            int endBracketposition;

            int bracketcount = 1;
            int position = jsonString.IndexOf(startBracket, cityObjectStart) +1;

            while (bracketcount!=0)
            {
                startBracketposition = jsonString.IndexOf(startBracket, position);
                endBracketposition = jsonString.IndexOf(endBracket, position);
                if (startBracketposition < endBracketposition)
                {
                    position = startBracketposition+1;
                    bracketcount++;
                }
                else
                {
                    position = endBracketposition+1;
                    bracketcount--;
                }
            }
            return position;
        }
    
}
}
#endif