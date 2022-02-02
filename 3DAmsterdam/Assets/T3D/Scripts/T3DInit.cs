using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.Settings;
using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HTMLKeys
{
    //todo: make $_ a part of the way the saveablevariables' keys are saved

    public const string SESSION_ID_KEY = "$_session_id";
    public const string STREET_KEY = "$_street";
    public const string CITY_KEY = "$_city";
    public const string HOUSE_NUMBER_KEY = "$_huisnummer";
    public const string ZIP_CODE_KEY = "$_postcode";
    public const string DATE_KEY = "$_date";

    public const string HAS_FILE_KEY = "$_hasfile";
    public const string RD_POSITION_KEY = "$_rd_position";
    public const string BAG_ID_KEY = "$_bag_id";
    public const string BLOB_ID_KEY = "$_blob_id";
    public const string MODEL_ID_KEY = "$_model_id";
    public const string MODEL_VERSION_ID_KEY = "$_model_version_id";
    public const string IS_USER_FEEDBACK_KEY = "$_is_user_feedback";
}

public class T3DInit : MonoBehaviour
{
    private string cameraPositionKey = HTMLKeys.RD_POSITION_KEY;
    private SaveableVector3RD cameraPosition;

    private string bagIdKey = HTMLKeys.BAG_ID_KEY;
    private SaveableString bagId;
    public string BagId => bagId.Value;

    private string uploadedModelKey = HTMLKeys.HAS_FILE_KEY;
    private SaveableBool uploadedModel;
    public bool UploadedModel => uploadedModel.Value;

    private string bimModelIdKey = HTMLKeys.MODEL_ID_KEY;
    public SaveableString bimModelId;
    public string BimModelId => bimModelId.Value;

    private string bimModelVersionIdKey = HTMLKeys.MODEL_VERSION_ID_KEY;
    public SaveableString bimModelVersionId;
    public string BimModelVersionId => bimModelVersionId.Value;

    private string isUserFeedbackKey = HTMLKeys.IS_USER_FEEDBACK_KEY;
    private SaveableBool isUserFeedback;

    private string blobIdKey = HTMLKeys.BLOB_ID_KEY;
    private SaveableString blobId;
    public string BlobId => blobId.Value;

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
        uploadedModel = new SaveableBool(uploadedModelKey);
        cameraPosition = new SaveableVector3RD(cameraPositionKey);
        bagId = new SaveableString(bagIdKey);
        bimModelId = new SaveableString(bimModelIdKey);
        bimModelVersionId = new SaveableString(bimModelVersionIdKey);
        isUserFeedback = new SaveableBool(isUserFeedbackKey);
        blobId = new SaveableString(blobIdKey);
    }

    void Start()
    {
        ToggleQuality(true);
    }


#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadBuilding();
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
        var positionRD = DebugSettings.PositionRD;

        if (positionRD.Equals(new Vector3RD(0, 0, 0))) return;
        cameraPosition.SetValue(positionRD);

        bagId.SetValue(DebugSettings.BagId);

        uploadedModel.SetValue(DebugSettings.UploadedModel);
        bimModelId.SetValue(DebugSettings.BimModelId);
        bimModelVersionId.SetValue(DebugSettings.BimModelVersionId);
        isUserFeedback.SetValue(DebugSettings.IsUserFeedback);
        IsEditMode = DebugSettings.IsEditMode;
        //blobId.SetValue(Application.absoluteURL.GetUrlParamValue("blobid"));
    }

    public void LoadBuilding()
    {
        print("loading bag id: " + BagId);

        //wait until the end of the frame and then load the building. this is needed to ensure all SaveableVariables are correctly loaded before using them.
        StartCoroutine(GoToBuildingAtEndOfFrame());
    }

    private IEnumerator GoToBuildingAtEndOfFrame()
    {
        yield return null; //wait a frame

        //CheckURLForPositionAndId("https://opslagt3d.z6.web.core.windows.net/3d/?sessionId=2d9e94f0-58c2-11ec-99a1-4b7435bae68a_637780107091771994.json&isuserfeedback=true&iseditmode=false");
        //CheckURLForPositionAndId("http://localhost:8080/?sessionId=048c5390-5133-11ec-b24c-53d3efea3919&position=138350.607_455582.274&id=0344100000021804&hasfile=false&iseditmode=true");

        //IFC upload
        //CheckURLForPositionAndId("https://opslagt3d.z6.web.core.windows.net/3d/?position=138350.607_455582.274&id=0344100000021804&hasfile=true&modelId=61e6dd19b3622280344bb05d&versionId=1&iseditmode=true");

        var posRD = cameraPosition.Value;
        GotoPosition(posRD);
        StartCoroutine(TileVisualizer.LoadTile(posRD.x, posRD.y, BagId));

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