using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using ConvertCoordinates;
using System;
using System.Linq;
using Netherlands3D;
using Netherlands3D.AssetGeneration;

public class ParserUnitTests 
{
    [Test]
    public void TileCombinerTest()
    {
        //Very small boundingbox for RD config
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.RelativeCenterRD = new Vector2RD(15000, 15000);
        Config.activeConfiguration.MinBoundingBox = new Vector2RD(10000, 10000);
        Config.activeConfiguration.MaxBoundingBox = new Vector2RD(20000, 20000);

        var vertArrayWithVertInBounds = new Vector3[2] { 
            CoordConvert.RDtoUnity(new Vector3(16000, 17000, 0)),
            CoordConvert.RDtoUnity(new Vector3(25000, 26000, 0))
         };
        var vertArrayWithWithoutVertsInBounds = new Vector3[2] {
            CoordConvert.RDtoUnity(new Vector3(30000, 34000, 0)),
            CoordConvert.RDtoUnity(new Vector3(23000, 23000, 0))
         };

        var shouldReturnTrue = TileCombineUtility.IsAnyVertexWithinConfigBounds(vertArrayWithVertInBounds);
        var shouldReturnFalse = TileCombineUtility.IsAnyVertexWithinConfigBounds(vertArrayWithWithoutVertsInBounds);

        Assert.AreEqual(true, shouldReturnTrue);
        Assert.AreEqual(false, shouldReturnFalse);


    }
}
