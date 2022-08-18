using System.Collections;
using System.Collections.Generic;
using ConvertCoordinates;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public class UitbouwMeasurement : DistanceMeasurement
    {
        private UitbouwBase uitbouw;

        protected override void Awake()
        {
            base.Awake();
            uitbouw = GetComponent<UitbouwBase>();
        }

        protected override void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
        {
            //var axis = (source.LinePoints[1].transform.position - source.LinePoints[0].transform.position).normalized;
            uitbouw.GetComponent<UitbouwMovement>().SetPosition(uitbouw.transform.position + direction * delta);
        }

        protected override void DrawLines()
        {
            TryDrawLine(0, uitbouw.LeftCorner, uitbouw.LeftCornerPlane);
            TryDrawLine(1, uitbouw.RightCorner, uitbouw.RightCornerPlane);
        }

        private bool TryDrawLine(int lineIndex, Vector3 point, Plane pointPlane)
        {
            bool cornerFound = GetMeasurementCorner(pointPlane, out Vector3 corner);
            lines[lineIndex].gameObject.SetActive(cornerFound);
            if (cornerFound)
            {
                lines[lineIndex].SetLinePosition(point, corner);
            }

            return cornerFound;
        }

        private static bool GetMeasurementCorner(Plane uitbouwWall, out Vector3 coord)
        {
            coord = new Vector3();
            if (RestrictionChecker.ActiveBuilding.AbsoluteBuildingCorners == null)
            {
                return false;
            }

            var coPlanarCorners = GetCoplanarCorners(RestrictionChecker.ActiveBuilding.AbsoluteBuildingCorners, RestrictionChecker.ActiveBuilding.SelectedWall.WallPlane, 0.1f);

            var maxDist = 0f;
            var cornerIndex = -1;
            for (int i = 0; i < coPlanarCorners.Length; i++)
            {
                var dst = uitbouwWall.GetDistanceToPoint(coPlanarCorners[i]);
                if (dst > 0 && dst > maxDist)
                {
                    maxDist = dst;
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

        private static Vector3[] GetCoplanarCorners(Vector3[] buildingCorners, Plane snapWall, float tolerance)
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
