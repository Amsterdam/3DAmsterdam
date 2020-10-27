using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Amsterdam3D.CameraMotion;
using System.Linq;
using Assets.Amsterdam3D.Scripts.Camera;
using UnityEngine.Events;

public class BoxSelect : MonoBehaviour
{
    // Start is called before the first frame update
    private Pointer pointer;
    [SerializeField]
    private RectTransform selectionBox;
    public GameObject BuildingContainer;
    Vector2 startPos;

    [SerializeField]
    private LayerMask layerMask;
    Vector3 startPosWorld;

    private List<string> testList = new List<string>();

    private RayCastBehaviour raycastBehaviour;
    

    private bool inBoxSelect;

    private Bounds currentBounds;

    public UnityEvent onSelection;
    
    
    public Bounds GetCurrentSelection()
    {

        return currentBounds;
    }
    void Start()
    {
        pointer = FindObjectOfType<Pointer>();
        raycastBehaviour = FindObjectOfType<RayCastBehaviour>();
        inBoxSelect = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (!inBoxSelect)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                
                if (raycastBehaviour.RayCast(out startPosWorld))
                {
                    startPos = Input.mousePosition;
                    selectionBox.gameObject.SetActive(true);
                    inBoxSelect = true;
                }

            }
        }
        else 
        {
            selectionBox.sizeDelta = new Vector2(Input.mousePosition.x - startPos.x, Input.mousePosition.y - startPos.y);
            selectionBox.position = startPos + new Vector2((Input.mousePosition.x - startPos.x) / 2, (Input.mousePosition.y - startPos.y) / 2);
            if (Input.GetKeyDown(KeyCode.H)) 
            {
                selectionBox.gameObject.SetActive(false);
                inBoxSelect = false;
                Vector3 currentWorldPos;
                raycastBehaviour.RayCast(out currentWorldPos);
                Vector2 currentMousePos = Input.mousePosition;
                Vector3 min = new Vector3();
                Vector3 max = new Vector3();
                if (currentWorldPos.z < startPosWorld.z)
                {
                    min.z = currentWorldPos.z;
                    max.z = startPosWorld.z;

                }
                else 
                {
                    min.z = startPosWorld.z;
                    max.z = currentWorldPos.z;
                }

                if (currentWorldPos.x < startPosWorld.x)
                {
                    min.x = currentWorldPos.x;
                    max.x = startPosWorld.x;
                }
                else 
                {
                    min.x = startPosWorld.x;
                    max.x = currentWorldPos.x;
                }
                Debug.Log("Min: " + min + " Max: " + max);
                currentBounds.min = min;
                currentBounds.max = max;
                onSelection?.Invoke();
                //StartCoroutine(GetAllBagIDsInRange(min, max, (value) => { BuildingContainer.GetComponent<LayerSystem.Layer>().Hide(value); }));
            }
        }
    }





}
