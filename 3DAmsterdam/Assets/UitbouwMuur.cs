using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    public class UitbouwMuur : MonoBehaviour
    {
        [SerializeField]
        bool isActive = false;

        [SerializeField]
        private UitbouwMuur left;
        [SerializeField]
        private UitbouwMuur right;
        [SerializeField]
        private UitbouwMuur top;
        [SerializeField]
        private UitbouwMuur bottom;
        [SerializeField]
        private UitbouwMuur back;

        private Vector3 oldPosition;
        public Vector3 deltaPosition { get; private set; }

        private Uitbouw uitbouw;

        private void Awake()
        {
            uitbouw = GetComponentInParent<Uitbouw>();
            oldPosition = transform.position;
        }

        private static Vector3 CalculateXYScale(Transform left, Transform right, Transform top, Transform bottom)
        {
            float hDist = Vector3.Distance(left.position, right.position);
            float vDist = Vector3.Distance(top.position, bottom.position);

            return new Vector3(hDist, vDist, 1);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {

                print("front;");
                if (gameObject.name == "Front")
                    SetActive(true);
                else
                    SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (gameObject.name == "Back")
                    SetActive(true);
                else
                    SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (gameObject.name == "Top")
                    SetActive(true);
                else
                    SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (gameObject.name == "Bottom")
                    SetActive(true);
                else
                    SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                if (gameObject.name == "Left")
                    SetActive(true);
                else
                    SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                if (gameObject.name == "Right")
                    SetActive(true);
                else
                    SetActive(false);
            }

            if (isActive) //moving this face
            {
                deltaPosition = transform.position - oldPosition;
                oldPosition = transform.position;

                //back.RecalculatePosition(deltaPosition/2);
                //RecalculatePosition(deltaPosition/2);

                left.RecalculatePosition(deltaPosition/2);
                right.RecalculatePosition(deltaPosition/2);
                top.RecalculatePosition(deltaPosition/2);
                bottom.RecalculatePosition(deltaPosition/2);

                left.RecalculateScale();
                right.RecalculateScale();
                top.RecalculateScale();
                bottom.RecalculateScale();

                //uitbouw.UpdateDimensions();
            }
        }

        public void RecalculatePosition(Vector3 delta)
        {
            transform.position += delta;
        }

        public void RecalculateScale()
        {
            transform.localScale = CalculateXYScale(left.transform, right.transform, top.transform, bottom.transform);
        }

        public void SetActive(bool active)
        {
            oldPosition = transform.position;
            deltaPosition = Vector3.zero;
            isActive = active;
        }
    }
}