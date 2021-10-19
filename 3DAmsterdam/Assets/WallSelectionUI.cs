using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.T3D.Uitbouw
{
    public class WallSelectionUI : MonoBehaviour
    {
        private BuildingMeshGenerator building;
        [SerializeField]
        private GameObject nextStep_U;
        [SerializeField]
        private GameObject NextStep_D;
        [SerializeField]
        private Button nextButton;

        private void Start()
        {
            building = RestrictionChecker.ActiveBuilding;
            building.SelectedWall.AllowSelection = true;

            nextButton.interactable = false;
        }

        private void Update()
        {
            if (building.SelectedWall.WallIsSelected)
            {
                nextButton.interactable = true;
            }
        }

        //called by the nextButton's event handler in the inspector
        public void CompleteStep()
        {
            building.SelectedWall.AllowSelection = false;
            MetadataLoader.Instance.PlaatsUitbouw();

            if (MetadataLoader.Instance.UploadedModel)
            {
                nextStep_U.SetActive(true);
            }
            else
            {
                NextStep_D.SetActive(true);
            }

            gameObject.SetActive(false);
        }
    }
}