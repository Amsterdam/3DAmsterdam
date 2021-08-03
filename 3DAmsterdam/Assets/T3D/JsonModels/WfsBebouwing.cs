using System;

namespace JsonModels.WfsBebouwing
{

    public class Rootobject
    {
        public string type { get; set; }
        public int numberMatched { get; set; }
        public string name { get; set; }
        public Crs crs { get; set; }
        public Feature[] features { get; set; }
    }

    public class Crs
    {
        public string type { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public string name { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public string id { get; set; }
        public Properties1 properties { get; set; }
        public Geometry geometry { get; set; }
    }

    public class Properties1
    {
        public string objectBeginTijd { get; set; }
        public DateTime LVpublicatiedatum { get; set; }
        public int relatieveHoogteligging { get; set; }
        public string inOnderzoek { get; set; }
        public DateTime tijdstipRegistratie { get; set; }
        public string identificatieNamespace { get; set; }
        public string identificatieLokaalID { get; set; }
        public string bronhouder { get; set; }
        public string bgtstatus { get; set; }
        public string plusstatus { get; set; }
        public string identificatieBAGPND { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public float[][][] coordinates { get; set; }
    }

}