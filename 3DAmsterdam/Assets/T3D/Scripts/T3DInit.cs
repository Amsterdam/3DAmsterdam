using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T3DInit : MonoBehaviour
{
    private string cameraPositionKey;
    private SaveableVector3RD cameraPosition;
    private SaveableString bagId;
    private string bagIdKey;

    public static T3DInit Instance;

    private void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        cameraPositionKey = GetType().ToString() + ".cameraPosition";// + nameof(cameraPosition);
        cameraPosition = new SaveableVector3RD(cameraPositionKey);
        print("loaded pos:" + cameraPosition.Value);

        bagIdKey = GetType().ToString() + ".bagId";// + nameof(bagId);
        bagId = new SaveableString(bagIdKey);
        print("loaded id : " + bagId.Value);

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
            MetadataLoader.Instance.BimModelId = "61a57eba0a6448f8eaacf9e9";
            MetadataLoader.Instance.BimModelVersionId = "1";

            //TODO add usecase with blobId (Sketchup)

            GoToTestBuilding();
        }
#endif
    }

    private void GoToTestBuilding()
    {
        var pos = new Vector3RD(138350.607, 455582.274, 0); //Stadhouderslaan 79 Utrecht
                                                            //var pos = new Vector3RD(137383.174, 454037.042, 0); //Hertestraat 15 utrecht
                                                            //var pos = new Vector3RD(137837.926, 452307.472, 0); //Catalonië 5 Utrecht
                                                            //var pos = new Vector3RD(136795.424, 455821.827, 0); //Domplein 24 Utrecht
        cameraPosition.SetValue(pos);
        GotoPosition(pos);

        bagId.SetValue("0344100000021804");
        //bagId.SetValue("0344100000021804");
        //bagId.SetValue("0344100000052214");

        MetadataLoader.Instance.PositionRD = pos;
        MetadataLoader.Instance.RequestBuildingData(pos, bagId.Value);

        //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000021804")); //Stadhouderslaan 79 Utrecht, 3583JE
        //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000068320")); //Hertestraat 15 utrecht 3582EP
        //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000052214")); //Catalonië 5 Utrecht utrecht 3524KX
    }

    void GotoPosition(Vector3RD position)
    {
        Vector3 cameraOffsetForTargetLocation = new Vector3(0, 38, 0);
        CameraModeChanger.Instance.ActiveCamera.transform.position = CoordConvert.RDtoUnity(position) + cameraOffsetForTargetLocation;
        CameraModeChanger.Instance.ActiveCamera.transform.LookAt(CoordConvert.RDtoUnity(position), Vector3.up);
    }

    private void CheckURLForPositionAndId()
    {
        var rd = Application.absoluteURL.GetRDCoordinateByUrl();
        if (rd.Equals(new Vector3RD(0, 0, 0))) return;

        cameraPosition.SetValue(rd);
        bagId.SetValue(Application.absoluteURL.GetUrlParamValue("id"));

        MetadataLoader.Instance.PositionRD = rd;

        MetadataLoader.Instance.UploadedModel = Application.absoluteURL.GetUrlParamBool("hasfile");                
        MetadataLoader.Instance.BimModelId = Application.absoluteURL.GetUrlParamValue("modelId");
        MetadataLoader.Instance.BimModelVersionId = Application.absoluteURL.GetUrlParamValue("versionId");
        MetadataLoader.Instance.BlobId = Application.absoluteURL.GetUrlParamValue("blobId");

        GotoPosition(rd);
        MetadataLoader.Instance.RequestBuildingData(rd, bagId.Value);
    }

    public void LoadBuilding()
    {
        var pos = cameraPosition.Value;
        GotoPosition(pos);
        print(pos + "_" + bagId.Value);
        MetadataLoader.Instance.RequestBuildingData(pos, bagId.Value);
    }
}
