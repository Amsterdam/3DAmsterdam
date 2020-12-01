using UnityEngine;
using System.Collections;
using Amsterdam3D.CameraMotion;

namespace Assets.Amsterdam3D.Scripts.Camera
{
    public class RayCastBehaviour : MonoBehaviour
    {

        // Use this for initialization

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