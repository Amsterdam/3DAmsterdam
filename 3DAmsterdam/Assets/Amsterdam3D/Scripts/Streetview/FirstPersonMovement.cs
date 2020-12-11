using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.UI;
using UnityEngine.InputSystem;
public class FirstPersonMovement : MonoBehaviour
{
    [SerializeField]
    private float groundOffset;

    [SerializeField]
    private LayerMask layerMaskGround;

    [SerializeField]
    private LayerMask layerMaskCustom;

    private Vector3 velocity;
    private bool isgrounded;
    private BoxCollider referenceCollider;

    [SerializeField]
    private float moveSpeed = 2;

    [SerializeField]
    private float runspeed = 4;

    private bool inMenus = false;

    private Ray ray;

    private RaycastHit hit;


    public InputActionAsset actionAsset;

    private InputAction moveAction;
    private UnityEngine.InputSystem.InputActionMap actionMap;

    void Start()
    {
        referenceCollider = GetComponent<BoxCollider>();

    }

    public void EnableMenusMovement()
    {
        inMenus = true;
    }

    void Update()
    {
        if (!inMenus)
        {
            CheckPhysics();
            CheckInput();
            //TODO: Refactor this to have some more global menu state, now each class is checking for menu state seperately 
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
        //Find grounding
        var playerCenter = transform.TransformPoint(new Vector3(referenceCollider.center.x, referenceCollider.center.y, referenceCollider.center.z));
        var rayGround = new Ray(playerCenter + Vector3.up*100.0f, Vector3.down);
        var rayCustom = new Ray(playerCenter, Vector3.down);

        //First check for custom colliders from center of player. Next, check for grounding above or under the player.
        if (Physics.Raycast(rayCustom, out hit, 1000.0f, layerMaskCustom.value) || Physics.Raycast(rayGround, out hit, 1000.0f, layerMaskGround.value))
        {
            //If we have ground to fall to, and we are not grounded, fall!
            isgrounded = Vector3.Distance(hit.point, playerCenter) <= groundOffset;

            if (!isgrounded)
            {
                if (hit.point.y < playerCenter.y)
                {
                    //Fall down
                    velocity -= -Vector3.up * Physics.gravity.y * Time.deltaTime;
                }
                else{
                    //Move player up
                    velocity += -Vector3.up * Physics.gravity.y * Time.deltaTime;
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + groundOffset, transform.position.z);
                velocity = new Vector3(velocity.x, 0, velocity.z);
            }
        }
        else
        {
            //We are not grounded, but have no ground to fall to. keep floating.
            isgrounded = false;
            this.transform.position = new Vector3(this.transform.position.x, Constants.ZERO_GROUND_LEVEL_Y + 1.8f, this.transform.position.z);
            velocity = new Vector3(velocity.x, 0, velocity.z);
        }
    }

    private void CheckInput() 
    {
        Vector2 value = moveAction.ReadValue<Vector2>();
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity.x = value.x * runspeed;
            velocity.z = value.y * runspeed;
        }
        else 
        {    
            velocity.x = value.x * moveSpeed;
            velocity.z = value.y * moveSpeed;
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
        if (!inMenus)
        {
            transform.position += velocity * Time.deltaTime;
        }
    }
}
