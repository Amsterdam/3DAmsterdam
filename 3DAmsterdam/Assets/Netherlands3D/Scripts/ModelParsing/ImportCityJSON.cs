using Netherlands3D.Coordinates;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Netherlands3D.CityJSON.Stream
{
    public class ImportCityJSON : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private int maxParseSteps = 30000;
        [SerializeField] private UnityEvent<GameObject> spawnImportedGameObject;

        //Transform values
        Vector3 scale = new Vector3();
        Vector3 translate = new Vector3();

        public void LoadCityJSONFromFile(string filepath)
        {
            var isCityJSON = IsCityJSON(filepath);

            if (isCityJSON)
            {
                Debug.Log("CityJSON, parsing..");
                StartCoroutine(StreamParseCityJSON(filepath));
            }
        }
#if UNITY_EDITOR
        [ContextMenu("Load CityJSON from disk")]
        public void EditorLoadCityJSON()
        {
            string path = EditorUtility.OpenFilePanel("Select CityJSON", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                LoadCityJSONFromFile(path);
            }
        }
#endif
        /// <summary>
        /// Streamread first part of json to check if it matches a CityJSON.
        /// Streamreading saves memory and time for parsing possible big CityJSON files.
        /// </summary>
        /// <param name="filepath">Path to json file</param>
        /// <returns>True if it is a CityJSON</returns>
        private bool IsCityJSON(string filepath)
        {
            Debug.Log("Checking if json is CityJSON");

            using (var fileStream = new FileStream(filepath, FileMode.Open))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer();

                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = jsonReader.Value.ToString();
                        if (propertyName == "type")
                        {
                            while (jsonReader.Read() && jsonReader.TokenType == JsonToken.String)
                            {
                                if (jsonReader.Value.ToString() == "CityJSON")
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private IEnumerator StreamParseCityJSON(string filepath)
        {
            Debug.Log("Parsing CityJSON");

            using (var fileStream = new FileStream(filepath, FileMode.Open))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                List<int> triangleIndices = new List<int>();
                List<Vector3> vertices = new List<Vector3>();

                // Advance the reader until we find properties we need and parse it
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.PropertyName)
                    {
                        yield return null;

                        var propertyName = jsonReader.Value.ToString();
                        Debug.Log(propertyName);
                        if (propertyName == "boundaries")
                        {
                            yield return ReadBoundaries(jsonReader, triangleIndices);
                        }
                        else if (propertyName == "vertices")
                        {
                            yield return ReadVertices(jsonReader, vertices);
                        }
                        else if (propertyName == "transform")
                        {
                            yield return ReadTransformValues(jsonReader);
                        }
                    }
                }

                //End of file
                SpawnNewGameObject(triangleIndices, vertices);
            }
        }

        private void SpawnNewGameObject(List<int> triangleIndices, List<Vector3> vertices)
        {
            Debug.Log($"--Mesh vertices: {vertices.Count}");
            Debug.Log($"--Mesh triangles: {triangleIndices.Count /3}");

            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();

            triangleIndices.Reverse();
            mesh.triangles = triangleIndices.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            var gameObject = new GameObject();
            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            gameObject.AddComponent<MeshRenderer>().material = new Material(material);

            /* //Move to RD offset
             * 
             * gameObject.transform.position = CoordinateConverter.RDtoUnity(
                translate.x,
                translate.y,
                translate.z
             );*/
            gameObject.transform.localScale = scale;

            spawnImportedGameObject.Invoke(gameObject);
        }

        private IEnumerator ReadTransformValues(JsonTextReader jsonReader)
        {
            scale = new Vector3();
            translate = new Vector3();

            //Read transform properties untill we move out of transform object
            while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndObject)
            {
                if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.ToString() == "scale")
                {
                    int index = 0;
                    while (jsonReader.Read() && jsonReader.TokenType == JsonToken.Float)
                    {
                        var number = Convert.ToDouble(jsonReader.Value);
                        scale[index] = (float)number;
                        index++;
                    }
                }
                else if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.ToString() == "translate")
                {
                    int index = 0;
                    while (jsonReader.Read() && jsonReader.TokenType == JsonToken.Float)
                    {
                        var number = Convert.ToDouble(jsonReader.Value);
                        translate[index] = (float)number;
                        index++;
                    }
                }
            }

            Debug.Log($"Translate: {translate.x},{translate.y},{translate.z}");
            yield return null;
        }

        private IEnumerator ReadVertices(JsonTextReader jsonReader, List<Vector3> vertices)
        {
            // Ensure the next token is the start of an array
            if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
            {
                //Read a vertex array
                while (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
                {
                    //Vertices
                    Vector3 vert = new Vector3();
                    int vertexPositionIndex = 0;
                    while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndArray)
                    {
                        var coordinate = Convert.ToSingle(jsonReader.Value);
                        vert[vertexPositionIndex] = coordinate;
                        vertexPositionIndex++;
                    }

                    vert = new Vector3(vert.x,vert.z,vert.y); //Swap Z and Y
                    vertices.Add(vert);

                    if ((vertices.Count % maxParseSteps) == 0) yield return null;

                }
                yield return null;
            }
        }

        private IEnumerator ReadBoundaries(JsonTextReader jsonReader, List<int> triangleIndices)
        {
            // Ensure the next token is the start of the boundaries array
            if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
            {
                // Read the outermost array elements to get to the nested triangle arrays
                // "boundaries":[[[2,1,0]],[[3,2,0]],[[6,5,4]]]
                while (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
                {
                    while (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
                    {
                        //Triangle indices
                        while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndArray)
                        {
                            if (jsonReader.TokenType == JsonToken.Integer)
                            {
                                var number = Convert.ToInt32(jsonReader.Value);
                                triangleIndices.Add(number);
                            }
                        }
                    }

                    yield return null;
                }

                yield return null;
            }
        }
    }
}