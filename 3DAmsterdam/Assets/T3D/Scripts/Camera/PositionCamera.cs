using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionCamera: MonoBehaviour
{
    public float CameraHeight = 10;
    public float CameraDistance = 10;
    public float LookatBuildingHeight = 2;

    Vector2RD? perceelcenter = null;
    Vector2RD? buildingcenter = null;
    
    float buildingRadius;
    
    float? groundLevel;
    bool cameraIsSet = false;    

    void OnEnable()
    {
        MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;
        BuildingMeshGenerator.Instance.BuildingDataProcessed += OnBuildingDataProcessed;
        MetadataLoader.Instance.BuildingOutlineLoaded += OnBuildingOutlineLoaded;

        SessionSaver.Loader.LoadingCompleted += Loader_LoadingCompleted;
    }

    void OnDisable()
    {
        MetadataLoader.Instance.PerceelDataLoaded -= OnPerceelDataLoaded;
        BuildingMeshGenerator.Instance.BuildingDataProcessed -= OnBuildingDataProcessed;
        MetadataLoader.Instance.BuildingOutlineLoaded -= OnBuildingOutlineLoaded;

        SessionSaver.Loader.LoadingCompleted -= Loader_LoadingCompleted;
    }

    private void Loader_LoadingCompleted(bool loadSucceeded)
    {
        SetCameraPosition();
    }

    private void OnBuildingOutlineLoaded(object source, BuildingOutlineEventArgs args)
    {
        buildingRadius = args.Radius;
    }

    private void OnBuildingDataProcessed(BuildingMeshGenerator building)
    {        
        groundLevel = building.GroundLevel;        
        CheckSetCameraposition();
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        perceelcenter = args.Center;        
        CheckSetCameraposition();
        // if (cameraIsSet == false && buildingcenter != null) SetCameraposition();
    }

    private void SetCameraPosition()
    {
        var buildingPosition = T3DInit.HTMLData.RDPosition;
        buildingcenter = new Vector2RD(buildingPosition.x, buildingPosition.y);
        CheckSetCameraposition();
        //if (cameraIsSet == false && perceelcenter != null) SetCameraposition();        
    }

    void CheckSetCameraposition()
    {
        if (cameraIsSet) return;
        if (perceelcenter == null) return;
        if (buildingcenter == null) return;
        if (groundLevel == null) return;

        cameraIsSet = true;

        var buildingCenterUnity = CoordConvert.RDtoUnity(buildingcenter.Value);
        var perceelCenter = CoordConvert.RDtoUnity(perceelcenter.Value);
        var cameraoffset = (perceelCenter - buildingCenterUnity).normalized * (buildingRadius + CameraDistance);

        var camera = CameraModeChanger.Instance.ActiveCamera;
        camera.transform.position = new Vector3(buildingCenterUnity.x + cameraoffset.x, groundLevel.Value + CameraHeight, buildingCenterUnity.z + cameraoffset.z);

        if (RestrictionChecker.ActiveUitbouw != null)
        {
            camera.transform.LookAt(RestrictionChecker.ActiveUitbouw.CenterPoint);
        }
        else if (RestrictionChecker.ActiveBuilding)
        {
            var lookatpos = new Vector3(RestrictionChecker.ActiveBuilding.BuildingCenter.x, groundLevel.Value + LookatBuildingHeight, RestrictionChecker.ActiveBuilding.BuildingCenter.z);
            camera.transform.LookAt(lookatpos);
        }

        
    }
}

