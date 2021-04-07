using UnityEngine;

namespace Amsterdam3D.Sewerage
{
    /// <summary>
    /// Example server return: https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&typeName=rioolleidingen&bbox=123000%2c483000%2c124000%2c484000
    /// </summary>
    [System.Serializable]
    public class SewerLines
    {
        public Feature[] features;

        [System.Serializable]
        public class Feature
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
}