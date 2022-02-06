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

    Vector2RD? perceelcenter = null;
    Vector2RD? buildingcenter = null;
    float perceelRadius;
    float? groundLevel;
    bool cameraIsSet = false;

    Vector3 buildingpos;

    BuildingMeshGenerator thebuilding;

    void Start()
    {
        MetadataLoader.Instance.AdresUitgebreidLoaded += OnAdresUitgebreidLoaded;
        MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;
        BuildingMeshGenerator.Instance.BuildingDataProcessed += OnBuildingDataProcessed;
    }

    bool lookatUitbouw = false;

    private void Update()
    {
        if (lookatUitbouw == false && RestrictionChecker.ActiveUitbouw != null)
        {
            var camera = CameraModeChanger.Instance.ActiveCamera;
            camera.transform.LookAt(RestrictionChecker.ActiveUitbouw.CenterPoint);
            lookatUitbouw = true;
        }
    }

    private void OnBuildingDataProcessed(BuildingMeshGenerator building)
    {
        thebuilding = building;
        groundLevel = building.GroundLevel;
        CheckSetCameraposition();
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        perceelcenter = args.PerceelCenter;
        perceelRadius = args.PerceelRadius;
        CheckSetCameraposition();
        // if (cameraIsSet == false && buildingcenter != null) SetCameraposition();
    }

    private void OnAdresUitgebreidLoaded(object source, AdresUitgebreidDataEventArgs args)
    {
        buildingcenter = args.Coordinate;
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

        buildingpos = CoordConvert.RDtoUnity(buildingcenter.Value);
        var perceelpos = CoordConvert.RDtoUnity(perceelcenter.Value);

        var cameraoffset = (perceelpos - buildingpos).normalized * (perceelRadius + CameraDistance);

        var camera = CameraModeChanger.Instance.ActiveCamera;
        camera.transform.position = new Vector3(perceelpos.x + cameraoffset.x, groundLevel.Value + CameraHeight, perceelpos.z + cameraoffset.z);

        if (RestrictionChecker.ActiveUitbouw != null)
        {
            camera.transform.LookAt(RestrictionChecker.ActiveUitbouw.CenterPoint);
        }
        else if (RestrictionChecker.ActiveBuilding)
        {
            camera.transform.LookAt(RestrictionChecker.ActiveBuilding.BuildingCenter);
        }

        //camera.transform.LookAt(buildingpos);
    }
}

