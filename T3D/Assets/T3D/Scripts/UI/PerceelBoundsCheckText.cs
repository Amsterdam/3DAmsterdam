using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class PerceelBoundsCheckText : MonoBehaviour
    {
        public Text text;
        public Button Volgende;

        private void Update()
        {
            var perceelBoundsRestriction = RestrictionChecker.GetRestriction(UitbouwRestrictionType.PerceelBounds);
            var inPerceelBounds = perceelBoundsRestriction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);

            //Volgende.interactable = inPerceelBounds && ServiceLocator.GetService<T3DInit>().IsEditMode;

            SetUitbouwText(inPerceelBounds);
        }

        private void SetUitbouwText(bool inPerceelBounds)
        {
            text.text = inPerceelBounds ? "Ja" : "Nee";
        }
    }
}
