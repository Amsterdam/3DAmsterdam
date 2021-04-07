using UnityEngine;

namespace Amsterdam3D.Sewerage
{
    /// <summary>
    /// Example server return: https://api.data.amsterdam.nl/v1/wfs/rioolnetwerk/?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetFeature&outputFormat=geojson&typeName=rioolknopen&bbox=123000%2c483000%2c124000%2c484000
    /// </summary>
    [System.Serializable]
    public class SewerManholes
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
                public float[] coordinates;
            }

            [System.Serializable]
            public class Properties
            {
                public string objnr;
                public string ggwcode;
                public string ggwnaam;
                public string wijkcode;
                public string wijknaam;
                public string buurtcode;
                public string buurtnaam;
                public string knoopnummer;
                public string objectsoort;
                public string symbool_hoek;
                public string x_coordinaat;
                public string y_coordinaat;
                public string gemaal_nummer;
                public string stadsdeelcode;
                public string stadsdeelnaam;
                public string type_fundering;
                public string overstort_nummer;
                public string putdekselhoogte;
                public string overstort_drempelbreedte;
                public string drempelhoogte_overstortputten;
            }
        }
    }
}