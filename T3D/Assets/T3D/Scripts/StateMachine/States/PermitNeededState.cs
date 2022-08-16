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

    [SerializeField]
    private RectTransform checkersPanel;
    private float checkersPanelDefaultHeight;
    private RestrictionCheckerDisabler[] checkers;

    protected override void Awake()
    {
        base.Awake();
        checkersPanelDefaultHeight = checkersPanel.sizeDelta.y;
    }

    public override void StateEnteredAction()
    {
        //check if restrictions fail
        //var failedRestrictions = RestrictionChecker.NonConformingRestrictions(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);
        conformsToAllRestrictions = RestrictionChecker.ConformsToAllRestrictions(RestrictionChecker.ActiveBuilding, RestrictionChecker.ActivePerceel, RestrictionChecker.ActiveUitbouw);

        noPermitNeededPanel.SetActive(conformsToAllRestrictions);
        permitNeededPanel.SetActive(!conformsToAllRestrictions);
        var buttonText = conformsToAllRestrictions ? noPermitNeededButtonText : permitNeededButtonText;
        SetButtonText(buttonText);

        checkers = GetComponentsInChildren<RestrictionCheckerDisabler>(true);
        float newY = checkersPanelDefaultHeight;
        foreach (var checker in checkers)
        {
            checker.UpdateUI();
            newY -= checker.SizeMinus;

        }
        checkersPanel.sizeDelta = new Vector2(checkersPanel.sizeDelta.x, newY);
    }

    public override int GetDesiredStateIndex()
    {
        //nullcheck is needed for OnValidate() in the base class for in the editor
        if (ServiceLocator.GetService<T3DInit>().HTMLData.HasSubmitted) //if this is a submitted request, go to the view state after all steps that reconstruct the request.
        {
            return 2;
        }
        //if this is not a submitted request, check if all restriction requirements are met, if so send the request and go to the success step. if not, ask for additional data in the next step
        else if (conformsToAllRestrictions)
        {
            return 1; //send + success step
        }
        else
        {
            return 0; //metadata/submission step
        }
    }

    private void SetButtonText(string newText)
    {
        nextButton.GetComponentInChildren<Text>().text = newText;
    }
}
