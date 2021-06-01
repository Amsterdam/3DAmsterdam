using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.IO;
using System;

public class CsvGeoLocationUnitTests
{

    [Test]
    public void TestCsvGeoLocationParse()
    {
        var csv = @"Omschrijving;RD X;RD Y;Datum aanvraag;Datum oplevering;3d object
Project 'Herziening rotonde 231';150000;350000;11/8/2021;3/2/2022;projects/423_1
Bouwplan 'De Nieuwe Stad'; 178000; 312000; 2 / 1 / 2022; 14/6/2023; projects / 423_2
Overspanning viaduct Laarderweg; 80000; 350000; 3/9/2021; 30/12/2021; projects / 423_3";
        
        var csvgeoloc = new CsvGeoLocation(csv);

        Assert.AreEqual(3, csvgeoloc.Rows.Count);
        Assert.AreEqual(2, csvgeoloc.CoordinateColumns.Length );       
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

        var projects = new CsvGeoLocation(csv);

        Assert.AreEqual(15, projects.Columns.Length);
        Assert.AreEqual(2, projects.Rows.Count);

        foreach (var row in projects.Rows)
        {
            Assert.AreEqual(15, row.Length);
        }

    }


    //[Test]
    //public void LoadAndParseCsvFile()
    //{
    //    string csv = File.ReadAllText(@"F:\Data\Projecten CSV\gu_programma_ruimtelijke_ontwikkeling_20200615_20200630.csv");
    //    var projects = new CsvGeoLocation(csv);
    //    Assert.AreEqual(15, projects.Columns.Length);
    //    Assert.AreEqual(370, projects.Rows.Count);

    //    foreach (var row in projects.Rows)
    //    {
    //        Assert.AreEqual(15, row.Length);
    //    }
    //}

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

        bool iscoordinate1 =  CsvGeoLocation.IsCoordinate("title");
        Assert.AreEqual(false, iscoordinate1, "IsCoordinate title");

        bool iscoordinate2 = CsvGeoLocation.IsCoordinate("2343");
        Assert.AreEqual(false, iscoordinate2, "IsCoordinate 2343");

        bool iscoordinate3 = CsvGeoLocation.IsCoordinate("7500");
        Assert.AreEqual(true, iscoordinate3, "IsCoordinate 7500");

        bool iscoordinate4 = CsvGeoLocation.IsCoordinate("4");
        Assert.AreEqual(true, iscoordinate4, "IsCoordinate 4");

        bool iscoordinate5 = CsvGeoLocation.IsCoordinate("9");
        Assert.AreEqual(false, iscoordinate5, "IsCoordinate 9");

        bool iscoordinate6 = CsvGeoLocation.IsCoordinate("284000");
        Assert.AreEqual(false, iscoordinate6, "IsCoordinate 284000");

        bool iscoordinate7 = CsvGeoLocation.IsCoordinate("650000");
        Assert.AreEqual(false, iscoordinate7, "IsCoordinate 650000");

        bool iscoordinate8 = CsvGeoLocation.IsCoordinate("450000");
        Assert.AreEqual(true, iscoordinate8, "IsCoordinate 450000");


    }

}
