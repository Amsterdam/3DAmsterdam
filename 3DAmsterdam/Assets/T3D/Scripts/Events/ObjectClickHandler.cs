using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;

public class ObjectClickHandler : MonoBehaviour
{
    public static Vector3 MouseDelta
    {
        get
        {
            if (Input.GetMouseButton(0))
                return Input.mousePosition - clickStartPosition;
            return Vector3.zero;
        }
    }

    private static float maxDistanceTraveledWithMouse = 10f;
    private static Vector3 clickStartPosition;
    private static List<Collider> clickColliders = new List<Collider>();
    //private static bool clickStarted;

    private static bool dragStarted = false;
    public static Collider DraggingCollider { get; private set; }

    private void Update()
    {
        Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            clickColliders.Clear();
            //clickStarted = true;
            clickStartPosition = Input.mousePosition;
            var hits = Physics.RaycastAll(ray, Mathf.Infinity);
            for (int i = 0; i < hits.Length; i++)
            {
                clickColliders.Add(hits[i].collider);
            }
            dragStarted = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragStarted = false;
            DraggingCollider = null;
        }
    }

    /// <summary>
    /// Returns if a user has clicked on a raycastable object. Will not return true for UI elements
    /// </summary>
    /// <param name="layerMask">The layermask to filter the click for</param>
    /// <returns></returns>
    public static bool GetClickOnObject(bool allowClickOnNothing, out Collider clickedCollider, int layerMask = Physics.DefaultRaycastLayers)
    {
        clickedCollider = null;
        if (Input.GetMouseButtonUp(0))
        {
            var dist = Input.mousePosition - clickStartPosition;
            if (dist.magnitude > maxDistanceTraveledWithMouse)
            {
                //print("dist exceeded");
                return false;
            }

            //if mouse did not move too much, wait for mouse up and raycast to see if the collider is the same as the one clicked on
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
            {
                if (clickColliders.Contains(hit.collider))
                {
                    clickedCollider = hit.collider;
                    //print("clicked on " + hit.collider);
                    return true;
                }
            }

            bool a = true; // assume there are no clicked colliders
            foreach (var col in clickColliders)
            {
                var isInMask = layerMask == (layerMask | (1 << col.gameObject.layer));
                if (isInMask)
                {
                    //if the collider is in the mask, return false
                    a = false;
                    break;
                }
            }

            return allowClickOnNothing && a;//allowClickOnNothing && clickColliders.Count == 0; //return true if clicked on nothing
        }
        //print("failed all paths");
        return false;
    }

    public static bool GetClickOnObject(bool allowClickOnNothing, int layerMask = Physics.DefaultRaycastLayers)
    {
        return GetClickOnObject(allowClickOnNothing, out var hitCol, layerMask);
    }

    public static bool GetDrag(out Collider draggedCollider, int layerMask = Physics.DefaultRaycastLayers)
    {
        draggedCollider = null;
        if (Input.GetMouseButton(0))
        {
            var dist = Input.mousePosition - clickStartPosition;
            if (dragStarted || dist.magnitude > maxDistanceTraveledWithMouse)
            {
                dragStarted = true;

                draggedCollider = GetColliderUnderMouse(layerMask);
                return true;
            }
        }
        return false;
    }

    private static Collider GetColliderUnderMouse(int layerMask)
    {
        Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
        {
            return hit.collider;
        }

        return null;
    }

    public static bool GetDragOnObject(Collider col, bool allowDragOverOtherCollidersWhenStarted)
    {
        var drag = GetDrag(out var draggedcol);

        if (DraggingCollider == null)
        {
            DraggingCollider = draggedcol;
        }

        if (allowDragOverOtherCollidersWhenStarted)
        {
            return drag && DraggingCollider == col;
        }

        return drag && col == draggedcol;
    }
}
