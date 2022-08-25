using ConvertCoordinates;
using Netherlands3D;
using Netherlands3D.Cameras;
using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[KeepSaveDataOnReset]
public class HTMLInitSaveData : SaveDataContainer
{
    public string SessionId;
    public string Street;
    public string City;
    public string HouseNumber;
    public string HouseNumberAddition;
    public string ZipCode;
    public bool HasFile;
    public Vector3RD RDPosition;
    public string BagId;
    public string BlobId;
    public string ModelId;
    public string ModelVersionId;
    public string Date;
    public bool IsUserFeedback;
    public bool HasSubmitted;
    public bool IsMonument;
    public bool IsBeschermd;
    public bool SnapToWall;
    public bool Add3DModel;    
}

public class T3DInit : MonoBehaviour, IUniqueService
{
    public bool IsEditMode { get; private set; } = true;
   
    public HTMLInitSaveData HTMLData = null;

    public Netherlands3D.Rendering.RenderSettings RenderSettings;

    private void Awake()
    {
        HTMLData = new HTMLInitSaveData();
    }

    void Start()
    {
        ToggleQuality(true);
    }

    void GotoPosition(Vector3RD position)
    {
        Vector3 cameraOffsetForTargetLocation = new Vector3(0, 38, 0);
        ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position = CoordConvert.RDtoUnity(position) + cameraOffsetForTargetLocation;
        ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.LookAt(CoordConvert.RDtoUnity(position), Vector3.up);
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

        StartCoroutine(ServiceLocator.GetService<MetadataLoader>().GetCityJsonBag(HTMLData.BagId));
        StartCoroutine(ServiceLocator.GetService<MetadataLoader>().GetCityJsonBagBoundingBox(HTMLData.RDPosition.x, HTMLData.RDPosition.y, HTMLData.BagId));

        ServiceLocator.GetService<MetadataLoader>().RequestBuildingData(HTMLData.RDPosition, HTMLData.BagId);
    }

    private void ToggleQuality(bool ishigh)
    {
        //postProcessingEffects
        RenderSettings.TogglePostEffects(ishigh);

        //antiAliasing
        RenderSettings.ToggleAA(ishigh);

        //ambientOcclusion
        RenderSettings.ToggleAO(ishigh);
    }
}
