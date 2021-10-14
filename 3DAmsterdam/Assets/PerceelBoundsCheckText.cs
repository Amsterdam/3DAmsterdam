using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class PerceelBoundsCheckText : MonoBehaviour
    {
        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void Update()
        {
            var perceelBoundsRestriction = RestrictionChecker.ActiveRestrictions[UitbouwRestrictionType.PerceelBounds];
            var inPerceelBounds = perceelBoundsRestriction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);

            SetUitbouwText(inPerceelBounds);
        }

        private void SetUitbouwText(bool inPerceelBounds)
        {
            text.text = inPerceelBounds ? "Ja" : "Nee";
        }
    }
}
