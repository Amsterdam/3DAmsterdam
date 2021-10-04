using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class InfoHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, TextArea]
        private string conformsText;
        [SerializeField, TextArea]
        private string exceedsText;
        [SerializeField]
        private GameObject popupPrefab;
        private GameObject popup;

        [SerializeField]
        private UitbouwRestrictionType restrictionType;
        private UitbouwRestriction restrction;

        private void Awake()
        {
            RestrictionChecker.ActiveRestrictions.TryGetValue(restrictionType, out restrction);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            bool conforms = restrction == null || restrction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);
            CreatePopup(conforms);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Destroy(popup);
        }

        void CreatePopup(bool conformsToRestriction)
        {
            popup = Instantiate(popupPrefab, transform.position, transform.rotation, transform);

            switch (restrictionType)
            {
                case UitbouwRestrictionType.Height:
                    conformsText = string.Format(conformsText, HeightRestriction.MaxHeight, RestrictionChecker.ActiveUitbouw.Height);
                    exceedsText = string.Format(exceedsText, HeightRestriction.MaxHeight, (RestrictionChecker.ActiveUitbouw.Height - HeightRestriction.MaxHeight).ToString("F2"));
                    break;
                case UitbouwRestrictionType.Depth:
                    conformsText = string.Format(conformsText,  DepthRestriction.MaxDepth, RestrictionChecker.ActiveUitbouw.Depth);
                    exceedsText = string.Format(exceedsText, DepthRestriction.MaxDepth, (RestrictionChecker.ActiveUitbouw.Depth - DepthRestriction.MaxDepth).ToString("F2"));
                    break;
                case UitbouwRestrictionType.Area:
                    var freeArea = RestrictionChecker.ActivePerceel.Area - RestrictionChecker.ActiveBuilding.Area;
                    var maxAvailableArea = freeArea * PerceelAreaRestriction.MaxAreaFraction;

                    conformsText = string.Format(conformsText, PerceelAreaRestriction.MaxAreaPercentage, freeArea.ToString("F2"), RestrictionChecker.ActiveUitbouw.Area.ToString("F2"));
                    exceedsText = string.Format(exceedsText, PerceelAreaRestriction.MaxAreaPercentage, freeArea.ToString("F2"), (RestrictionChecker.ActiveUitbouw.Area - maxAvailableArea).ToString("F2"));
                    break;
            }
            popup.GetComponentInChildren<PopupInfo>().SetText(conformsToRestriction ? conformsText : exceedsText);
        }
    }
}