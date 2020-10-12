using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amsterdam3D.CameraMotion;
public class SaveCameraPositionButton : MonoBehaviour
{
    private StreetViewSpawnObject objSpawner;
    void Start()
    {
        //todo, make these event handlers all a seperate monobehaviour
        CameraModeChanger.Instance.OnFirstPersonModeEvent += EnableObject;
        CameraModeChanger.Instance.OnGodViewModeEvent += DisableObject;
        gameObject.SetActive(false);
        Button button = GetComponent<Button>();
        objSpawner = FindObjectOfType<StreetViewSpawnObject>();
        button.onClick.AddListener(OnClick);
    }

    public void EnableObject()
    {
        gameObject.SetActive(true);
    }

    public void DisableObject()
    {
        gameObject.SetActive(false);
    }

    public void OnClick() 
    {
        Vector3 pos = CameraModeChanger.Instance.ActiveCamera.transform.position;
        Quaternion rot = CameraModeChanger.Instance.ActiveCamera.transform.rotation;
        objSpawner.SpawnFirstPersonAtPosition(pos, rot);
    }
}
