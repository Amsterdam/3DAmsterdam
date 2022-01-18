using ConvertCoordinates;
using Netherlands3D.Cameras;
using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionCamera: MonoBehaviour
{
    public float CameraHeight = 10;

    Vector2RD? perceelcenter = null;
    Vector2RD? buildingcenter = null;
    bool cameraIsSet = false;

    void Start()
    {
        MetadataLoader.Instance.AdresUitgebreidLoaded += OnAdresUitgebreidLoaded;
        MetadataLoader.Instance.PerceelDataLoaded += OnPerceelDataLoaded;
    }

    private void OnPerceelDataLoaded(object source, PerceelDataEventArgs args)
    {
        perceelcenter = args.PerceelnummerPlaatscoordinaat;
        if (cameraIsSet == false && buildingcenter != null) SetCameraposition();
    }

    private void OnAdresUitgebreidLoaded(object source, AdresUitgebreidDataEventArgs args)
    {
        buildingcenter = args.Coordinate;
        if (cameraIsSet == false && perceelcenter != null) SetCameraposition();        
    }

    void SetCameraposition()
    {
        cameraIsSet = true;

        var buildingpos = CoordConvert.RDtoUnity(buildingcenter.Value);
        var perceelpos = CoordConvert.RDtoUnity(perceelcenter.Value);

        var cameraoffset = (perceelpos - buildingpos).normalized * 10;

        var camera = CameraModeChanger.Instance.ActiveCamera;

        camera.transform.position = new Vector3(perceelpos.x + cameraoffset.x, CameraHeight, perceelpos.z + cameraoffset.z);
        camera.transform.LookAt(buildingpos);
    }
}

