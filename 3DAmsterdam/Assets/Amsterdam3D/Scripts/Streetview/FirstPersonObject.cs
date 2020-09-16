using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

    public class FirstPersonObject:MonoBehaviour
    {

    CameraManager manager;

    public bool placed = false;

    private void Awake()
    {
        manager = CameraManager.instance;
        manager.OnGodViewModeEvent += EnableObject;
    }

    private void EnableObject() 
    {
        gameObject.SetActive(true);
    }
    
    private void OnMouseDown()
    {
        if (placed)
        {
            manager.FirstPersonMode(transform.position, transform.rotation);
            gameObject.SetActive(false);
        }
    }

    private void OnMouseEnter()
    {
        
    }
}
