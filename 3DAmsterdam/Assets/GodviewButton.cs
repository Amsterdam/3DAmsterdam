using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodviewButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CameraManager.instance.OnFirstPersonModeEvent += EnableObject;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableObject() 
    {
        gameObject.SetActive(true);
    }
}
