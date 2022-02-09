using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.UI;

public class PermitNeededState : State
{
    [SerializeField]
    private GameObject noPermitNeededPanel;
    [SerializeField]
    private GameObject permitNeededPanel;

    private bool conformsToAllRestrictions;

    [SerializeField]
    private Button nextButton;
    [SerializeField]
    private string noPermitNeededButtonText;
    [SerializeField]
    private string permitNeededButtonText;

    public override void StateEnteredAction()
    {
        //check if restrictions fail
        //var failedRestrictions = RestrictionChecker.NonConformingRestrictions(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);
        conformsToAllRestrictions = RestrictionChecker.ConformsToAllRestrictions(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);

        noPermitNeededPanel.SetActive(conformsToAllRestrictions);
        permitNeededPanel.SetActive(!conformsToAllRestrictions);
        var buttonText = conformsToAllRestrictions ? noPermitNeededButtonText : permitNeededButtonText;
        SetButtonText(buttonText);
    }

    public override int GetDesiredStateIndex()
    {
        if (conformsToAllRestrictions)
        {
            return -1; //no next step
        }
        else
        {
            return 0; //metadata/submission step
        }
    }

    private void SetButtonText(string newText)
    {
        nextButton.GetComponentInChildren<Text>().text = newText;
        //var text = nextButton.GetComponentInChildren<Text>();
        //text.text = newText;
        //var rectTransform = nextButton.GetComponent<RectTransform>();
        //var width = text.preferredWidth;
        //rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
    }
}
