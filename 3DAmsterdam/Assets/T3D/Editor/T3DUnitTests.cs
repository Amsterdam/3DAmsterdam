using Netherlands3D.Core;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class T3DUnitTests : MonoBehaviour
{

    [Test]
    public void TestReadBinaryMetadata()
    {

        var bytes = File.ReadAllBytes(@"E:\T3D\Buildings-138000_455000.2.2-data.bin");
        var result = BinaryMeshConversion.ReadBinaryMetaData(bytes);


        Assert.That(true);

    }

    [Test]
    public void WriteReadBinaryMetadata()
    {
        ObjectMappingClass omc = ScriptableObject.CreateInstance<ObjectMappingClass>();
        omc.ids = new List<string>();
        omc.ids.Add("1");
        omc.ids.Add("2");
        omc.ids.Add("3");

        omc.vectorMap = new List<int>();
        omc.vectorMap.Add(1);
        omc.vectorMap.Add(2);
        omc.vectorMap.Add(0);

        omc.vectorMap.Add(3);
        omc.vectorMap.Add(4);
        omc.vectorMap.Add(0);

        omc.vectorMap.Add(5);
        omc.vectorMap.Add(6);
        omc.vectorMap.Add(0);

        BinaryMeshConversion.SaveMetadataAsBinaryFile(omc, @"e:\t3d\test.bin");

        var filebytes = File.ReadAllBytes(@"e:\t3d\test.bin");

        var omc2 = BinaryMeshConversion.ReadBinaryMetaData(filebytes);
        Assert.AreEqual("1", omc.ids[0]);
        Assert.AreEqual("2", omc.ids[1]);
        Assert.AreEqual("3", omc.ids[2]);

        Assert.AreEqual(1, omc.vectorMap[0]);
        Assert.AreEqual(2, omc.vectorMap[1]);
        Assert.AreEqual(0, omc.vectorMap[2]);

        Assert.AreEqual(3, omc.vectorMap[3]);
        Assert.AreEqual(4, omc.vectorMap[4]);
        Assert.AreEqual(0, omc.vectorMap[5]);

        Assert.AreEqual(5, omc.vectorMap[6]);
        Assert.AreEqual(6, omc.vectorMap[7]);
        Assert.AreEqual(0, omc.vectorMap[8]);
    }

}
