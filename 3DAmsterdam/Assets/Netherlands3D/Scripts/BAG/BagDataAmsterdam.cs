using System;

namespace Netherlands3D.BAG
{
    public enum BagApyType { Amsterdam, Kadaster }

    [System.Serializable]
    public class BagDataKadasterBuilding
    {
        [System.Serializable]
        public class Rootobject
        {
            public Pand pand;
            public _Links _links;
        }
        [System.Serializable]
        public class Pand
        {
            public string identificatie;
            public string domein;
            public Geometrie geometrie;
            public string oorspronkelijkBouwjaar;
            public string status;
            public string geconstateerd;
            public string documentdatum;
            public string documentnummer;
            public Voorkomen voorkomen;
        }
        [System.Serializable]
        public class Geometrie
        {
            public string type;
            public float[] coordinates = new float[0];
        }
        [System.Serializable]
        public class Voorkomen
        {
            public DateTime tijdstipRegistratie;
            public int versie;
            public string beginGeldigheid;
            public DateTime tijdstipRegistratieLV;
        }
        [System.Serializable]
        public class _Links
        {
            public Self self;
        }
        [System.Serializable]
        public class Self
        {
            public string href;
        }

    }

    [System.Serializable]
    public class BagDataKadasterBuildingAdresses
    {
        [System.Serializable]
        public class Rootobject
        {
            public _Links _links;
            public _Embedded _embedded;
        }
        [System.Serializable]
        public class _Links
        {
            public Self self;
            public Next next;
            public Last last;
        }
        [System.Serializable]
        public class Self
        {
            public string href;
        }
        [System.Serializable]
        public class Next
        {
            public string href;
        }
        [System.Serializable]
        public class Last
        {
            public string href;
        }
        [System.Serializable]
        public class _Embedded
        {
            public Adressen[] adressen;
        }
        [System.Serializable]
        public class Adressen
        {
            public string openbareRuimteNaam;
            public string korteNaam;
            public int huisnummer;
            public string postcode;
            public string woonplaatsNaam;
            public string nummeraanduidingIdentificatie;
            public string openbareRuimteIdentificatie;
            public string woonplaatsIdentificatie;
            public string adresseerbaarObjectIdentificatie;
            public string[] pandIdentificaties;
            public string adresregel5;
            public string adresregel6;
            public _Links1 _links;
        }
        [System.Serializable]
        public class _Links1
        {
            public Self1 self;
            public Openbareruimte openbareRuimte;
            public Nummeraanduiding nummeraanduiding;
            public Woonplaats woonplaats;
            public Adresseerbaarobject adresseerbaarObject;
            public Panden[] panden;
        }
        [System.Serializable]
        public class Self1
        {
            public string href;
        }
        [System.Serializable]
        public class Openbareruimte
        {
            public string href;
        }
        [System.Serializable]
        public class Nummeraanduiding
        {
            public string href;
        }
        [System.Serializable]
        public class Woonplaats
        {
            public string href;
        }
        [System.Serializable]
        public class Adresseerbaarobject
        {
            public string href;
        }
        [System.Serializable]
        public class Panden
        {
            public string href;
        }

    }

    [System.Serializable]
    public class BagDataAmsterdam
    {
        [System.Serializable]
        public class Rootobject
        {
            //public _Links _links = new _Links();
            public int count;
            public PandResults[] results;
            public string _display;
            public string pandidentificatie;
            public string date_modified;
            public string document_mutatie;
            public string document_nummer;
            public string status;
            public float[] bbox;
            //public Geometrie geometrie; 
            public string oorspronkelijk_bouwjaar;
            public int bouwlagen;
            public string hoogste_bouwlaag;
            public string laagste_bouwlaag;
            public string pandnaam;
            public string ligging;
            public string type_woonobject;
            public Verblijfsobjecten verblijfsobjecten;
            //public _Adressen _adressen;
            public Monumenten monumenten;
            public Bouwblok bouwblok;
            public string begin_geldigheid;
            public string einde_geldigheid;
            public _Buurt _buurt;
            public _Buurtcombinatie _buurtcombinatie;
            public _Stadsdeel _stadsdeel;
            public _Gemeente _gemeente;
            public string dataset;

            // simpele operator die de resultaten van het pand toevoegd aan het aangemaakte pand
            public static BagDataAmsterdam.Rootobject operator +(Rootobject a, Rootobject b)
            {
                a.count = b.count;
                a.results = b.results;
                return a;
            }

        }

        [System.Serializable]
        public class Verblijfsobjecten
        {
            public int count;
            public string href;
        }
        [System.Serializable]
        public class _Adressen
        {
            public string href;
        }
        [System.Serializable]
        public class _Monumenten
        {
            public string href;
        }

        [System.Serializable]
        public class Bouwblok
        {
            //public _Links _links = new _Links();
            public string _display;
            public string id;
            public string dataset;
        }

        [System.Serializable]
        public class _Buurt
        {
            //public _Links _links = new _Links();
            public string _display;
            public string code;
            public string naam;
            public string dataset;
        }

        [System.Serializable]
        public class _Buurtcombinatie
        {
            //public _Links _links = new _Links();
            public string _display;
            public string naam;
            public string vollcode;
            public string dataset;
        }

        [System.Serializable]
        public class _Stadsdeel
        {
            //public _Links _links = new _Links();
            public string _display;
            public string code;
            public string naam;
            public string dataset;
        }

        [System.Serializable]
        public class _Gemeente
        {
            public string _display;
            //public _Links _links = new _Links();
            public string naam;
            public string code;
            public string dataset;
        }

        [System.Serializable]
        public class PandResults
        {
            //public NummerLinks _links = new NummerLinks();
            public AddressInstance nummeraanduiding = new AddressInstance();
            public ResidenceInstance verblijfsobject = new ResidenceInstance();
            public string _display;
            public string landelijk_id;
            public string type_adres;
            public string vbo_status;
            public string dataset;
        }
        /* // onnodige links?
        [System.Serializable]
        public class NummerLinks
        {
            public NummerSelf self = new NummerSelf();
        }
        [System.Serializable]
        public class NummerSelf
        {
            public string href;
        }
        */
        [System.Serializable]
        public class AddressInstance
        {
            //public _Links _links;
            public string _display;
            public string nummeraanduidingidentificatie;
            public string date_modified;
            public string document_mutatie;
            public string document_nummer;
            public string begin_geldigheid;
            public string einde_geldigheid;
            public string status;
            public string bron;
            public string adres;
            public string postcode;
            public int huisnummer;
            public string huisletter;
            public string huisnummer_toevoeging;
            public string type;
            //public Openbare_Ruimte openbare_ruimte;
            public string type_adres;
            public string ligplaats;
            public string standplaats;
            public string verblijfsobject;
            //public Buurt buurt;
            //public Buurtcombinatie buurtcombinatie;
            //public Gebiedsgerichtwerken gebiedsgerichtwerken;
            public object grootstedelijkgebied;
            //public Stadsdeel stadsdeel;
            public Woonplaats woonplaats;
            public Bouwblok bouwblok;
            //public _Geometrie _geometrie;
            //public string dataset;
        }

        public class _Links
        {
            public Self self;
        }
        public class Self
        {
            public string href;
        }

        [System.Serializable]
        public class Woonplaats
        {
            public _Links _links;
            public string _display;
            public string landelijk_id;
            public string dataset;
        }


        //public class Bouwblok
        //{
        //    public _Links _links;
        //    public string _display;
        //    public string id;
        //    public string dataset;
        //}

        //public class _Geometrie
        //{
        //    public string type;
        //    public float[] coordinates;
        //}



        [System.Serializable]
        public class ResidenceInstance
        {
            //  public _Links _links;
            public string _display;
            public string verblijfsobjectidentificatie;
            //  public DateTime date_modified;
            public string document_mutatie;
            public string document_nummer;
            public string begin_geldigheid;
            public string einde_geldigheid;
            public string status;
            public string bron;
            public float[] bbox;
            //  public Geometrie geometrie;
            public string oppervlakte;
            public string verdieping_toegang;
            public string bouwlagen;
            public string hoogste_bouwlaag;
            public string laagste_bouwlaag;
            public string aantal_kamers;
            public string reden_afvoer;
            public string reden_opvoer;
            public string eigendomsverhouding;
            public string gebruik;
            public string[] toegang;
            //   public Hoofdadres hoofdadres;
            //   public Adressen adressen;
            //   public Buurt buurt;
            //   public Panden panden;
            //   public Kadastrale_Objecten kadastrale_objecten;
            //   public Rechten rechten;
            public URLLimitations beperkingen = new URLLimitations();
            //   public Bouwblok bouwblok;
            public string indicatie_geconstateerd;
            public string aanduiding_in_onderzoek;
            public string[] gebruiksdoel;
            public string gebruiksdoel_woonfunctie;
            public string gebruiksdoel_gezondheidszorgfunctie;
            public string aantal_eenheden_complex;
            //   public _Buurtcombinatie _buurtcombinatie;
            //   public _Stadsdeel _stadsdeel;
            //   public _Gebiedsgerichtwerken _gebiedsgerichtwerken;
            public string _grootstedelijkgebied;
            //   public _Gemeente _gemeente;
            //   public _Woonplaats _woonplaats;
            public string dataset;
        }
        public class Geometrie
        {
            public string type;
            public float[] coordinates;
        }

        public class Hoofdadres
        {
            public _Links _links;
            public string _display;
            public string landelijk_id;
            public string type_adres;
            public string vbo_status;
            public string dataset;
        }

        public class Adressen
        {
            public int count;
            public string href;
        }

        public class Buurt
        {
            public _Links _links;
            public string _display;
            public string code;
            public string naam;
            public string dataset;
        }

        public class Panden
        {
            public int count;
            public string href;
        }

        public class Kadastrale_Objecten
        {
            public int count;
            public string href;
        }

        public class Rechten
        {
            public int count;
            public string href;
        }

        public class URLLimitations
        {
            public int count;
            public string href;
        }

        public class _Gebiedsgerichtwerken
        {
            public _Links _links;
            public string _display;
            public string code;
            public string naam;
            public string dataset;
        }

        public class _Woonplaats
        {
            public _Links _links;
            public string _display;
            public string landelijk_id;
            public string dataset;
        }

        [System.Serializable]
        public class Monumenten
        {
            //public _Links _links;
            public int count;
            public MonumentResults[] results;
        }

        [System.Serializable]
        public class MonumentResults
        {
            //public _Links _links;
            public string identificerende_sleutel_monument;
            public string monumentnummer;
            public string monumentnaam;
            public string monumentstatus;
            public string monument_aanwijzingsdatum;
            //public Betreft_Pand[] betreft_pand;
            public string _display;
            public string heeft_als_grondslag_beperking;
            //public Heeft_Situeringen heeft_situeringen;
            //public Monumentcoordinaten monumentcoordinaten;
            public string ligt_in_complex;
            public string in_onderzoek;
        }
        public class Heeft_Situeringen
        {
            public int count;
            public string href;
        }

        public class Monumentcoordinaten
        {
            public string type;
            public float[] coordinates;
        }

        public class Betreft_Pand
        {
            public string pandidentificatie;
            public _Links _links;
        }
    }

    [System.Serializable]
    public class BagDataKadasterViewer
    {
        public openbareruimte_model openbareruimte;
        public adresseerbaarobject_model adresseerbaarobject;
        public nummeraanduiding_model nummeraanduiding;
        public pand_model[] panden;

        [System.Serializable]
        public class openbareruimte_model
        {
            public string naam;
            public string displayString;
            public string postcode;
            public string documentnummer;
        }

        [System.Serializable]
        public class adresseerbaarobject_model
        {
            public string displayString;
            public string gebruiksdoel;
            public string oppervlakte;
            public string documentnummer;
            public geometry_model geometry;
        }

        [System.Serializable]
        public class geometry_model
        {
            public string type;
            public float[] coordinates;
        }

        [System.Serializable]
        public class nummeraanduiding_model
        {
            public string postcode;
            public string huisletter;
            public string huisnummer;
        }

        [System.Serializable]
        public class pand_model
        {
            public string status;
            public string documentnummer;
            public geometry_model geometry;
            public string bouwjaar;
        }
    }


}