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
        CameraModeChanger.instance.OnFirstPersonModeEvent += EnableObject;
        CameraModeChanger.instance.OnGodViewModeEvent += DisableObject;
        gameObject.SetActive(false);
        Button button = GetComponent<Button>();
        objSpawner = FindObjectOfType<StreetViewSpawnObject>();
        button.onClick.AddListener(OnCLick);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnableObject()
    {
        gameObject.SetActive(true);
    }

    public void DisableObject()
    {
        gameObject.SetActive(false);
    }

    public void OnCLick() 
    {
        Vector3 pos = CameraModeChanger.instance.CurrentCameraComponent.transform.position;
        Quaternion rot = CameraModeChanger.instance.CurrentCameraComponent.transform.rotation;
        objSpawner.SpawnFirstPersonAtPosition(pos, rot);
    }
}
