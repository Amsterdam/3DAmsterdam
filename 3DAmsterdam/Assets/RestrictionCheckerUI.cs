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
            restrction = RestrictionChecker.ActiveRestrictions[restrictionType];
            image = GetComponent<Image>();
        }

        private void Start()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (restrction.ConformsToRestriction(HandleMetaDataUpdates.Building, HandleMetaDataUpdates.Perceel, HandleMetaDataUpdates.Uitbouw))
            {
                image.sprite = conformsSprite;
            }
            else
            {
                print(HandleMetaDataUpdates.Uitbouw.Depth);
                image.sprite = exceedsSprite;
            }
        }
    }
}