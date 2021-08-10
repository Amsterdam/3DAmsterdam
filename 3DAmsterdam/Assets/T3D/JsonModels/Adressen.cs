namespace JsonModels
{
    public class RootobjectAdressen
    {
        public _Links _links { get; set; }
        public _Embedded _embedded { get; set; }
    }

    public class _Links
    {
        public Self self { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class _Embedded
    {
        public Adressen[] adressen { get; set; }
    }

    public class Adressen
    {
        public string openbareRuimteNaam { get; set; }
        public string korteNaam { get; set; }
        public int huisnummer { get; set; }
        public string postcode { get; set; }
        public string woonplaatsNaam { get; set; }
        public string nummeraanduidingIdentificatie { get; set; }
        public string openbareRuimteIdentificatie { get; set; }
        public string woonplaatsIdentificatie { get; set; }
        public string adresseerbaarObjectIdentificatie { get; set; }
        public string[] pandIdentificaties { get; set; }
        public _Links1 _links { get; set; }
    }

    public class _Links1
    {
        public Self1 self { get; set; }
        public Openbareruimte openbareRuimte { get; set; }
        public Nummeraanduiding nummeraanduiding { get; set; }
        public Woonplaats woonplaats { get; set; }
        public Adresseerbaarobject adresseerbaarObject { get; set; }
        public Panden[] panden { get; set; }
    }

    public class Self1
    {
        public string href { get; set; }
    }

    public class Openbareruimte
    {
        public string href { get; set; }
    }

    public class Nummeraanduiding
    {
        public string href { get; set; }
    }

    public class Woonplaats
    {
        public string href { get; set; }
    }

    public class Adresseerbaarobject
    {
        public string href { get; set; }
    }

    public class Panden
    {
        public string href { get; set; }
    }


}