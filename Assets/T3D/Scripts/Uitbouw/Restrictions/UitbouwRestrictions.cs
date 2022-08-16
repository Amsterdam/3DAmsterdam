using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConvertCoordinates;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.Utilities;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public interface UitbouwRestriction
    {
        public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw);
    }

    public class MonumentRestriction : UitbouwRestriction
    {
        public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            return !building.IsMonument;
        }
    }

    public class BeschermdRestriction : UitbouwRestriction
    {
        public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            return !building.IsBeschermd;
        }
    }

    public class HeightRestriction : UitbouwRestriction
    {
        public static float MaxHeight = 3f;
        public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            var roundedHeight = Mathf.RoundToInt(uitbouw.Height * 100);
            var roundedMaxHeight = Mathf.RoundToInt(MaxHeight * 100);
            return roundedHeight <= roundedMaxHeight;
        }
    }

    public class DepthRestriction : UitbouwRestriction
    {
        public static float MaxDepth = 3f;
        public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            var roundedDepth = Mathf.RoundToInt(uitbouw.Depth * 100);
            var roundedMaxDepth = Mathf.RoundToInt(MaxDepth * 100);
            return roundedDepth <= roundedMaxDepth;
        }
    }

    public class PerceelAreaRestriction : UitbouwRestriction
    {
        public static float MaxAreaPercentage { get { return 33f; } }
        public static float MaxAreaFraction { get { return MaxAreaPercentage / 100f; } }

        public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            var uitbouwArea = uitbouw.Width * uitbouw.Depth;
            var freeArea = perceel.Area - building.Area;
            var percentage = (uitbouwArea / freeArea) * 100;
            return percentage <= MaxAreaPercentage;
        }
    }

    public class PerceelBoundsRestriction : UitbouwRestriction
    {
        public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            return IsInPerceel(uitbouw.GetFootprint(), perceel.Perceel, uitbouw.transform.position);
        }

        public static bool IsInPerceel(Vector2[] uitbouwFootprint, List<Vector2[]> perceel, Vector3 uitbouwPositionOffset)
        {
            if (uitbouwFootprint == null)
            {
                Debug.Log("UitbouwRestriction.cs > IsInPerceel > uitbouwFootprint is null");
                return false;
            }
            else if (perceel == null)
            {
                Debug.Log("UitbouwRestriction.cs > IsInPerceel > perceel is null");
                return false;
            }
            else if (uitbouwPositionOffset == null)
            {
                Debug.Log("UitbouwRestriction.cs > IsInPerceel > uitbouwPositionOffset is null");
                return false;
            }

            var q = from i in perceel
                    from p in i
                    select CoordConvert.RDtoUnity(p) into v3
                    select new Vector2(v3.x, v3.z);

            var polyPoints = q.ToArray(); //todo: test for non-contiguous perceels

            foreach (var vert in uitbouwFootprint)
            {
                if (!GeometryCalculator.ContainsPoint(polyPoints, vert + new Vector2(uitbouwPositionOffset.x, uitbouwPositionOffset.z)))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class AttachedToWallRestriction : UitbouwRestriction
    {
        public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            if (ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall)
                return building.SelectedWall.WallIsSelected;
            else
                return uitbouw != null;
        }
    }

    //public class AttachedToWallRestriction : UitbouwRestriction
    //{
    //    public bool ConformsToRestriction(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
    //    {
    //        return building.SelectedWall.WallIsSelected || !ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall;
    //    }
    //}

    public enum UitbouwRestrictionType
    {
        None,
        Monument,
        Beschermd,
        Height,
        Depth,
        Area,
        PerceelBounds,
        AttachedToWall,
        Width,
    }

    public static class RestrictionChecker
    {
        public static BuildingMeshGenerator ActiveBuilding => MetadataLoader.Building;
        public static PerceelRenderer ActivePerceel => MetadataLoader.Perceel;
        public static UitbouwBase ActiveUitbouw => MetadataLoader.Uitbouw;

        private static IDictionary<UitbouwRestrictionType, UitbouwRestriction> activeRestrictions = new Dictionary<UitbouwRestrictionType, UitbouwRestriction>
        {
            {UitbouwRestrictionType.Monument, new MonumentRestriction() },
            {UitbouwRestrictionType.Beschermd, new BeschermdRestriction() },
            {UitbouwRestrictionType.Height, new HeightRestriction() },
            {UitbouwRestrictionType.Depth, new DepthRestriction() },
            {UitbouwRestrictionType.Area, new PerceelAreaRestriction() },
            {UitbouwRestrictionType.PerceelBounds, new PerceelBoundsRestriction() },
            {UitbouwRestrictionType.AttachedToWall, new AttachedToWallRestriction() },
        };
        //public static IDictionary<UitbouwRestrictionType, UitbouwRestriction> ActiveRestrictions => activeRestrictions;

        public static UitbouwRestriction GetRestriction(UitbouwRestrictionType type)
        {
            if (activeRestrictions.ContainsKey(type))
            {
                return activeRestrictions[type];
            }
            return null;
        }

        public static bool ConformsToAllRestrictions(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            foreach (var restriction in activeRestrictions)
            {
                if (!restriction.Value.ConformsToRestriction(building, perceel, uitbouw))
                {
                    return false;
                }
            }
            return true;
        }

        public static IDictionary<UitbouwRestrictionType, UitbouwRestriction> NonConformingRestrictions(BuildingMeshGenerator building, PerceelRenderer perceel, UitbouwBase uitbouw)
        {
            var nonConformingRestrictions = new Dictionary<UitbouwRestrictionType, UitbouwRestriction>();
            foreach (var restriction in activeRestrictions)
            {
                if (!restriction.Value.ConformsToRestriction(building, perceel, uitbouw))
                {
                    nonConformingRestrictions.Add(restriction.Key, restriction.Value);
                }
            }
            return nonConformingRestrictions;
        }
    }
}
