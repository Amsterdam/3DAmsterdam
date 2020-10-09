using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amsterdam3D.CameraMotion;
public class StreetViewButton : MonoBehaviour
{
    // Start is called before the first frame update
    private Button firstPersonPrefabButton;

    private StreetViewSpawnObject spawnObject;
    void Start()
    {
        //Note: Should this be in a button class? Or should there be some abstraction layer
        spawnObject = FindObjectOfType<StreetViewSpawnObject>();
        firstPersonPrefabButton = GetComponent<Button>();
        firstPersonPrefabButton.onClick.AddListener(SpawnFirstPersonPrefab);
        CameraModeChanger.instance.OnFirstPersonModeEvent += DisableObject;
        CameraModeChanger.instance.OnGodViewModeEvent += EnableObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnFirstPersonPrefab() 
    {
        spawnObject.SpawnFirstPersonPrefab();
    }

    private void DisableObject() 
    {
        gameObject.SetActive(false);
    }

    private void EnableObject() 
    {
        gameObject.SetActive(true);
    }
}
