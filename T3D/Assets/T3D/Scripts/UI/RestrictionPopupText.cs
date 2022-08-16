using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class RestrictionPopupText : InfoHoverButton
    {
        [SerializeField, TextArea]
        private string conformsText;
        [SerializeField, TextArea]
        private string exceedsText;

        [SerializeField]
        private UitbouwRestrictionType restrictionType;
        private UitbouwRestriction restriction;

        private void Awake()
        {
            //RestrictionChecker.ActiveRestrictions.TryGetValue(restrictionType, out restriction);
            restriction = RestrictionChecker.GetRestriction(restrictionType);
        }

        protected override void CreatePopup()
        {
            bool conforms = restriction == null || restriction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);
            //Debug.Log(gameObject.name + "\t" + conforms, gameObject);
            CreatePopup(conforms);
        }

        void CreatePopup(bool conformsToRestriction)
        {
            popup = Instantiate(popupPrefab, transform.position, transform.rotation, GetComponentInParent<State>().transform);

            string passedText = conformsText;
            string failedText = exceedsText;

            switch (restrictionType)
            {
                case UitbouwRestrictionType.Height:
                    passedText = string.Format(conformsText, HeightRestriction.MaxHeight, RestrictionChecker.ActiveUitbouw.Height.ToString("F2"));
                    failedText = string.Format(exceedsText, HeightRestriction.MaxHeight, (RestrictionChecker.ActiveUitbouw.Height - HeightRestriction.MaxHeight).ToString("F2"));
                    break;
                case UitbouwRestrictionType.Depth:
                    passedText = string.Format(conformsText, DepthRestriction.MaxDepth, RestrictionChecker.ActiveUitbouw.Depth.ToString("F2"));
                    failedText = string.Format(exceedsText, DepthRestriction.MaxDepth, (RestrictionChecker.ActiveUitbouw.Depth - DepthRestriction.MaxDepth).ToString("F2"));
                    break;
                case UitbouwRestrictionType.Area:
                    var freeArea = RestrictionChecker.ActivePerceel.Area - RestrictionChecker.ActiveBuilding.Area;
                    var perc = (RestrictionChecker.ActiveUitbouw.Area / freeArea) * 100;

                    passedText = string.Format(conformsText, PerceelAreaRestriction.MaxAreaPercentage, perc.ToString("F2"));
                    failedText = string.Format(exceedsText, PerceelAreaRestriction.MaxAreaPercentage, perc.ToString("F2"));
                    break;
            }
            popup.GetComponentInChildren<PopupInfo>().SetText(conformsToRestriction ? passedText : failedText);
        }
    }
}