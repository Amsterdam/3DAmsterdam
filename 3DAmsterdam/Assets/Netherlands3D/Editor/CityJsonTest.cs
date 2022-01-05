using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Netherlands3D.AssetGeneration.CityJSON;
using Netherlands3D;

public class CityJsonTest : MonoBehaviour
{

    [Test]
    public void ParseCityJsonFromFile()
    {
        Config.activeConfiguration = new ConfigurationFile();        
        Config.activeConfiguration.RelativeCenterRD.x = 10;
        Config.activeConfiguration.RelativeCenterRD.y = 20;


        var filepath = @"F:\T3D\CityJson stuff\data\";

        var cityModel = new CityModel(filepath, "61ae0794bca82a123496d257.txt", true, true);

        var vertcount = cityModel.vertices.Count;

        Assert.That(vertcount > 0);


        var createBuildingSurface  = new CreateBuildingSurface();

        var origin = new ConvertCoordinates.Vector3RD();

        GameObject gam = createBuildingSurface.CreateMesh(cityModel, origin);
        




    }
}
