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

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

       public bool RayCast(out Vector3 hitpos)
        {
            RaycastHit hit;
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
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