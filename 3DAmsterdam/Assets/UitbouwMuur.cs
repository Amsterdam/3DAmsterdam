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
            if (Input.GetKeyDown(KeyCode.K) && gameObject.name == "Front")
                SetActive(true);

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

                uitbouw.UpdateDimensions();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {

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