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
            popup.GetComponentInChildren<PopupInfo>().SetText(conformsToRestriction ? conformsText : exceedsText);
        }
    }
}
