using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using T3D.Uitbouw;
using UnityEngine;

[RequireComponent(typeof(BoundaryFeature))]
public class BoundaryFeatureCityObject : CityObject
{
    public override CitySurface[] GetSurfaces()
    {
        var bf = GetComponent<BoundaryFeature>();

        SetParents(new CityObject[] {
            RestrictionChecker.ActiveUitbouw.GetComponent<CityObject>()
            });

        return new CitySurface[] { bf.Surface };
    }
}
