using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
public class FirstPersonMovement : MonoBehaviour
{

    // Use this for initialization



    [SerializeField]
    float groundOffset;


    [SerializeField]
    Vector3 velocity;

    [SerializeField]
    bool isgrounded;

    [SerializeField]
    float rayDistance = 0.1f;

    [SerializeField]
    BoxCollider col;

    [SerializeField]
    float gravity = -9.81f;

    [SerializeField]
    float moveSpeed = 1;

    [SerializeField]
    float runspeed = 3;

    private bool inMenus = false;
    void Start()
    {
        col = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {



        if (!inMenus)
        {
            CheckPhysics();
            CheckInput();


            //TODO: Refactor this to have some more global menu state, now each class is checking for menu state seperately 
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                inMenus = true;
            }
        }

        else 
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) 
            {
                inMenus = false;
            }
        }

     




    }


    private void CheckPhysics() 
    {
        if (!isgrounded)
        {
            velocity -= -Vector3.up * gravity * Time.deltaTime;



        }

        if (velocity.y <= 0)
        {
            Vector3 center = transform.TransformPoint(new Vector3(col.center.x, col.center.y, col.center.z));
            Ray ray = new Ray(center, -Vector3.up);
            rayDistance = 0.1f - (velocity.y * Time.deltaTime) + groundOffset;
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.blue, 10);
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + groundOffset, transform.position.z);
                velocity = new Vector3(velocity.x, 0, velocity.z);
                isgrounded = true;
            }

            else
            {
                isgrounded = false;
            }
        }
    }

    private void CheckInput() 
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity.x = Input.GetAxis("Horizontal") * runspeed;
            velocity.z = Input.GetAxis("Vertical") * runspeed;
        }

        else 
        {
            
            
            velocity.x = Input.GetAxis("Horizontal") * moveSpeed;
            velocity.z = Input.GetAxis("Vertical") * moveSpeed;
        }

        if (isgrounded)
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = 5;
                isgrounded = false;
            }
        }

        // rotate velocity 
        velocity = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * velocity;

    }
    
    private void LateUpdate()
    {
        transform.position += velocity * Time.deltaTime;
    }
}
