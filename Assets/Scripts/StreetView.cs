using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetView : MonoBehaviour
{
    private bool moveToStreet = false;
    private Vector3 startPos;
    private Vector3 endPos = new Vector3(0, 10, 0);

    public Camera cam;

    private void Start()
    {
        startPos = cam.transform.position;
    }

    private void LateUpdate()
    {
        if (moveToStreet) cam.transform.position = Vector3.Lerp(startPos, endPos, Time.deltaTime * 1f);

        //if (Camera.main.transform.position == endPos) moveToStreet = false;
    }

    public void StreetCam()
    {
        moveToStreet = true;
    }
}
