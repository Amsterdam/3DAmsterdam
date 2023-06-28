using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ImportCityJSON : MonoBehaviour
{
    [SerializeField] private int maxParseSteps = 30000;
    [SerializeField] private UnityEvent<GameObject> spawnImportedGameObject;

    //Transform values
    Vector3 scale = new Vector3();
    Vector3 translate = new Vector3();

    public void LoadCityJSONFromFile(string filepath)
    {
        if(IsCityJSON(filepath))
            StartCoroutine(Parse(filepath));
    }

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

            // Load the first few tokens to check if it is a CityJSON file
            var startTokens = new List<JsonToken>();
            var propertyNameTokens = new List<string>();

            // Read the first 5 tokens
            for (int i = 0; i < 5; i++)
            {
                if (!jsonReader.Read())
                {
                    break;
                }

                startTokens.Add(jsonReader.TokenType);

                if (jsonReader.TokenType == JsonToken.PropertyName)
                {
                    propertyNameTokens.Add(jsonReader.Value.ToString());
                }
            }

            // Check if it matches the CityJSON structure
            bool isCityJson = startTokens.Count >= 3 &&
                              startTokens[0] == JsonToken.StartObject &&
                              startTokens[1] == JsonToken.PropertyName &&
                              propertyNameTokens.Contains("type") &&
                              propertyNameTokens.Contains("version") &&
                              propertyNameTokens.Contains("CityObjects");

            return isCityJson;
        }
    }

    private IEnumerator Parse(string filepath)
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
        }
    }

    private IEnumerator ReadTransformValues(JsonTextReader jsonReader)
    {
        scale = new Vector3();
        translate = new Vector3();

        //Read transform properties untill we move out of transform object
        while (jsonReader.Read() && jsonReader.TokenType == JsonToken.EndObject)
        {
            if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value.ToString() == "scale")
            {
                int index = 0;
                while (jsonReader.Read() && jsonReader.TokenType == JsonToken.Float)
                {
                    var number = Convert.ToDouble(jsonReader.Value);
                    translate[index] = (float)number;
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
        yield return null;
    }

    private IEnumerator ReadVertices(JsonTextReader jsonReader, List<Vector3> vertices)
    {
        // Ensure the next token is the start of an array
        if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
        {
            while (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
            {
                //Vertices
                int vertexPositionIndex = 0;
                while (jsonReader.Read() && jsonReader.TokenType == JsonToken.Float)
                {
                    var number = Convert.ToInt32(jsonReader.Value);
                    Vector3 vert = new Vector3();
                    vertices.Add(vert);
                }
            }
            yield return null;
        }
    }

    private IEnumerator ReadBoundaries(JsonTextReader jsonReader, List<int> triangleIndices)
    {
        // Ensure the next token is the start of an array
        if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
        {
            var jsonSerializer = new JsonSerializer();

            // Read the outermost array elements to get to the nested triangle arrays
            // "boundaries":[[[2,1,0]],[[3,2,0]],[[6,5,4]]]
            if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
            {
                if (jsonReader.Read() && jsonReader.TokenType == JsonToken.StartArray)
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
