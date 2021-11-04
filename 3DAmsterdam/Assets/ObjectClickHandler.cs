using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;

public class ObjectClickHandler : MonoBehaviour
{
    private static float maxDistanceTraveledWithMouse = 10f;
    private static Vector3 clickStartPosition;
    private static List<Collider> clickColliders = new List<Collider>();
    //private static bool clickStarted;

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
            return allowClickOnNothing && clickColliders.Count == 0;//return true if clicked on nothing
        }
        //print("failed all paths");
        return false;
    }

    public static bool GetClickOnObject(bool allowClickOnNothing, int layerMask = Physics.DefaultRaycastLayers)
    {
        return GetClickOnObject(allowClickOnNothing, out var hitCol, layerMask);
    }
}
