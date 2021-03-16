using UnityEngine;
using System.Collections;
using Netherlands3D.CameraMotion;

namespace Netherlands3D.CameraMotion
{
    public class RayCastBehaviour : MonoBehaviour
    {
        [SerializeField]
        LayerMask layerMask;

       public bool RayCast(out Vector3 hitpos)
        {
            RaycastHit hit;
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 9999, layerMask.value))
            {
                hitpos = hit.point;
                return true;
            }
            hitpos = Vector3.zero;
            return false;
        }


        public bool RayCast(out Vector3 hitpos, Vector2 screenPosition)
        {
            RaycastHit hit;
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out hit, 9999, layerMask.value))
            {
                hitpos = hit.point;
                return true;
            }
            hitpos = Vector3.zero;
            return false;
        }

    }
}