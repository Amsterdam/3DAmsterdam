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
        private static int IdCounter = 0;

        public string Name { get; private set; }
        public CityObjectType Type;

        public int Lod { get; protected set; } = 3;
        public CitySurface[] Surfaces { get; private set; }
        private List<CityObject> cityChildren = new List<CityObject>();
        public CityObject[] CityChildren => cityChildren.ToArray();
        public CityObject[] CityParents { get; private set; } = new CityObject[0];

        public abstract CitySurface[] GetSurfaces();

        [SerializeField]
        private bool includeSemantics;

        protected virtual void Start()
        {
            IdCounter++;
            Name = "id-" + IdCounter;
            UpdateSurfaces();
            CityJSONFormatter.AddCityObejct(this);
        }

        public void UpdateSurfaces()
        {
            Surfaces = GetSurfaces();
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
                    parents[i] = CityParents[i].Name;
                }
                obj["parents"] = parents;
            }
            if (CityChildren.Length > 0)
            {
                var children = new JSONArray();
                for (int i = 0; i < CityChildren.Length; i++)
                {
                    children[i] = CityChildren[i].Name;
                }
                obj["children"] = children;
            }


            obj["geometry"] = new JSONArray();
            obj["geometry"].Add(GetGeometryNode());


            obj["attributes"] = GetAnnotations();
            return obj;
        }

        private JSONObject GetAnnotations()
        {
            var obj = new JSONObject();

            foreach (var ann in AnnotationState.AnnotationUIs)
            {
                if (Name == ann.ParentCityObject)
                {
                    var annotation = new JSONObject();
                    var point = new JSONArray();
                    point.Add("x", ann.ConnectionPointRD.x);
                    point.Add("y", ann.ConnectionPointRD.y);
                    point.Add("z", ann.ConnectionPointRD.z);
                    annotation.Add("location", point);
                    annotation.Add("text", ann.Text);
                    obj.Add("Annotation " + (ann.Id+1), annotation);
                }
            }
            return obj;
        }

        public virtual JSONObject GetGeometryNode()
        {
            var node = new JSONObject();
            node["type"] = "MultiSurface"; //todo support other types?
            node["lod"] = Lod;
            var boundaries = new JSONArray();
            for (int i = 0; i < Surfaces.Length; i++)
            {
                var surfaceArray = Surfaces[i].GetJSONPolygons();
                boundaries.Add(surfaceArray);
            }
            node["boundaries"] = boundaries;

            if (includeSemantics)
            {
                var semantics = GetSemantics();
                node["semantics"] = semantics;
            }
            return node;
        }

        private JSONNode GetSemantics()
        {
            var node = new JSONObject();
            var surfaceSemantics = new JSONArray();
            var indices = new JSONArray();
            for (int i = 0; i < Surfaces.Length; i++)
            {
                surfaceSemantics.Add(Surfaces[i].GetSemanticObject(Surfaces));
                indices.Add(i);
            }

            node["surfaces"] = surfaceSemantics;
            node["values"] = indices;

            return node;
        }
    }

}