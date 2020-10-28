using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Amsterdam3D.CameraMotion;
using System.Linq;
using Assets.Amsterdam3D.Scripts.Camera;
using UnityEngine.Events;
using Assets.Amsterdam3D.Scripts.SelectionTools;

[CreateAssetMenu]
public class BoxSelect : SelectionTool
{

    [SerializeField]
    private GameObject selectionBoxPrefab;
    private RectTransform selectionBox;

    Vector2 startPos;
    Vector3 startPosWorld;
    Vector2 newSizeData = new Vector2();

    private RayCastBehaviour raycastBehaviour;
    

    private bool inBoxSelect;
    private bool enabled;
    

    public override void EnableTool()
    {
        raycastBehaviour = FindObjectOfType<RayCastBehaviour>();
        GameObject selectionBoxObj = Instantiate(selectionBoxPrefab);
        selectionBox =  selectionBoxObj.GetComponent<RectTransform>();
        selectionBox.SetParent(Canvas.transform);
        selectionBoxObj.SetActive(true);
        inBoxSelect = false;
        enabled = true;

    }

    private void OnEnable()
    {
        toolType = ToolType.Box;
    }

    public override void Update()
    {

        if (enabled)
        {
            if (!inBoxSelect)
            {

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetMouseButtonDown(0))
                    {

                        if (raycastBehaviour.RayCast(out startPosWorld))
                        {
                            startPos = Input.mousePosition;
                            selectionBox.gameObject.SetActive(true);
                            inBoxSelect = true;
                        }

                    }
                }
            }
            else
            {
                selectionBox.sizeDelta = new Vector2(Input.mousePosition.x - startPos.x, Input.mousePosition.y - startPos.y);
                selectionBox.position = startPos + new Vector2((Input.mousePosition.x - startPos.x) / 2, (Input.mousePosition.y - startPos.y) / 2);
                Debug.Log(selectionBox.sizeDelta);
              /*  if (selectionBox.sizeDelta.x < 0) 
                {
                    selectionBox.position = new Vector2((selectionBox.position.x + selectionBox.sizeDelta.x) / 2, selectionBox.position.y);
                    selectionBox.sizeDelta = new Vector2(selectionBox.sizeDelta.x * -1, selectionBox.sizeDelta.y);
                    

                }
                if (selectionBox.sizeDelta.y < 0) 
                {
                    selectionBox.position = new Vector2(selectionBox.position.x, (selectionBox.position.y + selectionBox.sizeDelta.y) / 2);
                    selectionBox.sizeDelta = new Vector2(selectionBox.sizeDelta.x, newSizeData.y = selectionBox.sizeDelta.y * -1);
                } */
                if (Input.GetMouseButtonUp(0))
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
                    bounds.min = min;
                    bounds.max = max;
                    vertexes.Add(min);
                    vertexes.Add(max);
                    onSelectionCompleted?.Invoke();
                }
            }
        }
    }

    public override void DisableTool()
    {
        enabled = false;
        inBoxSelect = false;
        selectionBox.gameObject.SetActive(false);
    }
}
