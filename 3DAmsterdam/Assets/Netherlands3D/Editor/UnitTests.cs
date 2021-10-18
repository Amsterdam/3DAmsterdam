using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using ConvertCoordinates;
using System;
using System.Linq;

public class UnitTests 
{

    [Test]
    public void TestReplaceXY()
    {
        string template = "terrain/terrain_{x}_{y}.lod1.mesh";
        var result = template.ReplaceXY(444000, 123000);
        Assert.AreEqual("terrain/terrain_444000_123000.lod1.mesh", result);
    }


    [Test]
    public void TestReplacePlaceholders()
    {
        string template = "test_{x}_{y}_{z}";
        object d = new
        {
            x = 1,
            y = 2,
            z = 3
        };
        var test = template.ReplacePlaceholders(d);
        Assert.AreEqual("test_1_2_3", test);
    }

    [Test]
    public void TestGetRDCoordinate()
    {
        var testFilePaths1 = new string[]{
            "123000_443000_utrecht_lod1",
            "utrecht_123000_443000_lod1",
            "buildings_123000_443000.1.2",
            "terrain_123000-443000",
            "terrain_123000-443000-lod1",
            "trees_123000-443000",
            "trees_123000-443000-lod1",
        };

        foreach (var filePath in testFilePaths1)
        {
            var expectedRd = new Vector3RD(123000, 443000, 0);
            var result = filePath.GetRDCoordinate();
            Assert.AreEqual(expectedRd, result);
        }

        var testFilePaths2 = new string[]{
            "7000_392000_zee_land_lod1",
            "zee_land_7000_392000_lod1",
            "buildings_7000_392000.1.2",
            "terrain_7000-392000",
            "terrain_7000-392000-lod1",
            "trees_7000-392000",
            "trees_7000-392000-lod1",
        };

        foreach (var filePath in testFilePaths2)
        {
            var expectedRd = new Vector3RD(7000, 392000, 0);
            var result = filePath.GetRDCoordinate();
            Assert.AreEqual(expectedRd, result);
        }

        var testFilePaths3 = new string[]{
            "AssetBundles",
            "move.py"
        };

        foreach (var filePath in testFilePaths3)
        {
            Vector3RD rd = filePath.GetRDCoordinate();
            Assert.AreEqual(new Vector3RD(), rd);            
        }

        

    }

    [Test]
    public void TestGetRDCoordinateByUrl()
    {
        var testurl = "https://3d.amsterdam.nl?position=123.4_456.77";
        var rd = testurl.GetRDCoordinateByUrl();
        Assert.AreEqual(123.4, rd.x);
        Assert.AreEqual(456.77, rd.y);

        var testurl2 = "https://3d.amsterdam.nl?position=23424.4_234234.84&anotherparam=test";
        var rd2 = testurl2.GetRDCoordinateByUrl();
        Assert.AreEqual(23424.4, rd2.x);
        Assert.AreEqual(234234.84, rd2.y);

        var testurl3 = "https://3d.amsterdam.nl";
        var rd3 = testurl3.GetRDCoordinateByUrl();
        Vector3RD nodata = new Vector3RD(0, 0, 0);
        Assert.AreEqual(nodata, rd3);

        var testurl4 = "http://localhost:8080/?position=121705_486658";
        var rd4 = testurl4.GetRDCoordinateByUrl();
        Assert.AreEqual(121705, rd4.x);
        Assert.AreEqual(486658, rd4.y);


    }

    [Test]
    public void TestGetUrlParamBool()
    {
        var hasfile1 = "url?hasfile=true".GetUrlParamBool("hasfile");
        Assert.AreEqual(true, hasfile1);

        var hasfile2 = "url?hasfile=True".GetUrlParamBool("hasfile");
        Assert.AreEqual(true, hasfile2);

        var hasfile3 = "url".GetUrlParamBool("hasfile");
        Assert.AreEqual(false, hasfile3);
        
    }

        [Test]
    public void TestGetUrlParamValue()
    {
        string url = "website.nl?position=123_456";
        var result1 = url.GetUrlParamValue("position");
        Assert.AreEqual("123_456", result1);

        string url2 = "website.nl?position=123_456&param2=test";
        var result2 = url2.GetUrlParamValue("position");
        var result2a = url2.GetUrlParamValue("param2");
        Assert.AreEqual("123_456", result2);
        Assert.AreEqual("test", result2a);

        string url3 = "website.nl?param1=test";
        var result3 = url3.GetUrlParamValue("position");
        Assert.AreEqual(null, result3);

        string url4 = "website.nl?param1=test&position=123_456";
        var result4 = url4.GetUrlParamValue("position");
        var result4a = url4.GetUrlParamValue("param1");
        Assert.AreEqual("123_456", result4);
        Assert.AreEqual("test", result4a);

        string url5 = "http://t3d.lab4242.nl/3d/?position=138350.607_455582.274&id=0344100000021804";
        var result5 = url5.GetUrlParamValue("position");
        var result5a = url5.GetUrlParamValue("id");
        Assert.AreEqual("138350.607_455582.274", result5);
        Assert.AreEqual("0344100000021804", result5a);

        string hashtest = "http://t3d.nl/#/?param1=test&position=123_456#test1&param2=test2";
        var hash_result1 = hashtest.GetUrlParamValue("param1");
        Assert.AreEqual("test", hash_result1);
        var hash_result2 = hashtest.GetUrlParamValue("position");
        Assert.AreEqual("123_456", hash_result2);
        var hash_result3 = hashtest.GetUrlParamValue("param2");
        Assert.AreEqual("test2", hash_result3);

    }


    [Test]
    public void TestRDIsInThousands()
    {
        Vector2RD rd1 = new Vector2RD(163409, 483129);
        Assert.AreEqual(false, rd1.IsInThousands, $"x:{rd1.x} y:{rd1.y}");

        Vector2RD rd2 = new Vector2RD(163000, 483129);
        Assert.AreEqual(false, rd2.IsInThousands, $"x:{rd2.x} y:{rd2.y}");

        Vector2RD rd3 = new Vector2RD(163000, 483000);
        Assert.AreEqual(true, rd3.IsInThousands, $"x:{rd3.x} y:{rd3.y}");

    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
        Assert.IsTrue(true);
    }



}
