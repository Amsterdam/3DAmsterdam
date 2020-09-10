using UnityEngine;
using System.Collections;



//TODO: Make Class related with CameraControls somehow? (Abstract base class that both god view and first person view inherit from, that functions that need the camera use)
// Or we make a singleton CameraManager that scripts can get the current camera data from
public class FirstPersonMouseLook : MonoBehaviour
{

   
    
    Vector2 rotation = new Vector2(0, 0);
    public float speed = 3;



    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        rotation.y += Input.GetAxis("Mouse X") * speed;
        rotation.x += -Input.GetAxis("Mouse Y") * speed;
        rotation.x = ClampAngle(rotation.x, -90, 90);
        transform.eulerAngles = (Vector2)rotation;


        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
