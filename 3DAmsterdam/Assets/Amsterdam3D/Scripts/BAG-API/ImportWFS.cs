using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportWFS : MonoBehaviour
{
    /*
        More info:
    
        For getting all filter codes, specified, greater, lesse than etc
        https://mapserver.org/ogc/filter_encoding.html#tests 
    
        Gets a value based on a specific parameter 
        https://map.data.amsterdam.nl/maps/monumenten?service=WFS&version=2.0.0&request=GetFeature&typeName=ms:monument_coordinaten&Filter=%3CFilter%3E%3CPropertyIsEqualTo%3E%3CPropertyName%3Eexternal_id%3C/PropertyName%3E%3CLiteral%3E55d301fb-6cec-4fd4-817a-042a107fd380%3C/Literal%3E%3C/PropertyIsEqualTo%3E%3C/Filter%3E&outputFormat=geojson
    */

    public class WFS
    {
        public class Rootobject
        {
            public string type;
            public string name;
            public Crs crs = new Crs();
            public Feature[] features;
        }

        public class Crs
        {
            public string type;
            public Properties properties = new Properties();
        }

        public class Properties
        {
            public string name;
        }

        public class Feature
        {
            public string type;
            public Properties1 properties = new Properties1();
            public Geometry geometry = new Geometry();
        }

        public class Properties1
        {
            public string id;
            public string external_id;
            public string monument_aanwijzingsdatum;
            public string in_onderzoek;
            public string monumentnummer;
            public string monumentnaam;
            public string display_naam;
            public string betreft_pand;
            public string heeft_als_grondslag_beperking;
            public string monumentstatus;
        }

        public class Geometry
        {
            public string type;
            public float[] coordinates;
        }
    }
}
