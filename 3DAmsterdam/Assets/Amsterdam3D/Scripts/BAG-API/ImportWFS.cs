using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportWFS : MonoBehaviour
{
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
