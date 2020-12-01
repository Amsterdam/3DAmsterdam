using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Amsterdam3D.JavascriptConnection;
using UnityEngine.InputSystem.Interactions;

public class StreetViewCamera : MonoBehaviour, ICameraControls
{
    private Vector2 rotation = new Vector2(0, 0);
    public float speed = 3;

    private bool inMenus = false;

    [SerializeField]
    private GameObject MainMenu;

    [SerializeField]
    private GameObject Layers;

    private Camera camera;

    private Ray ray;
    private RaycastHit hit;


    public InputActionAsset actionAsset;
    
    private InputAction cameraMoveAction;
    private UnityEngine.InputSystem.InputActionMap actionMap;

    private void OnEnable()
    {
       actionMap =  actionAsset.FindActionMap("StreetView");
        cameraMoveAction = actionMap.FindAction("Look");
        camera = GetComponent<Camera>();
        if (!inMenus)
        {
            Layers.SetActive(false);
            MainMenu.SetActive(false);
        }
    }
    private void OnDisable()
    {
        inMenus = false;
    }

    public void EnableMenus() 
    {
        inMenus = true;
        Layers.SetActive(true);
        MainMenu.SetActive(true);
        //actionMap.Disable();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

    }

    public void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion rotation) 
    {
        if (targetLocation.y < Constants.ZERO_GROUND_LEVEL_Y+1.8f) 
        {
            targetLocation.y = Constants.ZERO_GROUND_LEVEL_Y + 1.81f;
        }
        
        transform.position = targetLocation;
        transform.rotation = rotation;
        Vector2 rotationEuler = rotation.eulerAngles;
        if (rotationEuler.x > 180) 
        {
            rotationEuler.x -= 360f;
        }

        if (rotationEuler.x < -180) 
        {
            rotationEuler.x += 360f;
        }
        this.rotation = rotationEuler;
    }

    void Update()
    {
        if (!inMenus)
        {

            var cameraRotation = cameraMoveAction.ReadValue<Vector2>();

            rotation.y += cameraRotation.x * speed;
            rotation.x += -cameraRotation.y * speed;
            rotation.x = ClampAngle(rotation.x, -90, 90);
            transform.eulerAngles = (Vector2)rotation;
            

            if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked) 
            {
                JavascriptMethodCaller.LockCursor();
            }
        }
        else 
        {

            if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject()) 
            {
                Cursor.visible = false;
                JavascriptMethodCaller.LockCursor();
                Layers.SetActive(false);
                MainMenu.SetActive(false);
                inMenus = false;
                actionMap.Enable();
            }
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

    public float GetNormalizedCameraHeight()
    {
        return Mathf.InverseLerp(1.8f, 2500, transform.position.y);
    }

    public float GetCameraHeight()
    {
        return transform.position.y;
    }
    public void OnRotation(Quaternion rotation)
    {
        Vector2 rotationEuler = rotation.eulerAngles;
        if (rotationEuler.x > 180)
        {
            rotationEuler.x -= 360f;
        }

        if (rotationEuler.x < -180)
        {
            rotationEuler.x += 360f;
        }

        this.rotation = rotationEuler;
    }

    public Vector3 GetMousePositionInWorld()
    {
        ray = camera.ScreenPointToRay(Input.mousePosition);
        float distance = 99;
        if (Physics.Raycast(ray, out hit, distance))
        {
            return hit.point;
        }
        else 
        {
            // return end of mouse ray if nothing collides
            return ray.origin + ray.direction * (distance / 10);
        }
    }

	public void SetNormalizedCameraHeight(float height)
	{
		//TODO: Determine if we want to expose the height slider.
	}


    public void InputTest(InputAction.CallbackContext context) 
    {

        if (context.performed)
        {
            if (context.interaction is HoldInteraction)
            {
                Debug.Log("Hold Interaction performed");
            }
        }

        
    }
}
