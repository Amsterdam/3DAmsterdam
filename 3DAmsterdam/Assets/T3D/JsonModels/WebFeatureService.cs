using System;
namespace JsonModels.WebFeatureService
{

    public class WFSRootobject
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
        public string identificatieNamespace { get; set; }
        public string identificatieLokaalID { get; set; }
        public DateTime beginGeldigheid { get; set; }
        public DateTime tijdstipRegistratie { get; set; }
        public int volgnummer { get; set; }
        public string statusHistorieCode { get; set; }
        public string statusHistorieWaarde { get; set; }
        public string kadastraleGemeenteCode { get; set; }
        public string kadastraleGemeenteWaarde { get; set; }
        public string sectie { get; set; }
        public string AKRKadastraleGemeenteCodeCode { get; set; }
        public string AKRKadastraleGemeenteCodeWaarde { get; set; }
        public float kadastraleGrootteWaarde { get; set; }
        public string soortGrootteCode { get; set; }
        public string soortGrootteWaarde { get; set; }
        public int perceelnummer { get; set; }
        public float perceelnummerRotatie { get; set; }
        public float perceelnummerVerschuivingDeltaX { get; set; }
        public float perceelnummerVerschuivingDeltaY { get; set; }
        public float perceelnummerPlaatscoordinaatX { get; set; }
        public float perceelnummerPlaatscoordinaatY { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public float[][][] coordinates { get; set; }
    }

}