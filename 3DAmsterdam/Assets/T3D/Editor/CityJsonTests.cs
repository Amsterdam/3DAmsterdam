using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.Linq;

public class CityJsonTests
{
    List<Vector2> testdata = new List<Vector2>()
    {
        new Vector2(2.6f, 0.2f),//0
        new Vector2(2.7f, 0.2f),//1
        new Vector2(2.7f, 0),//2
        new Vector2(-2.7f, 0),//3
        new Vector2(-2.7f, 0.2f),//4
        new Vector2(-2.6f, 0.2f),//5
        new Vector2(-2.2f, 0.2f),//6
        new Vector2(-0.2f, 0.199f),//7
        new Vector2(-0.2f, 2.3f),//8
        new Vector2(-2.2f, 2.3f),//9
        new Vector2(-2.2f, 0.2f),//10
        new Vector2(-2.6f, 0.2f),//11
        new Vector2(-2.7f, 0.2f),//12
        new Vector2(-2.7f, 2.849f),//13
        new Vector2(-2.6f, 2.849f),//14
        new Vector2(-2.2f, 2.849f),//15
        new Vector2(2.2f, 2.849f),//16
        new Vector2(2.6f, 2.849f),//17
        new Vector2(2.7f, 2.849f),//18
        new Vector2(2.7f, 0.2f),//19
        new Vector2(2.6f, 0.2f),//20
        new Vector2(2.2f, 0.2f),//21
        new Vector2(2.2f, 2.3f),//22
        new Vector2(0.212f, 2.3f),//23
        new Vector2(0.212f, 0.199f),//24
        new Vector2(2.2f, 0.2f)//25
    };


    [Test]
    public void BoundaryHolesTest()
    {
        //testdata should return a list with 3 Vector2 list. One outer polygon and two inner polygons
        var result = CityJsonMeshUtility.GetOuterAndInnerPolygons(testdata);

        foreach (var points in result)
        {
            Debug.Log($"list");
            foreach (var point in points) 
            {
                Debug.Log($"{point.x} {point.y}");
            }
        }

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(16, result[0].Count);

        //var firstInnerPoint1 = result[1][0];
        //Assert.AreEqual(new Vector2(-2.2f, 0.2f), firstInnerPoint1);

        //var secondInnerPoint1 = result[2][0];
        //Assert.AreEqual(new Vector2(2.2f, 0.2f), secondInnerPoint1);

    }

}
