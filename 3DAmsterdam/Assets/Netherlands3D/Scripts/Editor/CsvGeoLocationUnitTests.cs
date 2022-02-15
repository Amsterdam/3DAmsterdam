using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.IO;
using System;
using System.Linq;

public class CsvGeoLocationUnitTests
{
    string csv = @"Omschrijving;RD X;RD Y;Datum aanvraag;Datum oplevering;3d object
Project 'Herziening rotonde 231';150000;350000;11/8/2021;3/2/2022;projects/423_1
Bouwplan 'De Nieuwe Stad'; 178000; 312000; 2 / 1 / 2022; 14/6/2023; projects / 423_2
Overspanning viaduct Laarderweg; 80000; 350000; 3/9/2021; 30/12/2021; projects / 423_3";

    [Test]
    public void TestCsvGeoLocationParse()
    {
        var lines = CsvParser.ReadLines(csv, 0);
        var Columns = lines.First();
        var Rows = new List<string[]>();
        int columnscount = Columns.Length;
        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Length == columnscount)
            {
                Rows.Add(lines[i]);
            }
        }

        var csvgeoloc = new CsvGeoLocationFinder(Columns, Rows);
        Assert.AreEqual(3, csvgeoloc.Rows.Count);
        Assert.AreEqual(2, csvgeoloc.CoordinateColumns.Length );       
    }

    [Test]
    public void TestCsvGeoLocationRows()
    {
        var lines = CsvParser.ReadLines(csv, 0);
        var Columns = lines.First();
        var Rows = new List<string[]>();
        int columnscount = Columns.Length;
        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Length == columnscount)
            {
                Rows.Add(lines[i]);
            }
        }


        var csvgeoloc = new CsvGeoLocationFinder(Columns, Rows);
        Assert.AreEqual(3, csvgeoloc.Rows.Count);

        var omschrijving1 = csvgeoloc.Rows[0][0];
        Assert.AreEqual("Project 'Herziening rotonde 231'", omschrijving1);
    }

    [Test]
    public void TestMultiLine()
    {
        var csv = @"U_ID;Wijk;Prj_naam;Ontw;Omschr;Soort_proj;Type_proj;Fase;Tot_prod;Start;Oplevering;LAT;LONG;X_RD;Y_RD
359;West;Ravellaan 205-209;ReShape Properties BV;Nieuwbouw van woningen;Particulier Initiatief;Nieuwbouw;Uitvoering;31;2019;2020;52.083123;5.090972;134694.8139;455025.4800
437;West;Thomas a Kempisplantsoen;Mitros en Portaal;""Het slopen van 178 gestapelde sociale huurwoningen, 9 winkels, een garage en 6 opslagruimtes en nieuwbouwen van ongeveer 400 woningen
Het slopen van 178 gestapelde sociale huurwoningen, 9 winkels, een garage en 6 opslagruimtes en nieuwbouwen van ongeveer 400 woningen
Het slopen van 178 gestapelde sociale huurwoningen, 9 winkels, een garage en 6 opslagruimtes en nieuwbouwen van ongeveer 400 woningen
Het slopen van 178 gestapelde sociale huurwoningen, 9 winkels, een garage en 6 opslagruimtes en nieuwbouwen van ongeveer 400 woningen
"";Particulier Initiatief;Nieuwbouw;Definitie;400;2022;2024;52.0964235021;5.084729408;134273.0759;456507.0160
165;West;Vlampijpzone;Gemeente & meerdere partijen;Herinrichting openbare ruimte;Openbare Ruimte;Openbare Ruimte;Uitvoering;0;2017;2020;52.1053533697;5.0847081297;134275.7563;457500.5473";

        var lines = CsvParser.ReadLines(csv, 0);
        var Columns = lines.First();
        var Rows = new List<string[]>();
        int columnscount = Columns.Length;
        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Length == columnscount)
            {
                Rows.Add(lines[i]);
            }
        }

        var projects = new CsvGeoLocationFinder(Columns, Rows);

        Assert.AreEqual(15, projects.Columns.Length);
        Assert.AreEqual(2, projects.Rows.Count);

        foreach (var row in projects.Rows)
        {
            Assert.AreEqual(15, row.Length);
        }

    }

    [Test]
    public void TestBagLigplaatsen()
    {
        string csv = @"identificatie;aanduidingInOnderzoek;geconstateerd;heeftIn:BAG.NAG.identificatieHoofdadres;huisnummerHoofdadres;huisletterHoofdadres;huisnummertoevoegingHoofdadres;postcodeHoofdadres;ligtAan:BAG.ORE.identificatieHoofdadres;ligtAan:BAG.ORE.naamHoofdadres;ligtIn:BAG.WPS.identificatieHoofdadres;ligtIn:BAG.WPS.naamHoofdadres;ligtIn:BRK.GME.identificatie;ligtIn:BRK.GME.naam;heeftIn:BAG.NAG.identificatieNevenadres;status;is:WOZ.WOB.soortObject;beginGeldigheid;eindGeldigheid;documentdatum;documentnummer;ligtIn:GBD.BRT.identificatie;ligtIn:GBD.BRT.code;ligtIn:GBD.BRT.naam;ligtIn:GBD.WIJK.identificatie;ligtIn:GBD.WIJK.code;ligtIn:GBD.WIJK.naam;ligtIn:GBD.GGW.identificatie;ligtIn:GBD.GGW.code;ligtIn:GBD.GGW.naam;ligtIn:GBD.GGP.identificatie;ligtIn:GBD.GGP.code;ligtIn:GBD.GGP.naam;ligtIn:GBD.SDL.identificatie;ligtIn:GBD.SDL.code;ligtIn:GBD.SDL.naam;geometrie
0363020000724414;N;N;0363200000415534;206;;;1019BG;0363300000004167;Levantkade;3594;Amsterdam;0363;Amsterdam;;Plaats aangewezen;;2010-09-09T00:00:00;;2010-09-09;GV00000407;03630000000584;M33d;KNSM-eiland;03630012052076;M33;Oostelijk Havengebied;03630950000013;DX14;Indische Buurt, Oostelijk Havengebied;03630940000015;PX17;Oostelijk Havengebied;03630011872039;M;Oost;POLYGON ((124925.138 487680.771, 124926.262 487676.629, 124928.765 487676.792, 124929.671 487681.174, 124928.765 487701.625, 124928.545 487702.044, 124928.275 487702.432, 124927.96 487702.785, 124927.604 487703.097, 124927.212 487703.362, 124926.791 487703.578, 124926.347 487703.741, 124925.903 487703.552, 124925.488 487703.305, 124925.11 487703.004, 124924.777 487702.655, 124924.495 487702.264, 124924.268 487701.838, 124924.1 487701.386, 124923.996 487700.914, 124925.138 487680.771))
0363020000939758;N;N;0363200000386615;19;;;1081KE;0363300000002966;Boeierspad;3594;Amsterdam;0363;Amsterdam;;Plaats aangewezen;;2010-09-09T00:00:00;;2010-09-09;GV00000407;03630000000849;K90e;Amsterdamse Bos;03630012052010;K90;Buitenveldert-West;03630950000010;DX11;Buitenveldert, Zuidas;03630940000010;PX12;Buitenveldert, Zuidas;03630011872038;K;Zuid;POLYGON ((118435.143 482883.381, 118435.563 482888.364, 118419.349 482889.73, 118418.929 482884.747, 118435.143 482883.381))
0363020001002578;N;N;0363200000486734;34;;;1096CR;0363300000004774;Ouderkerkerdijk;3594;Amsterdam;0363;Amsterdam;;Plaats aangewezen;;2010-09-09T00:00:00;;2010-09-09;GV00000407;03630000000781;M58e;Amstelglorie;03630012052009;M58;Omval/Overamstel;03630950000014;DX15;Watergraafsmeer;03630940000017;PX19;Watergraafsmeer;03630011872039;M;Oost;POLYGON ((122082.563 482912.175, 122097.917 482916.632, 122095.85 482923.754, 122080.496 482919.296, 122082.563 482912.175))
0363020012061174;N;N;0363200012063218;24;A;;1081KG;0363300000004022;Koenenkade;3594;Amsterdam;0363;Amsterdam;;Plaats aangewezen;;2011-06-22T00:00:00;;2011-06-22;SK00000653;03630000000849;K90e;Amsterdamse Bos;03630012052010;K90;Buitenveldert-West;03630950000010;DX11;Buitenveldert, Zuidas;03630940000010;PX12;Buitenveldert, Zuidas;03630011872038;K;Zuid;POLYGON ((117912.506 482707.925, 117912.381 482708.05, 117904.486 482720.581, 117900.852 482721.584, 117900.1 482718.201, 117907.619 482705.043, 117912.506 482707.925))
0363020001027084;N;N;0363200000515068;120;G;;1018VZ;0363300000003920;Nieuwe Prinsengracht;3594;Amsterdam;0363;Amsterdam;;Plaats aangewezen;;2010-09-09T00:00:00;;2010-09-09;GV00000407;03630000000516;A08a;Weesperbuurt;03630012052031;A08;Weesperbuurt/Plantage;03630950000001;DX02;Centrum-Oost;03630940000001;PX02;Centrum-Oost;03630000000018;A;Centrum;POLYGON ((122508.275 486350.327, 122508.527 486350.053, 122509.492 486349.574, 122510.012 486349.508, 122511.188 486349.569, 122512.612 486349.838, 122526.48 486353.827, 122525.443 486357.431, 122511.575 486353.442, 122510.226 486352.914, 122509.197 486352.341, 122508.792 486352.01, 122508.229 486351.091, 122508.162 486350.72, 122508.275 486350.327))";

        var lines = CsvParser.ReadLines(csv, 0);
        var Columns = lines.First();
        var Rows = new List<string[]>();
        int columnscount = Columns.Length;
        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Length == columnscount)
            {
                Rows.Add(lines[i]);
            }
        }

        var bagligplaatsen = new CsvGeoLocationFinder(Columns, Rows);
        Assert.AreEqual(bagligplaatsen.Status, CsvContentFinder.CsvContentFinderStatus.Failed);
    }

    [Test]
    public void TestPanoramaSemicolon()
    {
        string csv = @"gps_seconds[s];panorama_file_name;latitude[deg];longitude[deg];altitude_ellipsoidal[m];roll[deg];pitch[deg];heading[deg]
1178263175.05672;1178263175_056717_0;52.3516092805;4.8426335585;44.4415162113;-0.9556548416;0.0973349015;87.3409739047
1178263176.19669;1178263176_196691_1;52.3516076631;4.8424874133;44.4738408364;-0.7259166659;-0.0314014078;88.5896089189
1178263177.30668;1178263177_306679_2;52.3516082396;4.8423413503;44.5097852116;-0.7085167049;-0.1083775873;90.0325795637
1178263178.38665;1178263178_386648_3;52.3516118477;4.8421957189;44.5319389142;-1.0652341476;0.2426424216;92.0613240058
1178263179.52664;1178263179_526641_4;52.3516184611;4.8420511209;44.5627396507;-1.5071903225;0.398587759;93.8898811884
1178263181.03681;1178263181_036811_5;52.3516244602;4.8419080467;44.611617892;-1.7137684824;0.6385912069;91.6988523355
1178263196.78151;1178263196_781509_6;52.3516072083;4.8417661974;45.2869139686;-1.4987202908;-0.0137625108;88.9704470561";

        var lines = CsvParser.ReadLines(csv, 0);
        var Columns = lines.First();
        var Rows = new List<string[]>();
        int columnscount = Columns.Length;
        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Length == columnscount)
            {
                Rows.Add(lines[i]);
            }
        }

        var panoramas = new CsvGeoLocationFinder(Columns, Rows);
        Assert.AreEqual(2, panoramas.CoordinateColumns.Length);

    }

    [Test]
    public void TestPanoramaDataNoColumns()
    {
        string panoramadata = @"gps_seconds[s]	panorama_file_name	latitude[deg]	longitude[deg]	altitude_ellipsoidal[m]	roll[deg]	pitch[deg]	heading[deg]
1178263175.056717	1178263175_056717_0	52.3516092805462	4.84263355846614	44.4415162112564	-0.955654841566599	0.0973349014609538	87.3409739047135
1178263176.196691	1178263176_196691_1	52.3516076631198	4.84248741326708	44.4738408364356	-0.725916665867848	-0.031401407848429	88.5896089189283
1178263177.306679	1178263177_306679_2	52.3516082396404	4.84234135030321	44.5097852116451	-0.708516704927227	-0.108377587284742	90.0325795636947
1178263178.386648	1178263178_386648_3	52.3516118476512	4.8421957188643	44.5319389142096	-1.065234147629	0.242642421605154	92.0613240058035";

        var lines = CsvParser.ReadLines(panoramadata, 0);
        var Columns = lines.First();
        var Rows = new List<string[]>();
        int columnscount = Columns.Length;
        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Length == columnscount)
            {
                Rows.Add(lines[i]);
            }
        }

        var panoramas = new CsvGeoLocationFinder(Columns, Rows);
        Assert.AreEqual(CsvContentFinder.CsvContentFinderStatus.Failed, panoramas.Status);
    }

    [Test]
    public void TestPanoramaDataCoordinates()
    {
        string panoramadata = @"gps_seconds[s];panorama_file_name;latitude[deg];longitude[deg];altitude_ellipsoidal[m];roll[deg];pitch[deg];heading[deg]
1178263175.056717;1178263175_056717_0;52.3516092805462;4.84263355846614;44.4415162112564;-0.955654841566599;0.0973349014609538;87.3409739047135
1178263176.196691;1178263176_196691_1;52.3516076631198;4.84248741326708;44.4738408364356;-0.725916665867848;-0.031401407848429;88.5896089189283
1178263177.306679;1178263177_306679_2;52.3516082396404;4.84234135030321;44.5097852116451;-0.708516704927227;-0.108377587284742;90.0325795636947
1178263178.386648;1178263178_386648_3;52.3516118476512;4.8421957188643;44.5319389142096;-1.065234147629;0.242642421605154;92.0613240058035";

        var lines = CsvParser.ReadLines(panoramadata, 0);
        var Columns = lines.First();
        var Rows = new List<string[]>();
        int columnscount = Columns.Length;
        for (int i = 1; i < lines.Count; i++)
        {
            if (lines[i].Length == columnscount)
            {
                Rows.Add(lines[i]);
            }
        }

        var panoramas = new CsvGeoLocationFinder(Columns, Rows);
        Assert.AreEqual(CsvContentFinder.CsvContentFinderStatus.Success, panoramas.Status);
        Assert.AreEqual(panoramas.CoordinateColumns.Length, 2);

    }

    [Test]
    public void TestIsCoordinate()
    {
        //Netherlands bounding box RD
        //sw x/y 7000, 289000
        //ne x/y 280000, 629000
        //range x = 7000 - 280000
        //range y = 289000 - 629000

        //Netherlands bounding box lat/lon
        //sw lat/lon 50.57222, 3.29804        
        //ne lat/lon 53.62702, 7.57893        
        //range lat 50.57222 - 53.62702
        //range lon 3.29804 - 7.57893

        bool iscoordinate1 = CsvGeoLocationFinder.IsCoordinate("title");
        Assert.AreEqual(false, iscoordinate1, "IsCoordinate title");

        bool iscoordinate2 = CsvGeoLocationFinder.IsCoordinate("2343");
        Assert.AreEqual(false, iscoordinate2, "IsCoordinate 2343");

        bool iscoordinate3 = CsvGeoLocationFinder.IsCoordinate("7500");
        Assert.AreEqual(true, iscoordinate3, "IsCoordinate 7500");

        bool iscoordinate4 = CsvGeoLocationFinder.IsCoordinate("4");
        Assert.AreEqual(true, iscoordinate4, "IsCoordinate 4");

        bool iscoordinate5 = CsvGeoLocationFinder.IsCoordinate("9");
        Assert.AreEqual(false, iscoordinate5, "IsCoordinate 9");

        bool iscoordinate6 = CsvGeoLocationFinder.IsCoordinate("284000");
        Assert.AreEqual(false, iscoordinate6, "IsCoordinate 284000");

        bool iscoordinate7 = CsvGeoLocationFinder.IsCoordinate("650000");
        Assert.AreEqual(false, iscoordinate7, "IsCoordinate 650000");

        bool iscoordinate8 = CsvGeoLocationFinder.IsCoordinate("450000");
        Assert.AreEqual(true, iscoordinate8, "IsCoordinate 450000");

        bool iscoordinate9 = CsvGeoLocationFinder.IsCoordinate("450000,12342134");
        Assert.AreEqual(true, iscoordinate9, "IsCoordinate 450000,12342134");

        bool iscoordinate10 = CsvGeoLocationFinder.IsCoordinate("450000.12342134");
        Assert.AreEqual(true, iscoordinate10, "IsCoordinate 450000.12342134");
    }

}
