using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GodviewButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CameraManager.instance.OnFirstPersonModeEvent += EnableObject;
        CameraManager.instance.OnGodViewModeEvent += DisableObject;
        gameObject.SetActive(false);
        Button button = GetComponent<Button>();
        button.onClick.AddListener(CameraManager.instance.GodViewMode);
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
}
