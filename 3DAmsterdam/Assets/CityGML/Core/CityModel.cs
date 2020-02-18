using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
namespace CityGML
{
    public enum CityGMLModule
    {
        Building,
        Bridge,
        CityFurniture,
        Generics,
        LandUse,
        Relief,
        Vegetation,
        WaterBody
    }


    public class CityModel
    {
        private XmlNamespaceManager nsmr;
        private XmlDocument doc;
        private string filepath;

        public CityModel(string Filepath, string filename)
        {
            filepath = Filepath;
            doc = new XmlDocument();
            doc.Load(Filepath+filename);
            nsmr = new XmlNamespaceManager(doc.NameTable);
            nsmr.AddNamespace("core", "http://www.opengis.net/citygml/2.0");
            nsmr.AddNamespace("bldg", "http://www.opengis.net/citygml/building/2.0");
            nsmr.AddNamespace("gml", "http://www.opengis.net/gml");
            nsmr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
            nsmr.AddNamespace("app", "http://www.opengis.net/citygml/appearance/2.0");
        }

        public List<BuildingFeature> GetBuildings(int LOD)
        {
            XmlNodeList buildingnodes = doc.SelectNodes("core:CityModel/core:cityObjectMember/bldg:Building", nsmr);
            
            
            List<BuildingFeature> output = new List<BuildingFeature>();

            BuildingFeature building;
            foreach (XmlNode node in buildingnodes)
            {
                building = new BuildingFeature(node, nsmr, filepath);
                if (building.isLOD(LOD))
                {
                    building.GetSurfaces(LOD,building.Appearancemembers);
                    output.Add(building);
                }
            }
            return output;
        }


        

    }
}