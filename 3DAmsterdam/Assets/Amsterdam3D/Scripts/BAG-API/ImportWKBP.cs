using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportWKBP : ImportAPI
{

}

public class WKBP
{
    //https://api.data.amsterdam.nl/wkpb/beperking/?verblijfsobjecten__id=0363010000802188
    public class RootBeperkingen
    {
        //public _Links _links;
        public int count;
        public Result[] results;
    }
    /*
    public class _Links
    {
        public Self self;
        public Next next;
        public Previous previous;
    }

    public class Self
    {
        public string href;
    }

    public class Next
    {
        public string href;
    }

    public class Previous
    {
        public object href;
    }
    */
    public class Result
    {
        public BeperkingLink _links = new BeperkingLink();
        public Beperking beperking = new Beperking();
        public string _display;
        public int inschrijfnummer;
    }

    public class BeperkingLink
    {
        public BeperkingURL self = new BeperkingURL();
    }

    public class BeperkingURL
    {
        public string href;
    }

    public class Beperking
    {
        //public _Links _links;
        public string _display;
        public string id;
        public string inschrijfnummer;
        public string datum_in_werking;
        public string datum_einde;
        public Kadastrale_Objecten kadastrale_objecten = new Kadastrale_Objecten();
        public Documenten documenten = new Documenten();
        public Beperkingcode beperkingcode = new Beperkingcode();
    }
/*
    public class _Links
    {
        public Self self;
    }

    public class Self
    {
        public string href;
    }
    */
    public class Kadastrale_Objecten
    {
        public int count;
        public string href;
    }

    public class Documenten
    {
        public int count;
        public string href;
    }

    public class Beperkingcode
    {
        public string code;
        public string omschrijving;
    }

}