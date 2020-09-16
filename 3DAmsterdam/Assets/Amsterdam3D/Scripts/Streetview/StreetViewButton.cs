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
        spawnObject = FindObjectOfType<StreetViewSpawnObject>();
        buttonh = GetComponent<Button>();
        buttonh.onClick.AddListener(h);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void h() 
    {
        spawnObject.SpawnFirstPersonPrefab();
    }
}
