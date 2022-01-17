using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.Settings;
using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T3DInit : MonoBehaviour
{
    private string cameraPositionKey;
    private SaveableVector3RD cameraPosition;

    private string urlBagId; //if the loaded bagId is different from the url bag id, a new session should be started.
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

    private string blobIdKey;
    private SaveableString blobId;
    public string BlobId => blobId.Value;

    public Vector3RD PositionRD;

    public bool IsUserFeedback => isUserFeedback.Value;

    public bool IsEditMode { get; private set; } = true;

    public static T3DInit Instance;

    public TileVisualizer TileVisualizer;

    public Netherlands3D.Rendering.RenderSettings RenderSettings;

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

        blobIdKey = GetType().ToString() + ".blobId";// + nameof(bagId);
        blobId = new SaveableString(blobIdKey);
    }

    void Start()
    {
        if (!SessionSaver.LoadPreviousSession)
            LoadBuilding();

        ToggleQuality(true);

    }

    private bool qualityToggle = false;

#if UNITY_EDITOR
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadBuilding();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            qualityToggle = !qualityToggle;

            //realtimeReflections
            RenderSettings.ToggleReflections(qualityToggle);

            //postProcessingEffects
            RenderSettings.TogglePostEffects(qualityToggle);

            //antiAliasing
            RenderSettings.ToggleAA(qualityToggle);

            //ambientOcclusion
            RenderSettings.ToggleAO(qualityToggle);
        }
    }
#endif

    void GotoPosition(Vector3RD position)
    {
        Vector3 cameraOffsetForTargetLocation = new Vector3(0, 38, 0);
        CameraModeChanger.Instance.ActiveCamera.transform.position = CoordConvert.RDtoUnity(position) + cameraOffsetForTargetLocation;
        CameraModeChanger.Instance.ActiveCamera.transform.LookAt(CoordConvert.RDtoUnity(position), Vector3.up);
    }

    private void SetPositionAndIdForEditor()
    {
        PositionRD = DebugSettings.PositionRD;
        //PositionRD = new Vector3RD(138350.607, 455582.274, 0); //Stadhouderslaan 79 Utrecht
        //PositionRD = new Vector3RD(137383.174, 454037.042, 0); //Hertestraat 15 utrecht
        //PositionRD = new Vector3RD(137837.926, 452307.472, 0); //Cataloni? 5 Utrecht
        //PositionRD = new Vector3RD(136795.424, 455821.827, 0); //Domplein 24 Utrecht
        //PositionRD = new Vector3RD(136932.03, 454272.937, 0); // measurement error building: 3523AA, 10

        if (PositionRD.Equals(new Vector3RD(0, 0, 0))) return;
        cameraPosition.SetValue(PositionRD);

        urlBagId = DebugSettings.BagId;
        //urlBagId = "0344100000021804"; //Stadhouderslaan 79 Utrecht
        //urlBagId = "0344100000068320";//Hertestraat 15 utrecht
        //urlBagId = "0344100000052214";//Cataloni? 5 Utrecht
        //urlBagId = "0344100000035416";// measurement error building : 3523AA, 10

        uploadedModel.SetValue(DebugSettings.UploadedModel);
        bimModelId.SetValue(DebugSettings.BimModelId);
        bimModelVersionId.SetValue(DebugSettings.BimModelVersionId);
        isUserFeedback.SetValue(DebugSettings.IsUserFeedback);
        IsEditMode = DebugSettings.IsEditMode;
        //blobId.SetValue(Application.absoluteURL.GetUrlParamValue("blobid"));
    }

    private void CheckURLForPositionAndId()
    {
        PositionRD = Application.absoluteURL.GetRDCoordinateByUrl();
        if (PositionRD.Equals(new Vector3RD(0, 0, 0))) return;

        cameraPosition.SetValue(PositionRD);
        urlBagId = Application.absoluteURL.GetUrlParamValue("id");
        //bagId.SetValue(urlBagId);

        uploadedModel.SetValue(Application.absoluteURL.GetUrlParamBool("hasfile"));
        bimModelId.SetValue(Application.absoluteURL.GetUrlParamValue("modelid"));
        bimModelVersionId.SetValue(Application.absoluteURL.GetUrlParamValue("versionid"));
        isUserFeedback.SetValue(Application.absoluteURL.GetUrlParamBool("isuserfeedback"));
        IsEditMode = Application.absoluteURL.GetUrlParamBool("iseditmode");
        blobId.SetValue(Application.absoluteURL.GetUrlParamValue("blobid"));
    }

    public void LoadBuilding()
    {
        //wait until the end of the frame and then load the building. this is needed to ensure all SaveableVariables are correctly loaded before using them.
        StartCoroutine(GoToBuildingAtEndOfFrame());
    }

    private IEnumerator GoToBuildingAtEndOfFrame()
    {
        yield return null; //wait a frame
#if !UNITY_EDITOR
        CheckURLForPositionAndId();
#else
        SetPositionAndIdForEditor();
#endif

        //print(urlBagId == bagId.Value);
        if (bagId.Value != urlBagId) //if the loaded id is not the same as the url id, a new session should be started.
        {
            SessionSaver.ClearAllSaveData();
            SessionSaver.LoadPreviousSession = false;
        }

        bagId.SetValue(urlBagId); //overwrite the saved id with the url id
        GotoPosition(cameraPosition.Value);

        yield return StartCoroutine(TileVisualizer.LoadTile(PositionRD.x, PositionRD.y, BagId));
        MetadataLoader.Instance.RequestBuildingData(cameraPosition.Value, bagId.Value);
    }

    private void ToggleQuality(bool ishigh)
    {
        //realtimeReflections
        RenderSettings.ToggleReflections(ishigh);

        //postProcessingEffects
        RenderSettings.TogglePostEffects(ishigh);

        //antiAliasing
        RenderSettings.ToggleAA(ishigh);

        //ambientOcclusion
        RenderSettings.ToggleAO(ishigh);
    }
}
