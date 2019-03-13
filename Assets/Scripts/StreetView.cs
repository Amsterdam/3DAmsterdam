using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetView : MonoBehaviour
{
    private bool moveToStreet = false;
    private Vector3 endPos = new Vector3(0, 1, 0);

    public Camera cam;

    public float speed;

    private void Start()
    {
    }

    private void LateUpdate()
    {
        if (moveToStreet) cam.transform.position = Vector3.Lerp(cam.transform.position, endPos, speed * Time.deltaTime);

        if (new Vector3(cam.transform.position.x, (int)cam.transform.position.y, cam.transform.position.z) == endPos)
        {
            moveToStreet = false;
        }
    }

    public void StreetCam()
    {
        moveToStreet = true;
    }
}
