using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SaveCameraPositionButton : MonoBehaviour
{
    // Start is called before the first frame update
    StreetViewSpawnObject objSpawner;
    void Start()
    {
        //todo, make these event handlers all a seperate monobehaviour
        CameraManager.instance.OnFirstPersonModeEvent += EnableObject;
        CameraManager.instance.OnGodViewModeEvent += DisableObject;
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
        Vector3 pos = CameraManager.instance.currentCameraComponent.transform.position;
        Quaternion rot = CameraManager.instance.currentCameraComponent.transform.rotation;
        objSpawner.SpawnFirstPersonAtPosition(pos, rot);
    }
}
