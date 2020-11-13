﻿using UnityEngine;
using Amsterdam3D.CameraMotion;

namespace Amsterdam3D.Interface
{
    public class BoxSelect : SelectionTool
    {
        [SerializeField]
        private GameObject selectionBoxPrefab;

        private RectTransform selectionBox;

        private Vector2 startPos;
        private Vector3 startPosWorld;
        private Vector2 newSizeData = new Vector2();

        private RayCastBehaviour raycastBehaviour;

        private bool inBoxSelect;

        [Tooltip("If the graphic contains extra pixels (for maybe a dropshadow) ignore those when calculating the graphic size.")]
        [SerializeField]
        private float selectionGraphicwCorrection = 10.0f;

		public override void EnableTool()
        {
            raycastBehaviour = FindObjectOfType<RayCastBehaviour>();
            GameObject selectionBoxObj = Instantiate(selectionBoxPrefab);
            selectionBox = selectionBoxObj.GetComponent<RectTransform>();
            selectionBox.SetParent(canvas.transform);
            selectionBoxObj.SetActive(false);
            inBoxSelect = false;
            enabled = true;

        }

        private void OnEnable()
        {
            toolType = ToolType.Box;
        }

        public void Update()
        {
            if (enabled)
            {
                if (!inBoxSelect)
                {

                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            startPosWorld = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
                            startPos = Input.mousePosition;
                            selectionBox.position = startPos;

                            selectionBox.gameObject.SetActive(true);
                            inBoxSelect = true;
                        }
                    }
                }
                else
                {
                    Vector2 currentMousePos = Input.mousePosition;
                    selectionBox.sizeDelta = new Vector3(Mathf.Abs((currentMousePos.x - startPos.x)) + selectionGraphicwCorrection, Mathf.Abs(currentMousePos.y - startPos.y) + selectionGraphicwCorrection, 1) / CanvasSettings.canvasScale;
                    //Move anchor to allow drawing negative direction selectionboxes
                    selectionBox.pivot = new Vector2(
                        ((currentMousePos.x - startPos.x) > 0.0) ? 0 : 1,
                        ((currentMousePos.y - startPos.y) > 0.0) ? 0 : 1
                    );

                    if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.LeftShift))
                    {
                        
                        selectionBox.gameObject.SetActive(false);
                        inBoxSelect = false;
                        
                        if (((startPos.x - currentMousePos.x < 10) && (startPos.x - currentMousePos.x > -10)) || ((startPos.y - currentMousePos.y < 10) && (startPos.y - currentMousePos.y > -10))) 
                        {
                            return;
                        }

                        Vector3 currentWorldPos = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
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

                        if (currentWorldPos.y < startPosWorld.y)
                        {
                            min.y = currentWorldPos.y;
                            max.y = startPosWorld.y;
                        }
                        else
                        {
                            min.y = startPosWorld.y;
                            max.y = currentWorldPos.y;
                        }

                        // screen space code
                        Vector2 p2Screen = new Vector2(startPos.x, currentMousePos.y);
                        Vector2 p4Screen = new Vector2(currentMousePos.x, startPos.y);

                        Debug.Log("Min: " + min + " Max: " + max);
                        Vector3 p2;
                        Vector3 p4;
                        if (raycastBehaviour.RayCast(out p2, p2Screen) && raycastBehaviour.RayCast(out p4, p4Screen)) 
                        {
                            Debug.Log("P2: " + p2);
                            Debug.Log("P4: " + p4);
                            DrawBounds(min, p2, max, p4);
                            vertices.Clear();
                            vertices.Add(min);
                            vertices.Add(p2);
                            vertices.Add(max);
                            vertices.Add(p4);
                            onSelectionCompleted?.Invoke();
                        }
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

        private void DrawBounds(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float delay = 100)
        {
            Debug.DrawLine(p1, p2, Color.blue, delay);
            Debug.DrawLine(p2, p3, Color.red, delay);
            Debug.DrawLine(p3, p4, Color.yellow, delay);
            Debug.DrawLine(p4, p1, Color.magenta, delay);
        }
    }
}
