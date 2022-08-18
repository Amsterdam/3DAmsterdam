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

        private bool conforms;
        public bool Conforms => conforms;

        [SerializeField]
        private float sizeMinusWhenDisabled;
        public float SizeMinus => conforms ? sizeMinusWhenDisabled : 0;

        private void Awake()
        {
            restriction = RestrictionChecker.GetRestriction(restrictionType);
        }

        private void Update()
        {
            UpdateUI();
        }

        public void UpdateUI()
        {
            conforms = restriction == null || restriction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);
            gameObject.SetActive(!conforms);
        }
    }
}