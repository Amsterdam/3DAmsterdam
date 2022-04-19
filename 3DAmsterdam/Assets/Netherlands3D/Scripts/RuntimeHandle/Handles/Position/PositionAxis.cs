using Netherlands3D.Cameras;
using UnityEngine;

namespace RuntimeHandle
{
    /**
     * Created by Peter @sHTiF Stefcek 20.10.2020
     */
    public class PositionAxis : HandleBase
    {
        protected Vector3 _startPosition;
        protected Vector3 _axis;
        protected Vector3 _perp;
        protected Plane _plane;
        protected Vector3 _interactionOffset;
        public PositionAxis Initialize(RuntimeTransformHandle p_runtimeHandle, Vector3 p_axis, Vector3 p_perp, Color p_color)
        {
            _parentTransformHandle = p_runtimeHandle;
            _axis = p_axis;
            _perp = p_perp;
            _defaultColor = p_color;
            
            InitializeMaterial();

            transform.SetParent(p_runtimeHandle.transform, false);

            GameObject o = new GameObject();
            o.transform.SetParent(transform, false);
            MeshRenderer mr = o.AddComponent<MeshRenderer>();
            mr.material = _material;
            MeshFilter mf = o.AddComponent<MeshFilter>();
            mf.mesh = MeshUtils.CreateCone(2f, .02f, .02f, 8, 1);
            MeshCollider mc = o.AddComponent<MeshCollider>();
            mc.sharedMesh = MeshUtils.CreateCone(2f, .1f, .2f, 6, 1);
            o.transform.localRotation = Quaternion.FromToRotation(Vector3.up, p_axis);
            o.layer = p_runtimeHandle.raycastLayer;

            o = new GameObject();
            o.transform.SetParent(transform, false);
            mr = o.AddComponent<MeshRenderer>();
            mr.material = _material;
            mf = o.AddComponent<MeshFilter>();
            mf.mesh = MeshUtils.CreateCone(.4f, .2f, .0f, 8, 1);
            mc = o.AddComponent<MeshCollider>();
            o.transform.localRotation = Quaternion.FromToRotation(Vector3.up, _axis);
            o.transform.localPosition = p_axis * 2;
            o.layer = p_runtimeHandle.raycastLayer;

            return this;
        }

        public override void Interact(Vector3 p_previousPosition)
        {            
            Ray ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);

            float d = 0.0f;
            _plane.Raycast(ray, out d);

            Vector3 hitPoint = ray.GetPoint(d);

            Vector3 offset = hitPoint + _interactionOffset - _startPosition;
            Vector3 snapping = _parentTransformHandle.positionSnap;
            float snap = Vector3.Scale(snapping, _axis).magnitude;
            if (snap != 0 && _parentTransformHandle.snappingType == HandleSnappingType.RELATIVE)
            {
                if (snapping.x != 0) offset.x = Mathf.Round(offset.x / snapping.x) * snapping.x;
                if (snapping.y != 0) offset.y = Mathf.Round(offset.y / snapping.y) * snapping.y;
                if (snapping.z != 0) offset.z = Mathf.Round(offset.z / snapping.z) * snapping.z;
            }

            Vector3 position = _startPosition + offset;

            if (snap != 0 && _parentTransformHandle.snappingType == HandleSnappingType.ABSOLUTE)
            {
                if (snapping.x != 0) position.x = _axis.x * Mathf.Round(position.x / snapping.x) * snapping.x;
                if (snapping.y != 0) position.y = _axis.x * Mathf.Round(position.y / snapping.y) * snapping.y;
                if (snapping.x != 0) position.z = _axis.x * Mathf.Round(position.z / snapping.z) * snapping.z;
            }

            Vector3 directionToDragTo = (position - _startPosition).normalized;
            _parentTransformHandle.target.position = _startPosition;
			if(_axis.x > 0)
            {
                _parentTransformHandle.target.Translate(Vector3.right * Vector3.Dot(_parentTransformHandle.target.right, directionToDragTo) * Vector3.Distance(_startPosition, position),Space.Self);
            }
            else if (_axis.y > 0)
            {
                _parentTransformHandle.target.Translate(Vector3.up * Vector3.Dot(_parentTransformHandle.target.up, directionToDragTo) * Vector3.Distance(_startPosition, position), Space.Self);
            }
            else if(_axis.z > 0)
            {
                _parentTransformHandle.target.Translate(Vector3.forward * Vector3.Dot(_parentTransformHandle.target.forward, directionToDragTo) * Vector3.Distance(_startPosition, position), Space.Self);
            }

            base.Interact(p_previousPosition);
        }

        public override void StartInteraction()
        {
            Vector3 rperp = _parentTransformHandle.space == HandleSpace.LOCAL
                ? _parentTransformHandle.target.rotation * _perp
                : _perp;

            _plane = new Plane(rperp, _parentTransformHandle.target.position);

            Ray ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);

            float d = 0.0f;
            _plane.Raycast(ray, out d);

            Vector3 hitPoint = ray.GetPoint(d);
            _startPosition = _parentTransformHandle.target.position;
            _interactionOffset = _startPosition - hitPoint;
        }
    }
}