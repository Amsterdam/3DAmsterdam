using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Cameras;
using Netherlands3D.Settings;
using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public static class HTMLKeys
//{
//    //todo: make $_ a part of the way the saveablevariables' keys are saved

//    public const string SESSION_ID_KEY = "$_session_id";
//    public const string STREET_KEY = "$_street";
//    public const string CITY_KEY = "$_city";
//    public const string HOUSE_NUMBER_KEY = "$_huisnummer";
//    public const string ZIP_CODE_KEY = "$_postcode";
//    public const string DATE_KEY = "$_date";

//    public const string HAS_FILE_KEY = "$_hasfile";
//    public const string RD_POSITION_KEY = "$_rd_position";
//    public const string BAG_ID_KEY = "$_bag_id";
//    public const string BLOB_ID_KEY = "$_blob_id";
//    public const string MODEL_ID_KEY = "$_model_id";
//    public const string MODEL_VERSION_ID_KEY = "$_model_version_id";
//    public const string IS_USER_FEEDBACK_KEY = "$_is_user_feedback";

//    public const string IS_BESCHERMD = "$_isbeschermd";
//    public const string IS_MONUMENT = "$_ismonument";

//    public const string HAS_SUBMITTED_KEY = "$_has_submitted";
//}

public class HTMLInitSaveData : SaveDataContainer
{
    public string SessionId;
    public string Street;
    public string City;
    public string HouseNumber;
    public string HouseNumberAddition;
    public string ZipCode;
    public bool HasFile;
    public Vector3RD RDPosition; //todo: is this serializable in the json parser
    public string BagId; //todo needs to be nullable
    public string BlobId;
    public string ModelId;
    public string ModelVersionId;
    public string Date;
    public bool IsUserFeedback;
    public bool HasSubmitted;
    public bool IsMonument;
    public bool IsBeschermd;
}

public class T3DInit : MonoBehaviour
{
    //private string cameraPositionKey = HTMLKeys.RD_POSITION_KEY;
    //private SaveableVector3RD cameraPosition;

    //private string bagIdKey = HTMLKeys.BAG_ID_KEY;
    //private SaveableString bagId;
    //public string BagId => bagId.Value;

    //private string uploadedModelKey = HTMLKeys.HAS_FILE_KEY;
    //private SaveableBool uploadedModel;
    //public bool UploadedModel => uploadedModel.Value;

    //private string bimModelIdKey = HTMLKeys.MODEL_ID_KEY;
    //public SaveableString bimModelId;
    //public string BimModelId => bimModelId.Value;

    //private string bimModelVersionIdKey = HTMLKeys.MODEL_VERSION_ID_KEY;
    //public SaveableString bimModelVersionId;
    //public string BimModelVersionId => bimModelVersionId.Value;

    //private string isUserFeedbackKey = HTMLKeys.IS_USER_FEEDBACK_KEY;
    //private SaveableBool isUserFeedback;

    //private string blobIdKey = HTMLKeys.BLOB_ID_KEY;
    //private SaveableString blobId;
    //public string BlobId => blobId.Value;

    //public bool IsUserFeedback => isUserFeedback.Value;
    public bool IsEditMode { get; private set; } = true;

    public static T3DInit Instance;

    public TileVisualizer TileVisualizer;

    //private static HTMLInitSaveData data;
    public static HTMLInitSaveData HTMLData = null;
    //{
    //    get
    //    {
    //        if (data == null)
    //        {
    //            data = new HTMLInitSaveData();
    //        }
    //        return data;
    //    }
    //}
    public Netherlands3D.Rendering.RenderSettings RenderSettings;

    private void Awake()
    {
        Instance = this;
        //InitializeSaveableVariables();
        HTMLData = new HTMLInitSaveData();
    }

    //private void InitializeSaveableVariables()
    //{

    //    uploadedModel = new SaveableBool(uploadedModelKey);
    //    cameraPosition = new SaveableVector3RD(cameraPositionKey);
    //    bagId = new SaveableString(bagIdKey);
    //    bimModelId = new SaveableString(bimModelIdKey);
    //    bimModelVersionId = new SaveableString(bimModelVersionIdKey);
    //    isUserFeedback = new SaveableBool(isUserFeedbackKey);
    //    blobId = new SaveableString(blobIdKey);
    //}

    void Start()
    {
        ToggleQuality(true);
    }

    void GotoPosition(Vector3RD position)
    {
        Vector3 cameraOffsetForTargetLocation = new Vector3(0, 38, 0);
        CameraModeChanger.Instance.ActiveCamera.transform.position = CoordConvert.RDtoUnity(position) + cameraOffsetForTargetLocation;
        CameraModeChanger.Instance.ActiveCamera.transform.LookAt(CoordConvert.RDtoUnity(position), Vector3.up);
    }

    public void LoadBuilding()
    {
        //wait until the end of the frame and then load the building. this is needed to ensure all SaveableVariables are correctly loaded before using them.
        StartCoroutine(GoToBuildingAtEndOfFrame());
    }

    private IEnumerator GoToBuildingAtEndOfFrame()
    {
        yield return null; //wait a frame

        //set relative center to cameraposition to avoid floating point precision issues
        Config.activeConfiguration.RelativeCenterRD = new Vector2RD(HTMLData.RDPosition.x, HTMLData.RDPosition.y);

        GotoPosition(HTMLData.RDPosition);
        StartCoroutine(TileVisualizer.LoadTile(HTMLData.RDPosition.x, HTMLData.RDPosition.y, HTMLData.BagId));

        MetadataLoader.Instance.RequestBuildingData(HTMLData.RDPosition, HTMLData.BagId);
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
