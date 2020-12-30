using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Amsterdam3D.JavascriptConnection;
using System.Collections.Generic;

public class StreetViewCamera : MonoBehaviour, ICameraControls
{
    private Vector2 rotation = new Vector2(0, 0);
    public float speed = 3;

    public float objectDistance = 1;
    public Vector3 objectOffset;

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
    private List<GameObject> fireworkPrefabs;

    [SerializeField]
    private List<GameObject> randomRocketPrefab;

    private int usedPrefab1 = 0;
    private int usedPrefab2 = 0;
    private int usedPrefab3 = 0;
    private FireworkType currentType;

    private GameObject[] firework1ObjectPool = new GameObject[20];
    private GameObject[] firework2ObjectPool = new GameObject[20];
    private GameObject[] firework3ObjectPool = new GameObject[20];

    private GameObject currentPrefab;

    public enum FireworkType 
    {
        Missle,Rocket,Missle2
    }
    
    
    private void OnEnable()
    {
        cameraComponent = GetComponent<Camera>();
        if (!inMenus)
        {
            interfaceLayers.SetActive(false);
            mainMenu.SetActive(false);
        }

        for (int i = 0; i < firework1ObjectPool.Length; i++) 
        {
            firework1ObjectPool[i] = Instantiate(fireworkPrefabs[0]);
            firework1ObjectPool[i].SetActive(false);
        }

        for (int i = 0; i < firework2ObjectPool.Length; i++)
        {
            firework2ObjectPool[i] = Instantiate(randomRocketPrefab[UnityEngine.Random.Range(0, randomRocketPrefab.Count)]);
            firework2ObjectPool[i].SetActive(false);
        }

        for (int i = 0; i < firework3ObjectPool.Length; i++)
        {
            firework3ObjectPool[i] = Instantiate(fireworkPrefabs[2]);
            firework3ObjectPool[i].SetActive(false);
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


            if (Input.GetMouseButtonDown(0))
            {
                if (placing)
                {
                    placing = false;
                    currentPrefab.GetComponent<FireworkAnimationScript>().EnableScript(this.transform);
                    currentPrefab.GetComponentInChildren<Rigidbody>().isKinematic = false;
                    currentPrefab.GetComponentInChildren<Rigidbody>().useGravity = true;

                }

                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Locked;

                }


            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EnableMenus();

            }


            if (placing)
            {
                Debug.Log("Placing");
                Vector2 pos = new Vector2(Screen.width / 2, Screen.height / 2);
                Vector3 worldPos = cameraComponent.ScreenToWorldPoint(pos);

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    placing = true;
                    DespawnObject();
                    currentPrefab = firework1ObjectPool[usedPrefab1 % 19];
                    usedPrefab1++;
                    currentType = FireworkType.Missle;
                    currentPrefab.GetComponent<Rigidbody>().isKinematic = true;
                    currentPrefab.transform.rotation = Quaternion.Euler(Vector3.zero);
                    currentPrefab.transform.position = GetMousePositionInWorld();
                    currentPrefab.SetActive(true);
                    currentPrefab.GetComponent<FireworkAnimationScript>().PickupScript();
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    placing = true;
                    DespawnObject();
                    currentPrefab = firework2ObjectPool[usedPrefab2 % 19];
                    usedPrefab2++;
                    currentType = FireworkType.Rocket;
                    currentPrefab.GetComponent<Rigidbody>().isKinematic = true;
                    currentPrefab.transform.rotation = Quaternion.Euler(Vector3.zero);
                    currentPrefab.transform.position = GetMousePositionInWorld();
                    currentPrefab.SetActive(true);
                    currentPrefab.GetComponent<FireworkAnimationScript>().PickupScript();
                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    placing = true;
                    DespawnObject();
                    currentPrefab = firework3ObjectPool[usedPrefab3 % 19];
                    currentPrefab.GetComponent<Rigidbody>().isKinematic = true;
                    usedPrefab3++;
                    currentType = FireworkType.Missle2;
                    currentPrefab.SetActive(true);
                    currentPrefab.GetComponent<FireworkAnimationScript>().PickupScript();
                }

            }

           

            if (!placing)
            {

                // lots of double code but I guess this is only used once anyway
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    placing = true;
                    currentPrefab = firework1ObjectPool[usedPrefab1 % 19];
                    usedPrefab1++;
                    currentType = FireworkType.Missle;
                    currentPrefab.GetComponent<Rigidbody>().isKinematic = true;
                    currentPrefab.transform.rotation = Quaternion.Euler(Vector3.zero);
                    currentPrefab.transform.position = GetMousePositionInWorld();
                    currentPrefab.SetActive(true);
                    currentPrefab.GetComponent<FireworkAnimationScript>().PickupScript();
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    placing = true;
                    currentPrefab = firework2ObjectPool[usedPrefab2 % 19];
                    usedPrefab2++;
                    currentType = FireworkType.Rocket;
                    currentPrefab.GetComponent<Rigidbody>().isKinematic = true;
                    currentPrefab.transform.rotation = Quaternion.Euler(Vector3.zero);
                    currentPrefab.transform.position = GetMousePositionInWorld();
                    currentPrefab.SetActive(true);
                    currentPrefab.GetComponent<FireworkAnimationScript>().PickupScript();
                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    placing = true;
                    currentPrefab = firework3ObjectPool[usedPrefab3 % 19];
                    usedPrefab3++;
                    currentType = FireworkType.Missle2;
                    currentPrefab.GetComponent<Rigidbody>().isKinematic = true;
                    currentPrefab.transform.rotation = Quaternion.Euler(Vector3.zero);
                    currentPrefab.transform.position = GetMousePositionInWorld();
                    currentPrefab.SetActive(true);
                    currentPrefab.GetComponent<FireworkAnimationScript>().PickupScript();
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

    private void LateUpdate()
    {
        if (placing)
        {
            currentPrefab.transform.position = transform.position + (transform.forward * (objectDistance) + objectOffset);
            currentPrefab.transform.eulerAngles = new Vector3(currentPrefab.transform.eulerAngles.x, transform.rotation.eulerAngles.y, currentPrefab.transform.eulerAngles.z);
        }
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

    public void SpawnPrefab(int type) 
    {
        placing = true;
        if (type == 2)
        {
            currentPrefab = firework2ObjectPool[usedPrefab2 % 19];
            usedPrefab2++;
        }
        else if (type == 3)
        {
            currentPrefab = firework3ObjectPool[usedPrefab3 % 19];
            usedPrefab3++;
        }

        else 
        {
            currentPrefab = firework1ObjectPool[usedPrefab1 % 19];
            usedPrefab1++;
        }

        currentPrefab.GetComponent<Rigidbody>().isKinematic = true;
        currentPrefab.transform.rotation = Quaternion.Euler(Vector3.zero);
        currentType = FireworkType.Missle;
        currentPrefab.transform.position = GetMousePositionInWorld();
        currentPrefab.SetActive(true);
        currentPrefab.GetComponent<FireworkAnimationScript>().PickupScript();
    }

    private void DespawnObject() 
    {
        if (currentType == FireworkType.Missle)
        {
            usedPrefab1--;
            firework1ObjectPool[usedPrefab1 % 19].SetActive(false);
        }

        else if (currentType == FireworkType.Rocket)
        {
            usedPrefab2--;
            firework2ObjectPool[usedPrefab2 % 19].SetActive(false);
        }

        else if (currentType == FireworkType.Missle2) 
        {
            usedPrefab3--;
            firework3ObjectPool[usedPrefab3 % 19].SetActive(false);
        }
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
