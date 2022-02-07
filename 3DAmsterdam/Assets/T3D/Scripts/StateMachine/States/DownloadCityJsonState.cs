using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadCityJsonState : State
{
    private string userNameKey;
    private SaveableString userName;
    [SerializeField] private InputField nameInputField;

    private string userMailKey;
    private SaveableString userMail;
    [SerializeField] private InputField mailInputField;

    private string userCommentsKey;
    private SaveableString userComments;
    [SerializeField] private InputField commentsInputField;

    private string userPermissionKey;
    private SaveableBool userPermission;
    [SerializeField] private Toggle permissionToggle;

    protected override void Awake()
    {
        base.Awake();

        userNameKey = GetType().ToString() + ".userName";
        userMailKey = GetType().ToString() + ".userMail";
        userCommentsKey = GetType().ToString() + ".userComments";
        userPermissionKey = GetType().ToString() + ".userPermission";

        userName = new SaveableString(userNameKey);
        userMail = new SaveableString(userMailKey);
        userComments = new SaveableString(userCommentsKey);
        userPermission = new SaveableBool(userPermissionKey);
    }

    public override void StateCompletedAction()
    {
        userName.SetValue(nameInputField.text);
        userMail.SetValue(mailInputField.text);
        userComments.SetValue(commentsInputField.text);
        userPermission.SetValue(permissionToggle.isOn);

        SessionSaver.ExportSavedData();
    }
}
