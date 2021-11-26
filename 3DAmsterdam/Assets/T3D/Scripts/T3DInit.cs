using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T3DInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR            
            CheckURLForPositionAndId();
#endif
    }

    private void Update()
    {

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            MetadataLoader.Instance.UploadedModel = Input.GetKey(KeyCode.LeftShift);
            
            //do not store auth tokens in GIT
            MetadataLoader.Instance.BimAuthToken = "";
            MetadataLoader.Instance.BimModelId = "619fc7f5190f7ec17bbb6447";
            MetadataLoader.Instance.BimModelVersionId = "1";
            

            var pos = new Vector3RD(138350.607, 455582.274, 0); //Stadhouderslaan 79 Utrecht
                                                                //var pos = new Vector3RD(137383.174, 454037.042, 0); //Hertestraat 15 utrecht
                                                                //var pos = new Vector3RD(137837.926, 452307.472, 0); //Catalonië 5 Utrecht
                                                                //var pos = new Vector3RD(136795.424, 455821.827, 0); //Domplein 24 Utrecht
            GotoPosition(pos);

            MetadataLoader.Instance.RequestBuildingData(pos, "0344100000021804");
            //MetadataLoader.Instance.RequestBuildingData(pos, "0344100000068320");
            //MetadataLoader.Instance.RequestBuildingData(pos, "0344100000052214");
            //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000021804")); //Stadhouderslaan 79 Utrecht, 3583JE
            //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000068320")); //Hertestraat 15 utrecht 3582EP
            //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000052214")); //Catalonië 5 Utrecht utrecht 3524KX
        }
#endif
    }

    private void CheckURLForPositionAndId()
    {

        var rd = Application.absoluteURL.GetRDCoordinateByUrl();
        if (rd.Equals(new Vector3RD(0, 0, 0))) return;

        var id = Application.absoluteURL.GetUrlParamValue("id");

        MetadataLoader.Instance.UploadedModel = Application.absoluteURL.GetUrlParamBool("hasfile");
        MetadataLoader.Instance.BimAuthToken = Application.absoluteURL.GetUrlParamValue("auth");
        MetadataLoader.Instance.BimModelId = Application.absoluteURL.GetUrlParamValue("modelId");
        MetadataLoader.Instance.BimModelVersionId = Application.absoluteURL.GetUrlParamValue("versionId");

        GotoPosition(rd);
        MetadataLoader.Instance.RequestBuildingData(rd, id);
    }

    void GotoPosition(Vector3RD position)
    {
        Vector3 cameraOffsetForTargetLocation = new Vector3(0, 38, 0);
        CameraModeChanger.Instance.ActiveCamera.transform.position = CoordConvert.RDtoUnity(position) + cameraOffsetForTargetLocation;
        CameraModeChanger.Instance.ActiveCamera.transform.LookAt(CoordConvert.RDtoUnity(position), Vector3.up);
    }

}
