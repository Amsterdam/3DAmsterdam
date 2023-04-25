using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static ProfileDownloadOption;
using System.Linq;

public class DownloadProfileImage : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown dropdown;

    [SerializeField]
    private ProfileDownloadEntry[] entries;

    private ProfileDownloadEntry currentSelect;

    // Start is called before the first frame update
    void Start()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(entries.Select(x => x.name).ToList());

        currentSelect = entries[0];
    }

    public void InvokeEntry()
    {
        currentSelect.trigger.InvokeStarted();
    }
}
