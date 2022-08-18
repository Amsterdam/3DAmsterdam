using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using T3D.Uitbouw;
using UnityEngine;
namespace Netherlands3D.T3D.Uitbouw
{
    [RequireComponent(typeof(UitbouwBase))]
    public class ShapableUitbouwCityObject : CityObject
    {
        protected override void Start()
        {
            base.Start();
            var building = GetComponent<UitbouwBase>().ActiveBuilding;
            SetParents(new CityObject[] {
                building.GetComponent<CityObject>()
            });
        }

        public override CitySurface[] GetSurfaces()
        {
            List<CitySurface> citySurfaces = new List<CitySurface>();
            var walls = GetComponentsInChildren<UitbouwMuur>();
            foreach (var wall in walls)
            {
                citySurfaces.Add(wall.Surface);
            }

            var boundaryFeatures = GetComponentsInChildren<BoundaryFeature>();
            foreach (var bf in boundaryFeatures)
            {
                citySurfaces.Add(bf.Surface);
            }

            return citySurfaces.ToArray();
        }
    }
}