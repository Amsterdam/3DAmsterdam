using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netherlands3D.Utilities
{

    public class CameraMover : MonoBehaviour
    {
        Vector3 startpos;
        Plane _floor = new Plane(Vector3.up, 0);
        Vector3 _pointOnFloor;

        public Vector3 euler;
        public float MinCameraHeight = 2;
        public float RotationSpeed = 100;
        public float FlySpeed = 100;

        bool _mouseClicked;

        private void Update()
        {
            HandleGamepadAndArrows();

            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool israyonfloor = _floor.Raycast(ray, out distance);

            if (israyonfloor == false) return;

            _pointOnFloor = ray.GetPoint(distance);

            var xaxis = Input.GetAxis("Mouse X");
            var yaxis = Input.GetAxis("Mouse Y");

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                _mouseClicked = true;
                startpos = _pointOnFloor;
            }

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            {
                _mouseClicked = false;
            }

            if (Input.GetMouseButton(0) && _mouseClicked == false)
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftControl) == false && Input.GetMouseButton(0))
            {
                HandleMove();
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0))
            {
                transform.eulerAngles += new Vector3(-yaxis, xaxis, 0);
            }
            else if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                RotateAround(xaxis, yaxis);
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                var moveSpeed = Mathf.Sqrt(transform.position.y) * 1.3f;
                var newpos = transform.position + ray.direction.normalized * (Input.mouseScrollDelta.y * moveSpeed);

                if (newpos.y < MinCameraHeight) newpos.y = 2;
                transform.position = newpos;
            }

        }

        void HandleGamepadAndArrows()
        {
            var verticalAxis = Input.GetAxis("Vertical");
            var pos = transform.position + transform.forward * verticalAxis * Time.deltaTime * FlySpeed;
            if (pos.y < MinCameraHeight) pos.y = MinCameraHeight;
            transform.position = pos;

            var horizontalAxis = Input.GetAxis("Horizontal");
            var posx = transform.localPosition + (transform.right * horizontalAxis * Time.deltaTime * FlySpeed);
            transform.localPosition = posx;
        }

        void HandleMove()
        {
            transform.position -= new Vector3(_pointOnFloor.x - startpos.x, 0, _pointOnFloor.z - startpos.z);
        }

        void RotateAround(float xaxis, float yaxis)
        {
            var previousPosition = transform.position;
            var previousRotation = transform.rotation;

            transform.RotateAround(startpos, Vector3.up, xaxis);
            transform.RotateAround(startpos, transform.right, -yaxis);

            euler = transform.eulerAngles;

            if (transform.position.y < MinCameraHeight)
            {
                transform.position = previousPosition;
                transform.rotation = previousRotation;
            }
        }
    }

}
