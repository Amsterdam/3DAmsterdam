using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.IO;
using UnityEngine.TestTools;

public class UrlParametersUnitTest 
{
    [UnityTest]
    public IEnumerator TestReadURLParameters()
    {
        var urlParameterTriggersObject = new GameObject().AddComponent<URLParameterTriggers>();
        var parametersAndValues = urlParameterTriggersObject.ReadURLParameters("https://3d.amsterdam.nl/web/app/index.html?T=OPENBARE_TOILETTEN&C=52.357800,4.899740&Z=13&B=52.404827658407925,5.064525604248048,52.31068263374237,4.734935760498048");
        yield return new WaitForEndOfFrame();

        //We should be able to retrieve the C parameters string value properly:
        Assert.AreEqual(parametersAndValues["C"], "52.357800,4.899740");
    }
}
