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

    private string bagIdKey;
    private SaveableString bagId;
    public string BagId => bagId.Value;

    private string uploadedModelKey;
    private SaveableBool uploadedModel;
    public bool UploadedModel => uploadedModel.Value;

    private string bimModelIdKey;
    public SaveableString bimModelId;
    public string BimModelId => bimModelId.Value;

    private string bimModelVersionIdKey;
    public SaveableString bimModelVersionId;
    public string BimModelVersionId => bimModelVersionId.Value;

    private string isUserFeedbackKey;
    private SaveableBool isUserFeedback;
    public bool IsUserFeedback => isUserFeedback.Value;

    public bool IsEditMode { get; private set; } = true;

    public static T3DInit Instance;

    private void Awake()
    {
        Instance = this;
        InitializeSaveableVariables();
    }

    private void InitializeSaveableVariables()
    {
        uploadedModelKey = GetType().ToString() + ".uploadedModel";
        uploadedModel = new SaveableBool(uploadedModelKey);

        cameraPositionKey = GetType().ToString() + ".cameraPosition";// + nameof(cameraPosition);
        cameraPosition = new SaveableVector3RD(cameraPositionKey);
        //print("loaded pos:" + cameraPosition.Value);

        bagIdKey = GetType().ToString() + ".bagId";// + nameof(bagId);
        bagId = new SaveableString(bagIdKey);
        //print("loaded id : " + bagId.Value);

        bimModelIdKey = GetType().ToString() + ".bimModelId";// + nameof(bagId);
        bimModelId = new SaveableString(bimModelIdKey);

        bimModelVersionIdKey = GetType().ToString() + ".bimModelVersionId";// + nameof(bagId);
        bimModelVersionId = new SaveableString(bimModelVersionIdKey);

        isUserFeedbackKey = GetType().ToString() + ".isUserFeedback";// + nameof(bagId);
        isUserFeedback = new SaveableBool(isUserFeedbackKey);
    }

    void Start()
    {
#if !UNITY_EDITOR
        if(!SessionSaver.LoadPreviousSession)
            CheckURLForPositionAndId();
#endif
    }


    private void Update()
    {

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
        {
            uploadedModel.SetValue( Input.GetKey(KeyCode.LeftShift));
            bimModelId.SetValue("61a57eba0a6448f8eaacf9e9");
            bimModelVersionId.SetValue("1");

            GoToTestBuilding();
        }
#endif
    }

    private void GoToTestBuilding()
    {
        var pos = new Vector3RD(138350.607, 455582.274, 0); //Stadhouderslaan 79 Utrecht
        //var pos = new Vector3RD(137383.174, 454037.042, 0); //Hertestraat 15 utrecht
        //var pos = new Vector3RD(137837.926, 452307.472, 0); //Cataloni? 5 Utrecht
        //var pos = new Vector3RD(136795.424, 455821.827, 0); //Domplein 24 Utrecht

        //var pos = new Vector3RD(136932.03, 454272.937, 0); // measurement error building: 3523AA, 10
        cameraPosition.SetValue(pos);
        GotoPosition(pos);

        bagId.SetValue("0344100000021804");
        //bagId.SetValue("0344100000068320");
        //bagId.SetValue("0344100000052214");

        //bagId.SetValue("0344100000035416");// measurement error building : 3523AA, 10

        MetadataLoader.Instance.RequestBuildingData(pos, bagId.Value);

        //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000021804")); //Stadhouderslaan 79 Utrecht, 3583JE
        //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000068320")); //Hertestraat 15 utrecht 3582EP
        //StartCoroutine(PerceelRenderer.Instance.RequestBuildingData(pos, "0344100000052214")); //Cataloni? 5 Utrecht utrecht 3524KX
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

        uploadedModel.SetValue(Application.absoluteURL.GetUrlParamBool("hasfile"));
        bimModelId.SetValue(Application.absoluteURL.GetUrlParamValue("modelId"));
        bimModelVersionId.SetValue(Application.absoluteURL.GetUrlParamValue("versionId"));
        isUserFeedback.SetValue(Application.absoluteURL.GetUrlParamBool("isUserFeedback"));
        IsEditMode = Application.absoluteURL.GetUrlParamBool("IsEditMode");

        GotoPosition(rd);
        MetadataLoader.Instance.RequestBuildingData(rd, bagId.Value);
    }

    public void LoadBuilding()
    {
        //InitializeSaveableVariables();
        //cameraPosition.Load();
        //bagId.Load();

        //print(cameraPosition.Value);

        //wait until the end of the frame and then load the building. this is needed to ensure all SaveableVariables are correctly loaded before using them.
        StartCoroutine( GoToBuildingAtEndOfFrame());
    }

    private IEnumerator GoToBuildingAtEndOfFrame()
    {
        yield return null; //wait a frame

        var pos = cameraPosition.Value;
        cameraPosition.SetValue(pos);
        GotoPosition(pos);
        //print(pos + "_" + bagId.Value);
        MetadataLoader.Instance.RequestBuildingData(pos, bagId.Value);
    }
}
