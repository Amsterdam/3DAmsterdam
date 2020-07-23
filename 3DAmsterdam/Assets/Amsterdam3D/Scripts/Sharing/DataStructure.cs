using UnityEngine;

namespace Amsterdam3D.Sharing
{
    [System.Serializable]
    public struct DataStructure
    {
        public string appVersion;
        public string buildType;
        public string timeStamp;
        public string virtualTimeStamp;
        public Weather weather;
        public PostProcessing postProcessing;
        public Camera camera;

        public CustomLayer[] customLayers;
        public FixedLayers fixedLayers;

        public struct FixedLayers{
            public FixedLayer buildings;
            public FixedLayer trees;
            public FixedLayer ground;
        }

        public struct FixedLayer{
            public bool active;
            public Material[] materials;
            public int textureID;
        }

        public struct CustomLayer {
            public int layerID;
            public int type;
            public bool active;
            public string layerName;
            public string modelFilePath;
            public int modelFileSize;
            public string parsedType;

            public Position position;
            public Rotation rotation;

            public Material[] materials;
            public Position[] verts; //for free shape corners. optionaly later vert overrides
        }

        public struct Weather{}
        public struct PostProcessing { }
        public struct Material
        {
            public string slotName;
            public float r;
            public float g;
            public float b;
            public float a;
        }
        public struct Position
        {
            public float x;
            public float y;
            public float z;
        }
        public struct Rotation
        {
            public float x;
            public float y;
            public float z;
            public float w;
        }
        
        public struct Camera{
            public Position position;
            public Rotation rotation;
        }
    }
}

/* Example JSON
{
  "appVersion": "1.0.1",
  "buildType": "production",
  "timeStamp": "2020-07-23T18:25:43.511Z",
  "virtualTimeStamp": "2018-04-23T18:25:43.511Z",
  "weather": {},
  "postProcessing": {},
  "camera":{
	  "position": {
		"x": 1560.76648,
		"y": 305.494873,
		"z": -3748.26978
	  },
	  "rotation": {
		"x": -0.226790473,
		"y": 0.04884689,
		"z": -0.0113894995,
		"w": -0.9726512
	  }
  },
  "customLayers": [
    {
      "layerID": 0,
      "type": 0,
      "active": true,
      "layerName": "My custom model",
      "modelFilePath": "o4kdm5asuonk2o22oa/building",
	  "modelFileSize": 120000,
      "parsedType": "obj",
      "position": {
        "x": 1560.76648,
        "y": 305.494873,
        "z": -3748.26978
      },
      "rotation": {
        "x": -0.226790473,
        "y": 0.04884689,
        "z": -0.0113894995,
        "w": -0.9726512
      },
      "materials": [
        {
          "slotName": "CustomMaterial1",
          "r": 0.5,
          "g": 0.5,
          "b": 0.5,
          "a": 1
        },
        {
          "slotName": "CustomMaterial2",
          "r": 0.5,
          "g": 0.5,
          "b": 0.5,
          "a": 1
        },
        {
          "slotName": "CustomMaterial3",
          "r": 0.5,
          "g": 0.5,
          "b": 0.5,
          "a": 1
        }
      ]
    },
    {
      "layerID": 1,
      "type": 1,
      "active": true,
      "layerName": "Free shape",
      "position": {
        "x": 1560.76648,
        "y": 305.494873,
        "z": -3748.26978
      },
      "rotation": {
        "x": -0.226790473,
        "y": 0.04884689,
        "z": -0.0113894995,
        "w": -0.9726512
      },
      "materials": [
        {
          "r": 0,
          "g": 1,
          "b": 0,
          "a": 1
        }
      ],
      "verts": [
        {
          "x": 1560.76648,
          "y": 305.494873,
          "z": -3748.26978
        },
        {
          "x": 1560.76648,
          "y": 305.494873,
          "z": -3748.26978
        },
        {
          "x": 1560.76648,
          "y": 305.494873,
          "z": -3748.26978
        },
        {
          "x": 1560.76648,
          "y": 305.494873,
          "z": -3748.26978
        }
      ]
    }
  ],
  "fixedLayers": {
	"buildings":{
	  "active": true,
      "materials": [
        {
          "r": 0.5,
          "g": 0.5,
          "b": 0.5,
          "a": 1
        }
      ]
    },
	"trees":{
	  "active": true,
      "materials": [
        {
          "r": 0.5,
          "g": 0.5,
          "b": 0.5,
          "a": 1
        },
        {
          "r": 0.5,
          "g": 0.5,
          "b": 0.5,
          "a": 1
        }
      ]
    },
	"ground":{
	  "active": true,
      "textureID": 3
    }
  }
}
*/
