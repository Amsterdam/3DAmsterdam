namespace Netherlands3D.Sharing
{
    [System.Serializable]
    public class SerializableScene
    {
        public string appVersion;
        public string buildType;
        public string timeStamp;
        public string sunTimeStamp; //Time used to set the sun position
        public bool allowSceneEdit;

        public PostProcessing postProcessing;
        public Camera camera;

        public CameraPoint[] cameraPoints;
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
            public string[] hiddenIds;
            public string[] interactedTiles;
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
            public static implicit operator Vector3(UnityEngine.Vector3 value)
            {
                return new Vector3 { x = value.x, y= value.y, z = value.z};
            }

            public static implicit operator UnityEngine.Vector3(Vector3 value) 
            {
                return new UnityEngine.Vector3(value.x, value.y, value.z);
            }

            public float x;
            public float y;
            public float z;
        }
        [System.Serializable]
        public struct Quaternion
        {
            public static implicit operator Quaternion(UnityEngine.Quaternion value) 
            {
                return new Quaternion { x = value.x, y = value.y, z = value.z, w = value.w };
            }

            public static implicit operator UnityEngine.Quaternion(Quaternion value) 
            {
                return new UnityEngine.Quaternion { x = value.x, y = value.y, z = value.z, w = value.w };
            }
            
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

        [System.Serializable]
        public struct CameraPoint
        {
            public Vector3 position;
            public Quaternion rotation;
            public string name;
            public bool active;
        }
    }
}