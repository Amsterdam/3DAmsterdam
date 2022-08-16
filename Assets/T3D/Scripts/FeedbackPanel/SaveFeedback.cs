using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveFeedback : MonoBehaviour
{
    public enum SaveStatus { ChangesSaved, Saving, WaitingToSave }

    [SerializeField]
    private Text feedbackText;
    [SerializeField]
    private Image busyImage;

    private void Start()
    {
        if (!ServiceLocator.GetService<T3DInit>().IsEditMode)
            gameObject.SetActive(false);
    }

    public void SetSaveStatus(SaveStatus status)
    {
        switch (status)
        {
            case SaveStatus.ChangesSaved:
                feedbackText.text = "";
                busyImage.enabled = false;
                break;
            case SaveStatus.Saving:

                feedbackText.text = "Opslaan...";
                busyImage.enabled = true;
                break;
            case SaveStatus.WaitingToSave:
                feedbackText.text = "";
                busyImage.enabled = false;
                break;
        }
    }
}
