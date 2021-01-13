using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amsterdam3D.CameraMotion;
using UnityEngine.EventSystems;

public class StreetViewButton : MonoBehaviour
{
    private Button firstPersonPrefabButton;

    private StreetViewSpawnObject spawnObject;
    void Start()
    {
        //Note: Should this be in a button class? Or should there be some abstraction layer
        spawnObject = FindObjectOfType<StreetViewSpawnObject>();
        firstPersonPrefabButton = GetComponent<Button>();
        firstPersonPrefabButton.onClick.AddListener(SpawnFirstPersonPrefab);
        CameraModeChanger.Instance.OnFirstPersonModeEvent += DisableObject;
        CameraModeChanger.Instance.OnGodViewModeEvent += EnableObject;
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