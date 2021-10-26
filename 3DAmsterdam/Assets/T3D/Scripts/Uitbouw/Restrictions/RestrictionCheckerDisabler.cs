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
        private UitbouwRestriction restrction;


        private void Awake()
        {
            restrction = RestrictionChecker.ActiveRestrictions[restrictionType];
        }

        private void Start()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (restrction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw))
                gameObject.SetActive(false);
        }
    }
}