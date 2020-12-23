using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Amsterdam3D.JavascriptConnection;

public class StreetViewCamera : MonoBehaviour, ICameraControls
{
    private Vector2 rotation = new Vector2(0, 0);
    public float speed = 3;

    private bool inMenus = false;

    [SerializeField]
    private GameObject mainMenu;

    [SerializeField]
    private GameObject interfaceLayers;

    private Camera cameraComponent;

    private Ray ray;
    private RaycastHit hit;

    private bool placing = false;





    [SerializeField]
    private GameObject fireworkPrefab;

    private GameObject currentPrefab;

    private void OnEnable()
    {
        cameraComponent = GetComponent<Camera>();
        if (!inMenus)
        {
            interfaceLayers.SetActive(false);
            mainMenu.SetActive(false);
        }
    }
    private void OnDisable()
    {
        inMenus = false;
    }

    public void EnableMenus() 
    {
        inMenus = true;
        interfaceLayers.SetActive(true);
        mainMenu.SetActive(true);
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
            rotation.y += Input.GetAxis("Mouse X") * speed;
            rotation.x += -Input.GetAxis("Mouse Y") * speed;
            rotation.x = ClampAngle(rotation.x, -90, 90);
            transform.eulerAngles = (Vector2)rotation;
                
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EnableMenus();

            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                placing = true;
                currentPrefab = Instantiate(fireworkPrefab);
                currentPrefab.transform.position = GetMousePositionInWorld();
                currentPrefab.SetActive(true);
            }

            if (placing) 
            {
                Vector2 pos = new Vector2(Screen.width / 2, Screen.height / 2);
                Vector3 worldPos = cameraComponent.ScreenToWorldPoint(pos);

                currentPrefab.transform.position = transform.position + (transform.forward * (10));
                currentPrefab.transform.eulerAngles = new Vector3(currentPrefab.transform.eulerAngles.x, transform.rotation.eulerAngles.y, currentPrefab.transform.eulerAngles.z);
            }

            if (Input.GetMouseButtonDown(0)) 
            {
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    JavascriptMethodCaller.LockCursor();

                }
                if (placing)
                {
                    placing = false;
                    Debug.Log("We here");
                    currentPrefab.GetComponentInChildren<Rigidbody>().isKinematic = false;
                    currentPrefab.GetComponentInChildren<Rigidbody>().useGravity = true;

                }

            }
        }
        else 
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EnableMenus();
            }

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) 
            {
                Cursor.visible = false;
                JavascriptMethodCaller.LockCursor();
                interfaceLayers.SetActive(false);
                mainMenu.SetActive(false);
                inMenus = false;
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

    public Vector3 GetMousePositionInWorld(Vector3 optionalPositionOverride = default)
    {
        var pointerPosition = Input.mousePosition;
        if (optionalPositionOverride != default) pointerPosition = optionalPositionOverride;

        ray = cameraComponent.ScreenPointToRay(pointerPosition);
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
}
