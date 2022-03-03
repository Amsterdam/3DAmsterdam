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

    Vector3 buildingCenter;
    

    void Start()
    {
        //MetadataLoader.Instance.AdresUitgebreidLoaded += OnAdresUitgebreidLoaded;
        MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;
        BuildingMeshGenerator.Instance.BuildingDataProcessed += OnBuildingDataProcessed;
        MetadataLoader.Instance.BuildingOutlineLoaded += OnBuildingOutlineLoaded;

        SessionSaver.Loader.LoadingCompleted += Loader_LoadingCompleted;
    }

    private void Loader_LoadingCompleted(bool loadSucceeded)
    {
        OnAdresUitgebreidLoaded();
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

    private void OnAdresUitgebreidLoaded()
    {
        var buildingPosition = new SaveableVector3RD(HTMLKeys.RD_POSITION_KEY);
        buildingcenter = new Vector2RD(buildingPosition.Value.x, buildingPosition.Value.y);
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

        buildingCenter = CoordConvert.RDtoUnity(buildingcenter.Value);
        var perceelCenter = CoordConvert.RDtoUnity(perceelcenter.Value);
        var cameraoffset = (perceelCenter - buildingCenter).normalized * (buildingRadius + CameraDistance);

        var camera = CameraModeChanger.Instance.ActiveCamera;
        camera.transform.position = new Vector3(buildingCenter.x + cameraoffset.x, groundLevel.Value + CameraHeight, buildingCenter.z + cameraoffset.z);

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

