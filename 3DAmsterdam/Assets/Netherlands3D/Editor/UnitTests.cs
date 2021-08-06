﻿/*
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
            Assert.Throws<Exception>(
             delegate { filePath.GetRDCoordinate(); });
        }

        Exception ex = Assert.Throws<Exception>(
            delegate { "AssetBundles".GetRDCoordinate(); });        

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
