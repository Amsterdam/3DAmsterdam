using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmitPermitRequestStateSaveDataContainer : SaveDataContainer
{
    public string UserName;
    public string UserMail;
    public string UserComments;
}

public class SubmitPermitRequestState : State
{
    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField mailInputField;
    [SerializeField] private InputField commentsInputField;

    public static string UserName = "onbekend";// => userMail.Value; //todo: save this info somewhere instead of static variables
    public static string UserMail;// => userMail.Value; //todo: save this info somewhere instead of static variables
    private SubmitPermitRequestStateSaveDataContainer saveData;

    protected override void Awake()
    {
        base.Awake();

        saveData = new SubmitPermitRequestStateSaveDataContainer();

        nameInputField.text = saveData.UserName;
        mailInputField.text = saveData.UserMail;
        commentsInputField.text = saveData.UserComments;
    }

    public override int GetDesiredStateIndex()
    {
        if (ServiceLocator.GetService<T3DInit>().HTMLData != null && ServiceLocator.GetService<T3DInit>().HTMLData.HasSubmitted)
            return 1;
        return 0;
    }

    public override void StateCompletedAction()
    {
        saveData.UserName = nameInputField.text;
        UserName = saveData.UserName;
        saveData.UserMail = mailInputField.text;
        UserMail = saveData.UserMail;
        saveData.UserComments = commentsInputField.text;
        //userPermission.SetValue(permissionToggle.isOn);
    }
}
