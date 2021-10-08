using System.Collections;
using System.Collections.Generic;
using ConvertCoordinates;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class UitbouwMeasurement : MonoBehaviour
    {
        [SerializeField]
        private GameObject measurementLine;

        private Uitbouw uitbouw;
        [SerializeField]
        private BuildingMeshGenerator building; //todo: quick and dirty reference

        private List<BuildingMeasuring> lines = new List<BuildingMeasuring>();


        private void Awake()
        {
            uitbouw = GetComponent<Uitbouw>();
        }

        private void Start()
        {
            DrawNewMeasurement();
            DrawNewMeasurement();
        }

        void DrawNewMeasurement()
        {
            var lineObject = Instantiate(measurementLine);
            var measuring = lineObject.GetComponent<BuildingMeasuring>();
            measuring.DistanceInputOverride += Measuring_DistanceInputOverride;
            lines.Add(measuring);
        }

        private void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
        {
            //var axis = (source.LinePoints[1].transform.position - source.LinePoints[0].transform.position).normalized;
            uitbouw.transform.position += direction * delta;
        }

        private void Update()
        {
            TryDrawLine(0, uitbouw.LeftCorner, uitbouw.LeftCornerPlane);
            TryDrawLine(1, uitbouw.RightCorner, uitbouw.RightCornerPlane);
        }

        private void TryDrawLine(int lineIndex, Vector3 point, Plane pointPlane)
        {
            bool cornerFound = GetMeasurementCorner(pointPlane, out Vector3 corner);
            lines[lineIndex].gameObject.SetActive(cornerFound);
            if (cornerFound)
            {
                lines[lineIndex].SetLinePosition(point, corner);
            }
        }

        bool GetMeasurementCorner(Plane uitbouwWall, out Vector3 coord)
        {
            coord = new Vector3();
            if (building.AbsoluteBuildingCorners == null)
            {
                return false;
            }

            var coPlanarCorners = GetCoplanarCorners(building.AbsoluteBuildingCorners, uitbouw.SnapWall, 0.1f);

            var smallestDst = Mathf.Infinity;
            var cornerIndex = -1;
            for (int i = 0; i < coPlanarCorners.Length; i++)
            {
                var dst = uitbouwWall.GetDistanceToPoint(coPlanarCorners[i]);
                if (dst > 0 && dst < smallestDst)
                {
                    smallestDst = dst;
                    cornerIndex = i;
                }
            }

            if (cornerIndex > -1)
            {
                coord = coPlanarCorners[cornerIndex];
                return true;
            }
            return false;
        }

        private Vector3[] GetCoplanarCorners(Vector3[] buildingCorners, Plane snapWall, float tolerance)
        {
            var corners = new List<Vector3>();
            foreach (var corner in buildingCorners)
            {
                if (Mathf.Abs(snapWall.GetDistanceToPoint(corner)) < tolerance)
                {
                    corners.Add(corner);
                }
            }
            return corners.ToArray();
        }
    }
}
