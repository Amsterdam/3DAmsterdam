using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amsterdam3D.CameraMotion;
public class SaveCameraPositionButton : MonoBehaviour
{
    private StreetViewSpawnObject streetViewSpawnObject;
    void Start()
    {
        //todo, make these event handlers all a seperate monobehaviour
        CameraModeChanger.Instance.OnFirstPersonModeEvent += EnableObject;
        CameraModeChanger.Instance.OnGodViewModeEvent += DisableObject;
        gameObject.SetActive(false);
    }

    public void EnableObject()
    {
        gameObject.SetActive(true);
    }

    public void DisableObject()
    {
        gameObject.SetActive(false);
    }

    public void SaveCurrentPosition() 
    {
        Vector3 pos = CameraModeChanger.Instance.ActiveCamera.transform.position;
        Quaternion rot = CameraModeChanger.Instance.ActiveCamera.transform.rotation;
        //streetViewSpawnObject.SpawnFirstPersonAtPosition(pos, rot);
    }
}
