namespace Amsterdam3D.Sharing
{
    [System.Serializable]
    public struct SerializableScene
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

            public Position position;
            public Rotation rotation;

            public Material[] materials;
        }

        [System.Serializable]
        public struct Weather{}
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
        public struct Position
        {
            public float x;
            public float y;
            public float z;
        }
        [System.Serializable]
        public struct Rotation
        {
            public float x;
            public float y;
            public float z;
            public float w;
        }
        [System.Serializable]
        public struct Camera{
            public Position position;
            public Rotation rotation;
        }
    }
}