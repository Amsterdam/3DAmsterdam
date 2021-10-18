using Netherlands3D.Cameras;
using UnityEngine;
using Netherlands3D.T3D.Uitbouw;

public class RotateCamera : MonoBehaviour
{
    private Camera mycam;
    public float MinCameraHeight = 2;
    public float RotationSpeed = 5;

    void Start()
    {
        mycam = CameraModeChanger.Instance.ActiveCamera;        
    }

    private void Update()
    {
        var xaxis = Input.GetAxis("Mouse X");
        var yaxis = Input.GetAxis("Mouse Y");
        
        if (Input.GetMouseButton(0))
        {
            RotateAround(xaxis, yaxis);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            var moveSpeed = Mathf.Sqrt(mycam.transform.position.y) * 1.3f;
            var newpos = mycam.transform.position + mycam.transform.forward.normalized * (Input.mouseScrollDelta.y * moveSpeed);

            if (newpos.y < MinCameraHeight) newpos.y = 2;
            mycam.transform.position = newpos;
        }
    }


    void RotateAround(float xaxis, float yaxis)
    {
        var previousPosition = mycam.transform.position;
        var previousRotation = mycam.transform.rotation;

        if (Uitbouw.Instance == null) return;

        mycam.transform.RotateAround(Uitbouw.Instance.CenterPoint, Vector3.up, xaxis * RotationSpeed);
        mycam.transform.RotateAround(Uitbouw.Instance.CenterPoint, mycam.transform.right, -yaxis * RotationSpeed);

        if (mycam.transform.position.y < MinCameraHeight)
        {
            mycam.transform.position = previousPosition;
            mycam.transform.rotation = previousRotation;
        }
    }
}
