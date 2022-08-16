using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class RestrictionCheckerUI : MonoBehaviour
    {
        [SerializeField]
        private UitbouwRestrictionType restrictionType;
        private UitbouwRestriction restrction;
        private Image image;
        [SerializeField]
        private Sprite conformsSprite;
        [SerializeField]
        private Sprite exceedsSprite;

        private void Awake()
        {
            restrction = RestrictionChecker.GetRestriction(restrictionType);
            image = GetComponent<Image>();
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (restrction.ConformsToRestriction(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw))
            {
                image.sprite = conformsSprite;
            }
            else
            {
                image.sprite = exceedsSprite;
            }
        }
    }
}