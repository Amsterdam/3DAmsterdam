using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class DisplayUitbouwText : MonoBehaviour
    {
        [SerializeField]
        private Text widthText;
        [SerializeField]
        private Text heightText;
        [SerializeField]
        private Text depthText;
        [SerializeField]
        private Text areaText;
        [SerializeField]
        private Text perceelBoundsText;

        private void Update()
        {
            var perceelBoundsRestriction = RestrictionChecker.ActiveRestrictions[UitbouwRestrictionType.PerceelBounds];
            var inPerceelBounds = perceelBoundsRestriction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);

            SetUitbouwText(RestrictionChecker.ActiveUitbouw.Width, RestrictionChecker.ActiveUitbouw.Height, RestrictionChecker.ActiveUitbouw.Depth, RestrictionChecker.ActiveUitbouw.Area, inPerceelBounds);
        }

        private void SetUitbouwText(float width, float height, float depth, float area, bool inPerceelBounds)
        {
            widthText.text = $"{width.ToString("F2")} m";
            heightText.text = $"{height.ToString("F2")} m";
            depthText.text = $"{depth.ToString("F2")} m";
            areaText.text = $"{area.ToString("F2")} m²";
            perceelBoundsText.text = inPerceelBounds ? "Ja" : "Nee";
        }
    }
}
