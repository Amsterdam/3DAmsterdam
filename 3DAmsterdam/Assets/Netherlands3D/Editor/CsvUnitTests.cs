/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using NUnit.Framework;
public class CsvUnitTests 
{
    [Test]
    public void TestCsvParse()
    {
        var csv = @"rd_x;rd_y;yearstart, yearend, summary, image_url
150000;350000;2021;2022;Project Herziening rotonde 231;""http://url;234""";

        var lines = CsvParser.ReadLines(csv, 1);

        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual("150000", lines[0][0]);
        Assert.AreEqual("Project Herziening rotonde 231", lines[0][4]);
        Assert.AreEqual("http://url;234", lines[0][5]);
    }

    [Test]
    public void TestCsvParseTrimAndSemicolon()
    {
        var csv = @"rd_x;rd_y;yearstart, yearend, summary, image_url
150000;350000;2021;2022; Project ""Herziening"" rotonde 231 ;""http://url;234""";

        var lines = CsvParser.ReadLines(csv, 1);
        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual("150000", lines[0][0]);
        Assert.AreEqual(@"Project ""Herziening"" rotonde 231", lines[0][4]);
        Assert.AreEqual("http://url;234", lines[0][5]);
    }

    [Test]
    public void TestCsvParseFromRow0()
    {
        var csv = @"150000 ; 350000;2021;2022; Project ""Herziening"" rotonde 231 ;""http://url;234""";
        var lines = CsvParser.ReadLines(csv, 0);
        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual("150000", lines[0][0]);
        Assert.AreEqual(@"Project ""Herziening"" rotonde 231", lines[0][4]);
        Assert.AreEqual("http://url;234", lines[0][5]);
    }

    [Test]
    public void TestCsvParseComma()
    {
        var csv = @"150000,123 ; 350000,123;2021;2022; Project ""Herziening"" rotonde 231 ;""http://url;234""";
        var lines = CsvParser.ReadLines(csv, 0);
        Assert.AreEqual(1, lines.Count);
        Assert.AreEqual("150000,123", lines[0][0]);
        
        
    }


    //[Test]
    //public void TestCsvParseFile()
    //{
    //    string csv = File.ReadAllText(@"F:\Data\Projecten CSV\Projecten_Convert_JSON_to_CSV_CLEAN_test.csv");

    //    var lines = CsvParser.ReadLines(csv, 1);

    //    Assert.AreEqual(199, lines.Count);
    //    Assert.AreEqual(12, lines[0].Length);
    //    Assert.AreEqual("52.3317937588", lines[0][0]);
    //    Assert.AreEqual(@"Vanaf 2019. We gaan de A.J. Ernststraat tussen Buitenveldertselaan en Van der Boechorststraat opnieuw inrichten vanwege bouw internationale school.", lines[0][11]);        
    //}

    //[Test]
    //public void TestCsvParseFileUtrecht()
    //{
    //    string csv = File.ReadAllText(@"F:\Data\Projecten CSV\gu_programma_ruimtelijke_ontwikkeling_20200615_20200630.csv");

    //    var lines = CsvParser.ReadLines(csv, 1);

    //    //Assert.AreEqual(199, lines.Count);
    //    //Assert.AreEqual(12, lines[0].Length);
    //    //Assert.AreEqual("52.3317937588", lines[0][0]);
    //    //Assert.AreEqual(@"Vanaf 2019. We gaan de A.J. Ernststraat tussen Buitenveldertselaan en Van der Boechorststraat opnieuw inrichten vanwege bouw internationale school.", lines[0][11]);
    //}



}
