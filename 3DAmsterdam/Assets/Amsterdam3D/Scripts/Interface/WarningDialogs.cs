using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningDialogs : MonoBehaviour
{
    public static WarningDialogs Instance;

    [SerializeField]
    private Warning warningPrefab;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Spawns a new warning prefab with a message. Leave blank for default server warning.
    /// </summary>
    /// <param name="message">Optional custom message</param>
    public void ShowNewDialog(string message = "")
    {
        var bodyText = Instantiate(warningPrefab, this.transform);
        bodyText.SetMessage(message);
    }
}
