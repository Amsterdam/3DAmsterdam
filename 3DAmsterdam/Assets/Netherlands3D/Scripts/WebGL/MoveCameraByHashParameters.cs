using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using System.Runtime.InteropServices;

public class MoveCameraByHashParameters : MonoBehaviour
{
    #if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SetHash(string newHash);
    #endif
    [SerializeField] private StringEvent moveCameraEvent;
    [SerializeField] private bool reportCameraLocationBack = true;
    [SerializeField] private float maxUrlUpdateRate = 1.0f;
    private double[] coordinates = new double[6];

    private Vector3 lastCameraPosition;
    private Vector3 lastCameraRotation;

    void Awake()
    {
        moveCameraEvent.AddListenerStarted(MoveCamera);

#if !UNITY_EDITOR && UNITY_WEBGL
        if (reportCameraLocationBack) StartCoroutine(ReportBack());
#endif
    }
#if !UNITY_EDITOR && UNITY_WEBGL
    private IEnumerator ReportBack()
    {
        while(reportCameraLocationBack)
        {
            GetCameraArray();
            SetHash($"{coordinates[0]:F2},{coordinates[1]:F2},{coordinates[2]:F2},{coordinates[3]:F2},{coordinates[4]:F2},{coordinates[5]:F2}");

            yield return new WaitForSeconds(maxUrlUpdateRate);
        }
    }
#endif

    private void GetCameraArray()
    {
        var currentMainCamera = Camera.main;
        if (currentMainCamera.transform.position != lastCameraPosition || currentMainCamera.transform.eulerAngles != lastCameraRotation)
        {
            var rdCoordinates = CoordConvert.UnitytoRD(currentMainCamera.transform.position);

            coordinates[0] = rdCoordinates.x;
            coordinates[1] = rdCoordinates.y;
            coordinates[2] = rdCoordinates.z;
            coordinates[3] = currentMainCamera.transform.eulerAngles.x;
            coordinates[4] = currentMainCamera.transform.eulerAngles.y;
            coordinates[5] = currentMainCamera.transform.eulerAngles.z;
        }
    }

    public void MoveCamera(string hashStringParameter)
    {
        var numbers = hashStringParameter.Split(',');
        if(numbers.Length > 1)
        {
            //Assume first two numbers as position in RD x,y if they are large
            if(double.TryParse(numbers[0], out double x) && double.TryParse(numbers[1], out double y))
            {
                if (x > 100)
                {
                    //Assume RD x,y
                    Debug.Log($"Moving camera to RD: {x},{y}");
                    Camera.main.transform.position = CoordConvert.RDtoUnity(new Vector2RD(x, y));
                }
                else
                {
                    //Assume WGS84 lat,long
                    Debug.Log($"Moving camera to Lat,Long: {x},{y}");
                    Camera.main.transform.position = CoordConvert.WGS84toUnity(y, x);
                }
            }
        }

        if (numbers.Length > 2)
        {
            //Assume 3rd number as height
            if (float.TryParse(numbers[2], out float height))
            {
                Debug.Log($"Moving camera to height: {height}");
                Camera.main.transform.Translate(0, height, 0,Space.World);
            }
        }

        if (numbers.Length > 5)
        {
            //Assume last three numbers are rotation
            if (float.TryParse(numbers[3], out float xRotation) && float.TryParse(numbers[4], out float yRotation) && float.TryParse(numbers[5], out float zRotation))
            {
                Debug.Log($"Setting camera rotation to: {xRotation},{yRotation},{zRotation}");
                Camera.main.transform.eulerAngles = new Vector3(xRotation, yRotation, zRotation);
            }
        }
    }
}
