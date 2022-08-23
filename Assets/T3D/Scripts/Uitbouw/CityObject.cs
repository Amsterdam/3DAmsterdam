using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.Assertions;
using SimpleJSON;
using System.Linq;
using System;


namespace T3D.Uitbouw
{
    public enum GeometryType
    {
        MultiPoint = 00,
        MultiLineString = 10,
        MultiSurface = 20,
        CompositeSurface = 21,
        Solid = 30,
        MultiSolid = 40,
        CompositeSolid = 41,
    }

    public enum CityObjectType
    {
        // 1000-2000: 1st level city objects
        Building = 1000,
        Bridge = 1010,
        CityObjectGroup = 1020,
        CityFurniture = 1030,
        GenericCityObject = 1040,
        LandUse = 1050,
        PlantCover = 1060,
        Railway = 1070,
        Road = 1080,
        SolitaryVegetationObject = 1090,
        TINRelief = 1100,
        TransportSquare = 1110,
        Tunnel = 1120,
        WaterBody = 1130,

        //2000-3000: 2nd level city objects. the middle numbers indicates the required parent. e.g 200x has to be a parent of 1000, 201x of 1010 etc.
        BuildingPart = 2000,
        BuildingInstallation = 2001,
        BridgePart = 2010,
        BridgeInstallation = 2011,
        BridgeConstructionElement = 2012,

        TunnelPart = 2120,
        TunnelInstallation = 2121
    }

    public abstract class CityObject : MonoBehaviour
    {
        private static string idPrefix = "NL.IMBAG.Pand.";
        public static string IdPrefix
        {
            get => idPrefix;
            set
            {
                idPrefix = value;
                print("set id prefix value to: " + value);
            }
        }
        private static int IdCounter = 0;

        public static readonly Dictionary<GeometryType, int> GeometryDepth = new Dictionary<GeometryType, int>{
            {GeometryType.MultiPoint, 0 }, //A "MultiPoint" has an array with the indices of the vertices; this array can be empty.
            {GeometryType.MultiLineString, 1 }, //A "MultiLineString" has an array of arrays, each containing the indices of a LineString
            {GeometryType.MultiSurface, 2 }, //A "MultiSurface", or a "CompositeSurface", has an array containing surfaces, each surface is modelled by an array of array, the first array being the exterior boundary of the surface, and the others the interior boundaries.
            {GeometryType.CompositeSurface, 2 },
            {GeometryType.Solid, 3 }, //A "Solid" has an array of shells, the first array being the exterior shell of the solid, and the others the interior shells. Each shell has an array of surfaces, modelled in the exact same way as a MultiSurface/CompositeSurface.
            {GeometryType.MultiSolid, 4 }, //A "MultiSolid", or a "CompositeSolid", has an array containing solids, each solid is modelled as above.
            {GeometryType.CompositeSolid, 4 },
        };

        public string Id { get; private set; }
        public CityObjectType Type;

        protected int activeLod = 3;
        public Dictionary<int, List<CitySurface[]>> Solids { get; protected set; }
        public Dictionary<int, CitySurface[]> Surfaces //todo: don't recaluclate every time
        {
            get
            {
                var surfaces = new Dictionary<int, CitySurface[]>();
                foreach (var solid in Solids)
                {
                    surfaces.Add(solid.Key, solid.Value[0]);
                }
                return surfaces;
            }
        }
        //public CitySurface[] Surfaces => Solids[activeLod][0];
        private List<CityObject> cityChildren = new List<CityObject>();
        public CityObject[] CityChildren => cityChildren.ToArray();
        public CityObject[] CityParents { get; private set; } = new CityObject[0];

        public abstract CitySurface[] GetSurfaces();

        [SerializeField]
        private bool includeSemantics;
        [SerializeField]
        private bool isMainBuilding;
        protected MeshFilter meshFilter;

        protected virtual void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            UpdateSurfaces();
            CityJSONFormatter.AddCityObejct(this);
            var bagId = ServiceLocator.GetService<T3DInit>().HTMLData.BagId;
            SetID(bagId);
        }

        public void SetID(string bagId)
        {
            if (isMainBuilding)
            {
                Id = IdPrefix + bagId;
                return;
            }

            Id = IdPrefix + bagId + "-" + IdCounter;
            IdCounter++;
        }

        public virtual void UpdateSurfaces()
        {
            Solids = new Dictionary<int, List<CitySurface[]>>();
            var outerShell = new List<CitySurface[]>() { GetSurfaces() };
            Solids.Add(activeLod, outerShell); //todo: fix this for different geometry types, currently this is based on a multisurface
            //Surfaces = GetSurfaces();
        }

        public void SetParents(CityObject[] newParents)
        {
            // remove this as the child of old parents
            foreach (var parent in CityParents)
            {
                parent.cityChildren.Remove(this);
            }

            // add this as child of new parents
            foreach (var parent in newParents)
            {
                Assert.IsTrue(IsValidParent(this, parent));
                parent.cityChildren.Add(this);
            }
            // set newparents for this
            CityParents = newParents;
        }

        public static bool IsValidParent(CityObject child, CityObject parent)
        {
            if (parent == null && ((int)child.Type < 2000))
                return true;

            if ((int)((int)child.Type / 10 - 200) == (int)((int)parent.Type / 10 - 100) || ((int)child.Type / 10) == ((int)parent.Type / 10))
                return true;


            //Debug.Log(child.Type + "\t" + parent, child.gameObject);
            return false;
        }

        public JSONObject GetJsonObject()
        {
            var obj = new JSONObject();
            obj["type"] = Type.ToString();
            if (CityParents.Length > 0)
            {
                var parents = new JSONArray();
                for (int i = 0; i < CityParents.Length; i++)
                {
                    Assert.IsTrue(IsValidParent(this, CityParents[i]));
                    parents[i] = CityParents[i].Id;
                }
                obj["parents"] = parents;
            }
            if (CityChildren.Length > 0)
            {
                var children = new JSONArray();
                for (int i = 0; i < CityChildren.Length; i++)
                {
                    children[i] = CityChildren[i].Id;
                }
                obj["children"] = children;
            }


            //obj["geometry"] = new JSONArray();
            var geometryArray = new JSONArray();
            foreach (var lod in Solids.Keys)
            {
                //obj["geometry"] = GetGeometryNode(lod);
                geometryArray.Add(GetGeometryNode(lod));
            }
            obj["geometry"] = geometryArray;

            obj["attributes"] = GetAttributes();
            return obj;
        }

        protected virtual JSONObject GetAttributes()
        {
            var obj = new JSONObject();
            obj.Add("annotations", GetAnnotationNode());
            return obj;
        }

        protected JSONObject GetAnnotationNode()
        {
            var annotationNode = new JSONObject();
            foreach (var ann in AnnotationState.AnnotationUIs)
            {
                if (Id == ann.ParentCityObject)
                {
                    var annotation = new JSONObject();
                    var point = new JSONArray();
                    point.Add("x", ann.ConnectionPointRD.x);
                    point.Add("y", ann.ConnectionPointRD.y);
                    point.Add("z", ann.ConnectionPointRD.z);
                    annotation.Add("location", point);
                    annotation.Add("text", ann.Text);
                    annotationNode.Add("Annotation " + (ann.Id + 1), annotation);
                }
            }
            return annotationNode;
        }

        public virtual JSONObject GetGeometryNode(int lod)
        {
            //var newGeometryArray = new JSONObject();
            //for (int i = 0; i < 1; i++) //multiple geometry objects represent different LODs
            //{
            var geometryObject = new JSONObject();
            geometryObject["type"] = "MultiSurface"; //todo support other types?
            geometryObject["lod"] = activeLod;
            var boundaries = new JSONArray();
            for (int j = 0; j < Surfaces[lod].Length; j++)
            {
                var surfaceArray = Surfaces[lod][j].GetJSONPolygons();
                boundaries.Add(surfaceArray);
            }
            geometryObject["boundaries"] = boundaries;

            if (includeSemantics)
            {
                var semantics = GetSemantics(activeLod);
                geometryObject["semantics"] = semantics;
            }
            //newGeometryArray.Add(geometryObject);
            //}
            return geometryObject;
        }

        protected virtual JSONNode GetSemantics(int lod)
        {
            var node = new JSONObject();
            var surfaceSemantics = new JSONArray();
            var indices = new JSONArray();
            for (int i = 0; i < Surfaces[lod].Length; i++)
            {
                surfaceSemantics.Add(Surfaces[lod][i].GetSemanticObject(Surfaces[lod]));
                indices.Add(i);
            }

            node["surfaces"] = surfaceSemantics;
            node["values"] = indices;

            return node;
        }
    }
}