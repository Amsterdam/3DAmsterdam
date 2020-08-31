namespace Amsterdam3D.Sharing
{
    [System.Serializable]
    public struct SerializableScene
    {
        public string appVersion;
        public string buildType;
        public string timeStamp;
        public string sunTimeStamp; //Time used to set the sun position
        public bool allowSceneEdit;

        public PostProcessing postProcessing;
        public Camera camera;

        public Annotation[] annotations;
        public CustomLayer[] customLayers;
        public FixedLayers fixedLayers;

        [System.Serializable]
        public struct Annotation
        {
            public bool active;
            public string bodyText;
            public Vector3 position;
        }

        [System.Serializable]
        public struct FixedLayers{ 
            public FixedLayer buildings;
            public FixedLayer trees;
            public FixedLayer ground;
        }

        [System.Serializable]
        public struct FixedLayer{
            public bool active;
            public Material[] materials;
            public int textureID;
        }

        [System.Serializable]
        public struct CustomLayer {
            public int layerID;
            public int type;
            public string token;
            public bool active;
            public string layerName;
            public string modelFilePath;
            public int modelFileSize;
            public string parsedType;

            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public Material[] materials;
        }

        [System.Serializable]
        public struct PostProcessing { }
        [System.Serializable]
        public struct Material
        {
            public int slotId;
            public string slotName;
            public float r;
            public float g;
            public float b;
            public float a;
        }
        [System.Serializable]
        public struct Vector3
        {
            public float x;
            public float y;
            public float z;
        }
        [System.Serializable]
        public struct Quaternion
        {
            public float x;
            public float y;
            public float z;
            public float w;
        }
        [System.Serializable]
        public struct Camera{
            public Vector3 position;
            public Quaternion rotation;
        }
    }
}