/// <summary>
/// Search results JSON data object based on following urls:
/// https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q={SEARCHTERM}%20and%20Amsterdam%20and%20type:adres&rows=5
/// https://geodata.nationaalgeoregister.nl/locatieserver/v3/lookup?id={ID}
/// </summary>

namespace Netherlands3D.Interface.Search{
    [System.Serializable]
    public class LookupData
    {
        public Response response;

        [System.Serializable]
        public class Response
        {
            public int numFound = 0;
            public int start = 0;
            public float maxScore = 0.0f;
            public Docs[] docs;

            [System.Serializable]
            public class Docs
            {
                public string type = "";
                public string id = "";
                public string centroide_ll = "POINT(0.0 0.0)";
                public string nummeraanduiding_id = "";
                public string openbareruimte_id = "";
                public string adresseerbaarobject_id = "";
            }
        }
    }
    [System.Serializable]
    public class SearchData
    {
        public Response response;
        public SearchSpellcheck spellcheck;

        [System.Serializable]
        public class Response
        {
            public int numFound = 0;
            public int start = 0;
            public float maxScore = 0.0f;
            public Docs[] docs;

            [System.Serializable]
            public class Docs
            {
                public string type = "";
                public string weergavenaam = "";
                public string id = "";
                public float score = 0.0f;
            }
        }
        [System.Serializable]
        public class SearchSpellcheck
        {
            public Suggestions[] suggestions;
            [System.Serializable]
            public class Suggestions
            {
                public int numFound = 0;
                public int start = 0;
                public float maxScore = 0.0f;
                public string[] suggestion;
            }
        }
    }
}