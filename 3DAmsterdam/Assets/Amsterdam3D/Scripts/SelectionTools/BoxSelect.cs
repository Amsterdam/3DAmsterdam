using UnityEngine;
using Amsterdam3D.CameraMotion;
using System.Collections.Generic;

namespace Amsterdam3D.Interface
{
    public class BoxSelect : SelectionTool
    {
		private const float requiredPixelDragDistance = 10.0f;
		[SerializeField]
        private GameObject selectionBoxPrefab;

        private RectTransform selectionBox;

        private Vector2 startMousePosition;
        private Vector3 startPosWorld;
        private Vector2 newSizeData = new Vector2();

        private RayCastBehaviour raycastBehaviour;

        private bool inBoxSelect;

        [Tooltip("If the graphic contains extra pixels (for maybe a dropshadow) ignore those when calculating the graphic size.")]
        [SerializeField]
        private float selectionGraphicCorrection = 10.0f;

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

		private void OnDrawGizmos()
		{
            foreach (var vert in vertices)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(vert, 10.0f);
            }
            if (vertices.Count < 1) return;

            Gizmos.DrawLine(vertices[0], vertices[1]);
            Gizmos.DrawLine(vertices[1], vertices[2]);
            Gizmos.DrawLine(vertices[2], vertices[3]);
            Gizmos.DrawLine(vertices[3], vertices[0]);
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
                            vertices.Clear();

                            startPosWorld = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
                            startMousePosition = Input.mousePosition;
                            selectionBox.position = startMousePosition;

                            selectionBox.gameObject.SetActive(true);
                            inBoxSelect = true;
                        }
                    }
                }
                else
                {
                    Vector2 currentMousePosition = Input.mousePosition;
                    bool enoughDistanceDragged = Vector3.Distance(startMousePosition, currentMousePosition) > requiredPixelDragDistance;

                    //Resize our visual selection box. Flipping the anchor to allow for negative direction drawn boxes.
                    if(enoughDistanceDragged){ 
                        selectionBox.sizeDelta = new Vector3(Mathf.Abs((currentMousePosition.x - startMousePosition.x)) + selectionGraphicCorrection, Mathf.Abs(currentMousePosition.y - startMousePosition.y) + selectionGraphicCorrection, 1) / CanvasSettings.canvasScale;
                        selectionBox.pivot = new Vector2(
                            ((currentMousePosition.x - startMousePosition.x) > 0.0) ? 0 : 1,
                            ((currentMousePosition.y - startMousePosition.y) > 0.0) ? 0 : 1
                        );
                    }
                    selectionBox.gameObject.SetActive(enoughDistanceDragged);

                    //On release, check our selected area
                    if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.LeftShift))
                    {
                        //Avoid 1 frame flicker when we are enabled again (UI scales are a bit late)
                        selectionBox.sizeDelta = Vector3.zero;

                        //Remove visual bounding box
                        selectionBox.gameObject.SetActive(false);
                        inBoxSelect = false;
                        
                        //Too small selections are ignored, so we bail out and do not invoke a selection
                        if (!enoughDistanceDragged)
                            return;

                        //Our for corners of the bounding box as points on our world plane
                        Vector3 point1 = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld(startMousePosition);
                        Vector3 point2 = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld(startMousePosition + (Vector2.left * (startMousePosition.x - currentMousePosition.x)));
                        Vector3 point3 = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld(currentMousePosition);
                        Vector3 point4 = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld(startMousePosition + (Vector2.down * (startMousePosition.y-currentMousePosition.y)));
                        
                        vertices.AddRange(new List<Vector3>(){ point1,point2,point3,point4 });
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
}
