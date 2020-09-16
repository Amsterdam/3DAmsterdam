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
    }

    private void OnMouseDown()
    {
        if (placed)
        {
            manager.FirstPersonMode(transform.position, transform.rotation);
        }
    }

    private void OnMouseEnter()
    {
        
    }
}
