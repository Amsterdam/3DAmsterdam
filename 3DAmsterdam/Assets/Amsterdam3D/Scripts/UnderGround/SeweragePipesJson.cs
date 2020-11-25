using UnityEngine;

[System.Serializable]
public class SeweragePipes
{
    public Features[] features;

    [System.Serializable]
    public class Features
    {
        public string id;
        public Geometry geometry;
        public Properties properties;

        [System.Serializable]
        public class Geometry
        {
            public string type = "";
            public string coordinates = "";

            public Vector3[] unity_coordinates;
        }
            
        [System.Serializable]
        public class Properties
        {
            public string vorm;
            public string objnr;
            public string ggwcode;
            public string ggwnaam;
            public string diameter;
            public string wijkcode;
            public string wijknaam;
            public string buurtcode;
            public string buurtnaam;
            public string materiaal;
            public string bob_eindpunt;
            public string leidingnaam;
            public string type_leiding;
            public string type_stelsel;
            public string bob_beginpunt;
            public string stadsdeelcode;
            public string stadsdeelnaam;
            public string type_fundering;
            public string lengte_in_meters;
            public string bemalingsgebied;
            public string leiding_toelichting;
        }
    }
}

