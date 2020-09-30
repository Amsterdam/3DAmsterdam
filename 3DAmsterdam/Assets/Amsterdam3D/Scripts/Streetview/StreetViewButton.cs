using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StreetViewButton : MonoBehaviour
{
    // Start is called before the first frame update
    private Button buttonh;

    private StreetViewSpawnObject spawnObject;
    void Start()
    {
        //Note: Should this be in a button class? Or should there be some abstraction layer
        spawnObject = FindObjectOfType<StreetViewSpawnObject>();
        buttonh = GetComponent<Button>();
        buttonh.onClick.AddListener(h);
        CameraModeChanger.instance.OnFirstPersonModeEvent += DisableObject;
        CameraModeChanger.instance.OnGodViewModeEvent += EnableObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void h() 
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
