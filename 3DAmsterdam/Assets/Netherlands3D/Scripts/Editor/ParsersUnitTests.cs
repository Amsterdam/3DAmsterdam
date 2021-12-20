using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using Netherlands3D.Core;
using System;
using System.Linq;
using Netherlands3D;
using Netherlands3D.AssetGeneration;

public class ParserUnitTests 
{
    private string rdInsideAmsterdamBasedOBJ = @"mtllib bag3d_v21050_ams_utr_b0a525f7_stage.3dbag_v21050_ams_utr_b0a525f7_lod22_3d_1787.obj.mtl
v 129376.923 484045.728 -0.090
v 129382.487 484044.438 -0.090
v 129377.049 484045.361 -0.090
o NL.IMBAG.Pand.0363100012201729
f 1 2 3
o NL.IMBAG.Pand.0363100012201728
f 3 2 1
o NL.IMBAG.Pand.0363100012201727
f 2 1 3
o NL.IMBAG.Pand.0363100012201726
f 3 1 2";

    //[Test]
    //public void TestAmsterdamRDBasedObjImport()
    //{
    //    //Amsterdam RD coordinates bounding box config
    //    Config.activeConfiguration = new ConfigurationFile();
    //    Config.activeConfiguration.RelativeCenterRD = new Vector2RD(121000, 487000);
    //    Config.activeConfiguration.BottomLeftRD = new Vector2RD(109000, 474000);
    //    Config.activeConfiguration.TopRightRD = new Vector2RD(141000, 501000);

    //    GameObject gameObject = new GameObject();
    //    var objLoader = gameObject.AddComponent<ObjLoad>();
    //    objLoader.BottomLeftBounds = Config.activeConfiguration.BottomLeftRD;
    //    objLoader.TopRightBounds = Config.activeConfiguration.TopRightRD;
    //    objLoader.IgnoreObjectsOutsideOfBounds = true;
    //    objLoader.MaxSubMeshes = 1;
    //    objLoader.SplitNestedObjects = true;
    //    objLoader.WeldVertices = true;
    //    objLoader.EnableMeshRenderer = false;
    //    objLoader.SetGeometryData(ref rdInsideAmsterdamBasedOBJ);
    //    objLoader.ParseNextObjLines(12);

    //    //Should be equal to 1 + amount of 'o ' object lines found in obj
    //    Assert.AreEqual(5, objLoader.Buffer.NumberOfObjects);
    //}

    [Test]
    public void TileCombinerTest()
    {
        //Very small boundingbox for RD config
        Config.activeConfiguration = new ConfigurationFile();
        Config.activeConfiguration.RelativeCenterRD = new Vector2RD(15000, 15000);
        Config.activeConfiguration.BottomLeftRD = new Vector2RD(10000, 10000);
        Config.activeConfiguration.TopRightRD = new Vector2RD(20000, 20000);

        var vertArrayWithVertInBounds = new Vector3[2] { 
            CoordConvert.RDtoUnity(new Vector3(10001, 10000, 0)),
            CoordConvert.RDtoUnity(new Vector3(20000, 20000, 0))
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
