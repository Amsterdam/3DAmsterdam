using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class RestrictionCheckerDisabler : MonoBehaviour
    {

        [SerializeField]
        private UitbouwRestrictionType restrictionType;
        private UitbouwRestriction restriction;


        private void Awake()
        {
            restriction = RestrictionChecker.GetRestriction(restrictionType);
        }

        private void Start()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (restriction == null || restriction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw))
                gameObject.SetActive(false);
        }
    }
}